// File: src/Systems/PaletteBuilder.cs
//
//   Build/Register the clickable ARTZone tools in RoadsServices palette:
//     - Find a "donor" tile in RoadsServices (ex: Wide Sidewalk / Crosswalk).
//     - Clone donor once per ToolDefinition.
//     - Give clone our icon, ID, and bumped priority (+1) so it shows up next to Wide Sidewalk.
//     - Attach NetUpgrade so the clone acts like a vanilla tool button.
//     - Hook the clone to the correct ToolBaseSystem so clicking it activates our tool.
//     - After the map finishes loading, copy the donor's PlaceableNetData and apply flags
//        from the ToolDefinition (underground allowed, etc.).
//
// Donor selection logic:
//   - Try hard-coded donors first ("Wide Sidewalk", then "Crosswalk").
//   - If both are missing (e.g., patch day), reflection-scan PrefabSystem for the closest RoadsServices match.
//   - We log when we have to do the fallback so release builds report breakage.
//
// Notes:
//   - No Harmony. Only read PrefabSystem via reflection to find a 3rd fallback donor.
//   - Never modify the donor prefab itself; always DuplicatePrefab()
//   - PaletteBootstrapSystem controls WHEN this runs; PaletteBuilder just DOES the build.

namespace ARTZone.Systems
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using ARTZone.Tools;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Net;
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Tools;
    using Unity.Entities;
    using UnityEngine;

    public static class PaletteBuilder
    {
        // --- EASY KNOB --------------------------------------------------------
        // How far after the donor we insert our clone in the RoadsServices group.
        // 1 = right after donor. 0 = same slot (don't do that).
        private const int kTilePriorityOffset = 1;

        // --- REGISTRATION STATE ----------------------------------------------
        // All registered ARTZone tools. One ToolDefinition = one palette tile.
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(4);

        public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
        public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

        // Map from ToolDefinition to its clone prefab + UIObject.
        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_ToolsLookup = new(4);

        // Cached systems.
        private static World? s_World;
        private static PrefabSystem? s_PrefabSystem;

        // Cached donor (the RoadsServices tile we clone)
        private static PrefabBase? s_DonorPrefab;
        private static UIObject? s_DonorUI;

        // Whether we've already created the clones.
        private static bool s_Instantiated;

        public static bool IsReady => s_Instantiated && s_ToolsLookup.Count > 0;

        // --- DEBUG LOG --------------------------------------------------------
        [Conditional("DEBUG")]
        private static void Dbg(string message)
        {
            var log = ARTZoneMod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info(message);
            }
            catch { }
        }

        // --- Init -------------------------------------------------------------
        // Called from ARTZoneMod.OnLoad(). Clears caches so hot reload works
        public static void Initialize(bool force = false)
        {
            if (!force && s_World != null)
                return;

            ToolDefinitions = new(4);
            s_ToolsLookup.Clear();
            s_Instantiated = false;

            s_DonorPrefab = null;
            s_DonorUI = null;

            s_World = World.DefaultGameObjectInjectionWorld;
            s_PrefabSystem = s_World != null
                ? s_World.GetExistingSystemManaged<PrefabSystem>()
                : null;
        }

        // Register a tool so it can get a palette tile
        // Safe to call multiple times; rejects invalid/duplicate entries.
        public static void RegisterTool(ToolDefinition def)
        {
            // Guard: type must actually be a ToolBaseSystem to activate it when clicked
            if (def.Type == null || !typeof(ToolBaseSystem).IsAssignableFrom(def.Type))
            {
                ARTZoneMod.s_Log.Error("[ART][Palette] RegisterTool: Type must inherit ToolBaseSystem.");
                return;
            }

            if (string.IsNullOrWhiteSpace(def.ToolID))
            {
                ARTZoneMod.s_Log.Error("[ART][Palette] RegisterTool: ToolID must be non-empty.");
                return;
            }

            if (HasTool(def) || HasTool(def.ToolID))
            {
                ARTZoneMod.s_Log.Error($"[ART][Palette] RegisterTool: \"{def.ToolID}\" already registered.");
                return;
            }

            Dbg($"[ART][Palette] Register \"{def.ToolID}\" for {def.Type.Name}");
            ToolDefinitions.Add(def);
        }

        // --- Build Clones -----------------------------------------------------
        // Build the actual clickable tiles:
        //   - clone donor, set icon - priority - group
        //   - add NetUpgrade (so it's treated like a vanilla upgrade tool button)
        //   - hook the clone to the correct ToolBaseSystem
        // PaletteBootstrapSystem calls this once the game world is initialized
        public static void InstantiateTools(bool logIfNoDonor = true)
        {
            if (s_Instantiated)
                return;

            s_World ??= World.DefaultGameObjectInjectionWorld;
            s_PrefabSystem ??= s_World != null
                ? s_World.GetExistingSystemManaged<PrefabSystem>()
                : null;

            if (s_PrefabSystem == null)
            {
                ARTZoneMod.s_Log.Error("[ART][Palette] PrefabSystem not available.");
                return;
            }

            // Find (or reuse cached) donor.
            if ((s_DonorPrefab == null || s_DonorUI == null) &&
                !TryResolveDonor(s_PrefabSystem, out s_DonorPrefab, out s_DonorUI))
            {
                if (logIfNoDonor)
                {
                    ARTZoneMod.s_Log.Error("[ART][Palette] Could not find RoadsServices donor. Will retry next frame.");
                }
                return; // PaletteBootstrapSystem will call again until it gives up
            }

            var donorPrefab = s_DonorPrefab!;
            var donorUI = s_DonorUI!;

            Dbg($"[ART][Palette] Creating tiles. Count={ToolDefinitions.Count}");

            foreach (var def in ToolDefinitions)
            {
                try
                {
                    // 1 - Clone donor. This becomes our clickable tile.
                    var clonePrefab = s_PrefabSystem.DuplicatePrefab(donorPrefab, def.ToolID);

                    // 2 - Remove donor-only parts not needed
                    if (clonePrefab.Has<Unlockable>())
                        clonePrefab.Remove<Unlockable>();
                    if (clonePrefab.Has<NetSubObjects>())
                        clonePrefab.Remove<NetSubObjects>();

                    // 3 - Make a fresh UIObject for the clone
                    var cloneUI = ScriptableObject.CreateInstance<UIObject>();
                    cloneUI.name = def.ToolID;
                    cloneUI.m_Icon = def.Ui.ImagePath;  // icon path in coui://ui-mods
                    cloneUI.m_IsDebugObject = donorUI.m_IsDebugObject;
                    cloneUI.m_Group = donorUI.m_Group;  // "RoadsServices"
                    cloneUI.active = donorUI.active;
                    // Priority: donor priority + offset; place custom icon to right of the donor tile
                    cloneUI.m_Priority = donorUI.m_Priority + kTilePriorityOffset;

                    clonePrefab.AddComponentFrom(cloneUI);

                    // 4 - NetUpgrade marks this clone as a "tool selector"
                    //     Needed so the game treats it like a vanilla upgrade tool button
                    var netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    clonePrefab.AddComponentFrom(netUpgrade);

                    // 5 - Tell PrefabSystem the clone’s components changed
                    s_PrefabSystem.UpdatePrefab(clonePrefab);

                    // 6 - Hook clone to the actual ToolBaseSystem so clicking the tile activates our tool
                    var toolSystem = s_World!.GetOrCreateSystemManaged(def.Type) as ToolBaseSystem;
                    if (toolSystem == null || !toolSystem.TrySetPrefab(clonePrefab))
                    {
                        ARTZoneMod.s_Log.Error($"[ART][Palette] Failed to attach prefab for \"{def.ToolID}\".");
                        continue;
                    }

                    s_ToolsLookup[def] = (clonePrefab, cloneUI);
                    Dbg($"[ART][Palette] Tile created for {def.ToolID}");
                }
                catch (Exception ex)
                {
                    ARTZoneMod.s_Log.Error($"[ART][Palette] Could not create tile for {def.ToolID}: {ex}");
                }
            }

            // After the map is fully loaded, finalize placement data for any clone
            // This wires up flags like UndergroundUpgrade, set/unset flags, etc.
            if (GameManager.instance != null)
            {
                GameManager.instance.onGameLoadingComplete -= ApplyPlacementDataAfterLoad;
                GameManager.instance.onGameLoadingComplete += ApplyPlacementDataAfterLoad;
            }

            s_Instantiated = true;
        }

        // --- Placement Data ---------------------------------------------------
        // After the save loads, copy donor PlaceableNetData onto any clone,
        // then tweak it using data from each ToolDefinition
        private static void ApplyPlacementDataAfterLoad(Purpose purpose, GameMode mode)
        {
            if (s_PrefabSystem == null || s_DonorPrefab == null)
            {
                ARTZoneMod.s_Log.Error("[ART][Palette] ApplyPlacementDataAfterLoad: missing PrefabSystem or donor.");
                return;
            }

            foreach (var kv in s_ToolsLookup)
            {
                var def = kv.Key;
                var clonePair = kv.Value;

                try
                {
                    // Start from donor's PlaceableNetData
                    PlaceableNetData baseData;
                    if (!s_PrefabSystem.TryGetComponentData(s_DonorPrefab, out baseData))
                        baseData = default;

                    // Apply tool-specific flags.
                    baseData.m_SetUpgradeFlags = def.SetFlags;
                    baseData.m_UnsetUpgradeFlags = def.UnsetFlags;
                    baseData.m_PlacementFlags = def.PlacementFlags;

                    if (def.Underground)
                        baseData.m_PlacementFlags |= PlacementFlags.UndergroundUpgrade;

                    // Replace whatever the clone currently has
                    PlaceableNetData existing;
                    if (s_PrefabSystem.TryGetComponentData(clonePair.Prefab, out existing))
                        s_PrefabSystem.RemoveComponent<PlaceableNetData>(clonePair.Prefab);

                    s_PrefabSystem.AddComponentData(clonePair.Prefab, baseData);
                    Dbg($"[ART][Palette] Applied PlaceableNetData to {def.ToolID}");
                }
                catch (Exception ex)
                {
                    ARTZoneMod.s_Log.Error($"[ART][Palette] Could not apply PlaceableNetData for {def.ToolID}: {ex}");
                }
            }
        }

        // --- DONOR RESOLUTION -------------------------------------------------
        // Try to locate a donor in RoadsServices.
        // Returns true if we find BOTH:
        //   - a prefab, and its UIObject lives in "RoadsServices".
        //
        // Steps:
        //   1. If there is a cached donor, reuse it.
        //   2. Try known donors ("Wide Sidewalk", then "Crosswalk"). If one works, Stop immediately.
        //   3. If both fail (patch day, names changed), reflection-scan PrefabSystem
        //      to guess a best match. If fallback needed, then *also* log a Warn in Release to alert game change research needed.
        public static bool TryResolveDonor(PrefabSystem prefabSystem, out PrefabBase? donorPrefab, out UIObject? donorUI)
        {
            donorPrefab = null;
            donorUI = null;

            if (prefabSystem == null)
                return false;

            // 0 - Cached donor - cheap fast path
            if (s_DonorPrefab != null && s_DonorUI != null)
            {
                Dbg($"[ART][Palette] Cached donor: {s_DonorPrefab.name} group='{(s_DonorUI.m_Group != null ? s_DonorUI.m_Group.name : "(null)")}'");
                donorPrefab = s_DonorPrefab;
                donorUI = s_DonorUI;
                return true;
            }

            // 1 - Locked donor candidates (exact IDs).
            var locked = new (string typeName, string name)[]
            {
                ("FencePrefab", "Wide Sidewalk"),
                ("FencePrefab", "Crosswalk"),
            };

            for (int i = 0; i < locked.Length; i++)
            {
                var (typeName, name) = locked[i];
                var id = new PrefabID(typeName, name);

                PrefabBase? candidate;
                bool found = prefabSystem.TryGetPrefab(id, out candidate) && candidate != null;
                Dbg($"[ART][Palette] Probe {typeName}:{name}: {(found ? "FOUND" : "missing")}");

                if (!found)
                    continue;

                UIObject? candidateUI;
                bool hasUI = candidate!.TryGet(out candidateUI) && candidateUI != null;
                if (!hasUI)
                {
                    Dbg("  …found prefab but it has no UIObject → skip");
                    continue;
                }

                string groupName = candidateUI!.m_Group != null ? candidateUI.m_Group.name : "(null)";
                if (!string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase))
                {
                    Dbg($"  …UIObject group is '{groupName}', not 'RoadsServices' → skip");
                    continue;
                }

                Dbg($"[ART][Palette] Donor OK (locked): {name} group='{groupName}' priority={candidateUI.m_Priority}");

                s_DonorPrefab = candidate;
                s_DonorUI = candidateUI;
                donorPrefab = candidate;
                donorUI = candidateUI;
                return true; // we found a donor, stop here (no reflection)
            }

            // 2 - Fallback: reflection scan.
            // Log if it ends up here so users can report game patch breaks
            try
            {
                var allPrefabs = GetAllPrefabsUnsafe(prefabSystem);
                PrefabBase? bestP = null;
                UIObject? bestU = null;
                int bestScore = int.MinValue;

                foreach (var p in allPrefabs)
                {
                    if (p == null)
                        continue;

                    UIObject? uComp;
                    if (!p.TryGet(out uComp) || uComp == null)
                        continue;

                    string groupName = uComp.m_Group != null ? uComp.m_Group.name : "(null)";
                    if (!string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase))
                        continue;

                    int score = 0;
                    string n = p.name ?? string.Empty;

                    // Prefer tiles that *look* like vanilla road service upgrades.
                    if (n.IndexOf("Wide Sidewalk", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 1000;
                    else if (n.IndexOf("Crosswalk", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 900;
                    else if (n.IndexOf("Grass", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 800;

                    // Bias vanilla priority so high-priority tiles win ties.
                    score += uComp.m_Priority;

                    bool hasPlaceable = prefabSystem.TryGetComponentData(p, out PlaceableNetData _);
                    Dbg($"[ART][Palette] Scan {p.GetType().Name}:{n} score={score} group='{groupName}' hasPlaceable={hasPlaceable} priority={uComp.m_Priority}");

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestP = p;
                        bestU = uComp;
                    }
                }

                if (bestP != null && bestU != null)
                {
                    // PATCH-DAY WARNING (this logs in release too)
                    ARTZoneMod.s_Log.Warn("[ART][Palette] Fallback donor used via reflection. Likely a patch renamed RoadsServices tiles. Please report this!");

                    s_DonorPrefab = bestP;
                    s_DonorUI = bestU;
                    donorPrefab = bestP;
                    donorUI = bestU;

                    Dbg($"[ART][Palette] Donor OK (scan): {bestP.name} group='{(bestU.m_Group != null ? bestU.m_Group.name : "(null)")}'' priority={bestU.m_Priority}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                ARTZoneMod.s_Log.Error("[ART][Palette] Fallback donor scan failed: " + ex);
            }

            // Nothing yet. PaletteBootstrapSystem will retry next frame, up to a cap, then give up and log.
            return false;
        }

        // --- Fallback only: reflection scan --------------------------------------------------
        // Read PrefabSystem's private prefab list. Read-only and safe in release.
        private static List<PrefabBase> GetAllPrefabsUnsafe(PrefabSystem ps)
        {
            // Preferred path: private property "prefabs".
            try
            {
                var prop = typeof(PrefabSystem).GetProperty(
                    "prefabs",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    var enumerable = prop.GetValue(ps) as IEnumerable<PrefabBase>;
                    if (enumerable != null)
                        return new List<PrefabBase>(enumerable);
                }
            }
            catch { /* ignore and fall through */ }

            // Fallback path: known private field "m_Prefabs".
            try
            {
                var fi = typeof(PrefabSystem).GetField(
                    "m_Prefabs",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var list = fi != null
                    ? fi.GetValue(ps) as List<PrefabBase>
                    : null;

                return list != null
                    ? new List<PrefabBase>(list)
                    : new List<PrefabBase>(0);
            }
            catch
            {
                return new List<PrefabBase>(0);
            }
        }
    }
}
