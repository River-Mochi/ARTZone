// File: src/Systems/PanelBuilder.cs
//
//   Build/Register the clickable ARTZone tools in RoadsServices Panel:
//     - Find a donor button in RoadsServices (ex: Wide Sidewalk / Crosswalk).
//     - Clone donor once per ToolDefinition.
//     - Give clone our icon, ID, and bumped priority (+1) so it shows up next to Wide Sidewalk.
//     - Attach NetUpgrade so the clone acts like a vanilla tool button.
//     - Hook the clone to the correct ToolBaseSystem so clicking it activates our tool.
//     - After the map finishes loading, copy the donor's PlaceableNetData and apply flags
//       from the ToolDefinition (underground allowed, etc.).
//
// Donor selection logic (FINAL):
//   - In RELEASE builds: try “Wide Sidewalk”, then “Crosswalk”. No reflection fallback.
//   - In DEBUG builds: same as release, then reflection scan as a last resort (dev aid).
//
// Notes:
//   - Never modify the donor prefab itself; always DuplicatePrefab().
//   - PanelBootStrapSystem controls WHEN this runs; PanelBuilder just DOES the build.

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

    public static class PanelBuilder
    {
        // --- KNOBS ------------------------------------------------------------
        // Insert our clone to the right of the donor.
        private const int TilePriorityOffset = 1;

        // --- REGISTRATION STATE ----------------------------------------------
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(4);

        public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
        public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

        // Map from ToolDefinition to its clone prefab + UIObject.
        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_ToolsLookup = new(4);

        // Cached systems.
        private static World? s_World;
        private static PrefabSystem? s_PrefabSystem;

        // Cached donor (the RoadsServices button we clone)
        private static PrefabBase? s_DonorPrefab;
        private static UIObject? s_DonorUI;

        // Are clones already created?
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
        // Called from ARTZoneMod.OnLoad(). Clears caches so hot reload works.
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

        // Register a tool so it can get a Panel button
        public static void RegisterTool(ToolDefinition def)
        {
            if (def.Type == null || !typeof(ToolBaseSystem).IsAssignableFrom(def.Type))
            {
                ARTZoneMod.s_Log.Error("[ART][Panel] RegisterTool: Type must inherit ToolBaseSystem.");
                return;
            }

            if (string.IsNullOrWhiteSpace(def.ToolID))
            {
                ARTZoneMod.s_Log.Error("[ART][Panel] RegisterTool: ToolID must be non-empty.");
                return;
            }

            if (HasTool(def) || HasTool(def.ToolID))
            {
                ARTZoneMod.s_Log.Error($"[ART][Panel] RegisterTool: \"{def.ToolID}\" already registered.");
                return;
            }

            Dbg($"[ART][Panel] Register \"{def.ToolID}\" for {def.Type.Name}");
            ToolDefinitions.Add(def);
        }

        // --- Build Clones -----------------------------------------------------
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
                ARTZoneMod.s_Log.Error("[ART][Panel] PrefabSystem not available.");
                return;
            }

            // Find (or reuse cached) donor
            if ((s_DonorPrefab == null || s_DonorUI == null) &&
                !TryResolveDonor(s_PrefabSystem, out s_DonorPrefab, out s_DonorUI))
            {
                if (logIfNoDonor)
                    ARTZoneMod.s_Log.Error("[ART][Panel] Could not find RoadsServices donor. Will retry next frame.");
                return; // PanelBootStrapSystem will call again
            }

            var donorPrefab = s_DonorPrefab!;
            var donorUI = s_DonorUI!;

            Dbg($"[ART][Panel] Creating buttons. Count={ToolDefinitions.Count}");

            foreach (var def in ToolDefinitions)
            {
                try
                {
                    // 1 - Clone donor. This becomes our clickable button.
                    var clonePrefab = s_PrefabSystem.DuplicatePrefab(donorPrefab, def.ToolID);

                    // 2 - Remove donor-only parts not needed
                    if (clonePrefab.Has<Unlockable>())
                        clonePrefab.Remove<Unlockable>();
                    if (clonePrefab.Has<NetSubObjects>())
                        clonePrefab.Remove<NetSubObjects>();

                    // 3 - Make a fresh UIObject for the clone
                    var cloneUI = ScriptableObject.CreateInstance<UIObject>();
                    cloneUI.name = def.ToolID;
                    cloneUI.m_Icon = def.Ui.ImagePath;             // icon path in coui://ui-mods
                    cloneUI.m_IsDebugObject = donorUI.m_IsDebugObject;
                    cloneUI.m_Group = donorUI.m_Group;              // "RoadsServices"
                    cloneUI.active = donorUI.active;
                    cloneUI.m_Priority = donorUI.m_Priority + TilePriorityOffset;

                    clonePrefab.AddComponentFrom(cloneUI);

                    // 4 - NetUpgrade marks this clone as a "tool selector" like vanilla buttons
                    var netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    clonePrefab.AddComponentFrom(netUpgrade);

                    // 5 - Let PrefabSystem re-index the clone’s components
                    s_PrefabSystem.UpdatePrefab(clonePrefab);

                    // 6 - Connect the clone to our ToolBaseSystem so clicking activates our tool
                    var toolSystem = s_World!.GetOrCreateSystemManaged(def.Type) as ToolBaseSystem;
                    bool attached = toolSystem != null && toolSystem.TrySetPrefab(clonePrefab);

                    if (!attached)
                    {
                        ARTZoneMod.s_Log.Error(
                            $"[ART][Panel] Failed to attach prefab for \"{def.ToolID}\" (toolSystem={(toolSystem?.GetType().Name ?? "null")})");
                        continue;
                    }

                    Dbg($"[ART][Panel] Button created and attached: {def.ToolID} → {toolSystem!.GetType().Name}");
                    s_ToolsLookup[def] = (clonePrefab, cloneUI);
                }
                catch (Exception ex)
                {
                    ARTZoneMod.s_Log.Error($"[ART][Panel] Could not create button for {def.ToolID}: {ex}");
                }
            }

            // After the map is fully loaded, finalize placement data for any clone
            if (GameManager.instance != null)
            {
                GameManager.instance.onGameLoadingComplete -= ApplyPlacementDataAfterLoad;
                GameManager.instance.onGameLoadingComplete += ApplyPlacementDataAfterLoad;
            }

            s_Instantiated = true;
        }

        // --- Placement Data ---------------------------------------------------
        private static void ApplyPlacementDataAfterLoad(Purpose purpose, GameMode mode)
        {
            if (s_PrefabSystem == null || s_DonorPrefab == null)
            {
                ARTZoneMod.s_Log.Error("[ART][Panel] ApplyPlacementDataAfterLoad: missing PrefabSystem or donor.");
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
                    Dbg($"[ART][Panel] Applied PlaceableNetData to {def.ToolID}");
                }
                catch (Exception ex)
                {
                    ARTZoneMod.s_Log.Error($"[ART][Panel] Could not apply PlaceableNetData for {def.ToolID}: {ex}");
                }
            }
        }

        // --- DONOR RESOLUTION -------------------------------------------------
        // RELEASE: try exact donors only. DEBUG: try donors then reflection scan.
        public static bool TryResolveDonor(PrefabSystem prefabSystem, out PrefabBase? donorPrefab, out UIObject? donorUI)
        {
            donorPrefab = null;
            donorUI = null;

            if (prefabSystem == null)
                return false;

            // Cached donor?
            if (s_DonorPrefab != null && s_DonorUI != null)
            {
                Dbg($"[ART][Panel] Cached donor: {s_DonorPrefab.name} group='{(s_DonorUI.m_Group != null ? s_DonorUI.m_Group.name : "(null)")}'");
                donorPrefab = s_DonorPrefab;
                donorUI = s_DonorUI;
                return true;
            }

            // 1) Hard-coded donors first (stable behavior in Release)
            if (TryGetExactDonor(prefabSystem, "FencePrefab", "Wide Sidewalk", out donorPrefab, out donorUI))
            {
                CacheDonor(donorPrefab!, donorUI!);
                return true;
            }
            if (TryGetExactDonor(prefabSystem, "FencePrefab", "Crosswalk", out donorPrefab, out donorUI))
            {
                CacheDonor(donorPrefab!, donorUI!);
                return true;
            }

#if DEBUG
            // 2) DEBUG ONLY — reflection scan (dev aid)
            if (TryReflectionDonor(prefabSystem, out donorPrefab, out donorUI))
            {
                ARTZoneMod.s_Log.Warn("[ART][Panel] Fallback donor used via reflection (DEBUG).");
                CacheDonor(donorPrefab!, donorUI!);
                return true;
            }
#endif
            return false;
        }

        private static void CacheDonor(PrefabBase p, UIObject u)
        {
            s_DonorPrefab = p;
            s_DonorUI = u;
            Dbg($"[ART][Panel] Donor selected: {p.name} (group='{(u.m_Group != null ? u.m_Group.name : "(null)")}', prio={u.m_Priority})");
        }

        private static bool TryGetExactDonor(PrefabSystem ps, string typeName, string name, out PrefabBase? donor, out UIObject? donorUI)
        {
            donor = null;
            donorUI = null;

            var id = new PrefabID(typeName, name);
            PrefabBase? candidate;
            bool found = ps.TryGetPrefab(id, out candidate) && candidate != null;
            Dbg($"[ART][Panel] Probe {typeName}:{name}: {(found ? "FOUND" : "missing")}");

            if (!found)
                return false;

            UIObject? ui;
            bool hasUI = candidate!.TryGet(out ui) && ui != null;
            if (!hasUI)
            {
                Dbg("  …found prefab but it has no UIObject → skip");
                return false;
            }

            string groupName = ui!.m_Group != null ? ui.m_Group.name : "(null)";
            if (!string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase))
            {
                Dbg($"  …UIObject group is '{groupName}', not 'RoadsServices' → skip");
                return false;
            }

            donor = candidate;
            donorUI = ui;
            return true;
        }

#if DEBUG
        // DEBUG-only: scan RoadsServices buttons to guess a reasonable donor.
        private static bool TryReflectionDonor(PrefabSystem ps, out PrefabBase? donor, out UIObject? donorUI)
        {
            donor = null;
            donorUI = null;

            try
            {
                var all = GetAllPrefabsUnsafe(ps);
                PrefabBase? bestP = null;
                UIObject? bestU = null;
                int bestScore = int.MinValue;

                foreach (var p in all)
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

                    if (n.IndexOf("Wide Sidewalk", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 1000;
                    else if (n.IndexOf("Crosswalk", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 900;
                    else if (n.IndexOf("Grass", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 800;

                    score += uComp.m_Priority;

                    Dbg($"[ART][Panel] Scan {p.GetType().Name}:{n} score={score} group='{groupName}' priority={uComp.m_Priority}");

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestP = p;
                        bestU = uComp;
                    }
                }

                if (bestP != null && bestU != null)
                {
                    donor = bestP;
                    donorUI = bestU;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ARTZoneMod.s_Log.Warn("[ART][Panel] Reflection donor scan failed (DEBUG): " + ex.Message);
            }
            return false;
        }

        // Reflection helper: get PrefabSystem's internal list (read-only).
        private static List<PrefabBase> GetAllPrefabsUnsafe(PrefabSystem ps)
        {
            try
            {
                var prop = typeof(PrefabSystem).GetProperty("prefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    var enumerable = prop.GetValue(ps) as IEnumerable<PrefabBase>;
                    if (enumerable != null)
                        return new List<PrefabBase>(enumerable);
                }
            }
            catch { /* ignore */ }

            try
            {
                var fi = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                var list = fi != null ? fi.GetValue(ps) as List<PrefabBase> : null;
                return list != null ? new List<PrefabBase>(list) : new List<PrefabBase>(0);
            }
            catch
            {
                return new List<PrefabBase>(0);
            }
        }
#endif
    }
}
