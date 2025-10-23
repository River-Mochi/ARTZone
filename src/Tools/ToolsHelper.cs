// File: src/Tools/ToolsHelper.cs
// Purpose: Duplicate a RoadsServices donor (Wide Sidewalk / Crosswalk / Grass) and create our tile
//          with the same UI group and priority+1. Donor prefab is never modified.
// Notes:
//   • Probes prefer NetUpgradePrefab, then fall back to PrefabBase.
//   • If probes miss, do a safe reflective scan of PrefabSystem to find a RoadsServices candidate.
//   • DuplicatePrefab + AddComponentFrom(UIObject, NetUpgrade) + UpdatePrefab to register ECS components.
//   • Icon path comes from definition.ui.ImagePath (single source of truth).
//   • Debug logging is null-safe and compiled out in Release.
//   • All nullable accesses are guarded; no CS860x warnings.

namespace AdvancedRoadTools.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using AdvancedRoadTools.Systems;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Net;
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Tools;
    using Unity.Entities;
    using UnityEngine;

    public static class ToolsHelper
    {
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(4);
        public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
        public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_ToolsLookup = new(4);

        private static bool s_Instantiated;
        private static World? s_World;
        private static PrefabSystem? s_PrefabSystem;

        private static PrefabBase? s_AnchorPrefab;
        private static UIObject? s_AnchorUI;

        // Allows bootstrapper to know when a tile has been created.
        public static bool IsReady => s_Instantiated && s_ToolsLookup.Count > 0;

        // Null-safe debug logger. Compiled out in Release.
#if DEBUG
        [Conditional("DEBUG")]
        private static void Dbg(string message)
        {
            var log = AdvancedRoadToolsMod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info(message);
            }
            catch { /* swallow early logger NREs */ }
        }
#else
        [Conditional("DEBUG")]
        private static void Dbg(string message) { }
#endif

        public static void Initialize(bool force = false)
        {
            if (!force && s_World != null)
                return;

            ToolDefinitions = new(4);
            s_ToolsLookup.Clear();
            s_Instantiated = false;

            s_World = World.DefaultGameObjectInjectionWorld;
            s_PrefabSystem = s_World != null ? s_World.GetExistingSystemManaged<PrefabSystem>() : null;
        }

        public static void RegisterTool(ToolDefinition toolDefinition)
        {
            if (HasTool(toolDefinition))
            {
                AdvancedRoadToolsMod.s_Log.Error($"Tool \"{toolDefinition.ToolID}\" already registered");
                return;
            }
#if DEBUG
            Dbg($"[ART][Tools] Registering tool \"{toolDefinition.ToolID}\" for {toolDefinition.Type.Name}");
#endif
            ToolDefinitions.Add(toolDefinition);
        }

        /// <summary>
        /// Duplicate the donor, attach our UIObject & NetUpgrade, and hand the prefab to the tool system.
        /// Safe to call repeatedly; if donor not yet found it logs and returns without throwing.
        /// </summary>
        public static void InstantiateTools(bool logIfNoAnchor = true)
        {
            if (s_Instantiated)
                return;

            s_World ??= World.DefaultGameObjectInjectionWorld;
            s_PrefabSystem ??= s_World != null ? s_World.GetExistingSystemManaged<PrefabSystem>() : null;

            if (s_PrefabSystem == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("[ART][Tools] PrefabSystem not available.");
                return;
            }

            if ((s_AnchorPrefab == null || s_AnchorUI == null) &&
                !TryResolveAnchor(s_PrefabSystem, out s_AnchorPrefab!, out s_AnchorUI!))
            {
                if (logIfNoAnchor)
                    AdvancedRoadToolsMod.s_Log.Error("[ART][Tools] Could not find RoadsServices anchor. Will retry later.");
                return;
            }

            // local non-null copies for analyzer clarity
            var anchorPrefab = s_AnchorPrefab!;
            var anchorUI = s_AnchorUI!;

#if DEBUG
            Dbg($"[ART][Tools] Creating tools UI. {ToolDefinitions.Count} registered tools");
#endif

            foreach (var definition in ToolDefinitions)
            {
                try
                {
                    // Duplicate donor (PrefabSystem registers an entity for the duplicate).
                    var toolPrefab = s_PrefabSystem.DuplicatePrefab(anchorPrefab, definition.ToolID);

                    // Strip donor-only bits on the duplicate.
                    if (toolPrefab.Has<Unlockable>())
                        toolPrefab.Remove<Unlockable>();
                    if (toolPrefab.Has<NetSubObjects>())
                        toolPrefab.Remove<NetSubObjects>();

                    // Fresh UIObject; keep donor group, bump priority, use mod-specified icon.
                    var uiObject = ScriptableObject.CreateInstance<UIObject>();
                    uiObject.name = definition.ToolID;
                    uiObject.m_Icon = definition.ui.ImagePath; // single source of truth is Mod.cs

                    // Copy group & active/debug flags from donor UI.
                    uiObject.m_IsDebugObject = anchorUI.m_IsDebugObject;
                    uiObject.m_Priority = anchorUI.m_Priority + 1;
                    uiObject.m_Group = anchorUI.m_Group; // "RoadsServices"
                    uiObject.active = anchorUI.active;

                    toolPrefab.AddComponentFrom(uiObject);

                    // Mark tile as a “tool” selector (vanilla pattern).
                    var netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(netUpgrade);

                    // IMPORTANT: inform PrefabSystem that the duplicate's components changed.
                    s_PrefabSystem.UpdatePrefab(toolPrefab);

                    // Hand the prefab to our Tool system.
                    var tb = s_World!.GetOrCreateSystemManaged(definition.Type) as ToolBaseSystem;
                    if (tb == null || !tb.TrySetPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"[ART][Tools] Failed to set up tool prefab for type \"{definition.Type}\"");
                        continue;
                    }

                    s_ToolsLookup[definition] = (toolPrefab, uiObject);
#if DEBUG
                    Dbg($"[ART][Tools] Tool \"{definition.ToolID}\" created");
#endif
                }
                catch (Exception ex)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"[ART][Tools] Tool \"{definition.ToolID}\" could not be created: {ex}");
                }
            }

            // Copy donor’s PlaceableNetData after load so all flags match vanilla.
            if (GameManager.instance != null)
            {
                GameManager.instance.onGameLoadingComplete -= SetupToolsOnGameLoaded;
                GameManager.instance.onGameLoadingComplete += SetupToolsOnGameLoaded;
            }

            s_Instantiated = true;
        }

        private static void SetupToolsOnGameLoaded(Purpose purpose, GameMode mode)
        {
            if (s_PrefabSystem == null || s_AnchorPrefab == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("[ART][Tools] SetupToolsOnGameLoaded: missing PrefabSystem or anchor prefab.");
                return;
            }

            foreach (var kv in s_ToolsLookup)
            {
                var def = kv.Key;
                var pair = kv.Value;

                try
                {
                    // Start from donor's PlaceableNetData if present; otherwise use default.
                    PlaceableNetData basePlaceable;
                    if (!s_PrefabSystem.TryGetComponentData(s_AnchorPrefab, out basePlaceable))
                        basePlaceable = default;

                    basePlaceable.m_SetUpgradeFlags = def.SetFlags;
                    basePlaceable.m_UnsetUpgradeFlags = def.UnsetFlags;
                    basePlaceable.m_PlacementFlags = def.PlacementFlags;
                    if (def.Underground)
                        basePlaceable.m_PlacementFlags |= PlacementFlags.UndergroundUpgrade;

                    // If the duplicate already has PlaceableNetData, remove before adding.
                    PlaceableNetData existing;
                    if (s_PrefabSystem.TryGetComponentData(pair.Prefab, out existing))
                        s_PrefabSystem.RemoveComponent<PlaceableNetData>(pair.Prefab);

                    s_PrefabSystem.AddComponentData(pair.Prefab, basePlaceable);
#if DEBUG
                    Dbg($"[ART][Tools] Applied PlaceableNetData to {def.ToolID}");
#endif
                }
                catch (Exception ex)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"[ART][Tools] Could not setup tool {def.ToolID}: {ex}");
                }
            }
        }

        /// <summary>
        /// Probe for a stable RoadsServices donor with a UIObject (Grass / Wide Sidewalk / Crosswalk).
        /// Returns true only when both prefab & UIObject are found and group == "RoadsServices".
        /// </summary>
        public static bool TryResolveAnchor(PrefabSystem prefabSystem, out PrefabBase prefab, out UIObject ui)
        {
            prefab = default!;
            ui = default!;

            if (prefabSystem == null)
                return false;

            // Prefer NetUpgradePrefab (earlier success), then fall back.
            var probes = new (string typeName, string name)[]
            {
                ("NetUpgradePrefab", "Grass"),
                ("NetUpgradePrefab", "Wide Sidewalk"),
                ("NetUpgradePrefab", "Crosswalk"),

                ("PrefabBase",       "Crosswalk"),
                ("PrefabBase",       "Wide Sidewalk"),
                ("PrefabBase",       "Sound Barrier"),
            };

            for (int i = 0; i < probes.Length; i++)
            {
                var typeName = probes[i].typeName;
                var name = probes[i].name;

                var id = new PrefabID(typeName, name);

                PrefabBase? cand = null;
                PrefabBase tmp;
                if (prefabSystem.TryGetPrefab(id, out tmp) && tmp != null)
                    cand = tmp;

#if DEBUG
                Dbg($"Probe {typeName}:{name}: {(cand != null ? "FOUND" : "missing")}");
#endif
                if (cand == null)
                    continue;

                UIObject? uioMaybe;
                if (!cand.TryGet(out uioMaybe) || uioMaybe == null)
                {
#if DEBUG
                    Dbg("  …found prefab but it has no UIObject → skip");
#endif
                    continue;
                }

                var group = uioMaybe.m_Group;
                string groupName = group != null ? group.name : "(null)";
                bool inRoadsServices = string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase);
                if (!inRoadsServices)
                {
#if DEBUG
                    Dbg($"  …UIObject group is '{groupName}', not 'RoadsServices' → skip");
#endif
                    continue;
                }

#if DEBUG
                Dbg($"Anchor OK (probe): {name}  Group={groupName}  Priority={uioMaybe.m_Priority}");
#endif
                prefab = cand;   // cand proven non-null
                ui = uioMaybe;   // uioMaybe proven non-null
                return true;
            }

            // Probes missed → robust fallback scan.
            return TryResolveAnchorByScan(prefabSystem, out prefab, out ui);
        }

        /// <summary>
        /// Slow but robust fallback: reflect PrefabSystem.m_Prefabs and find any prefab whose UIObject is in RoadsServices.
        /// Prefers names you expect (Wide Sidewalk / Crosswalk / Grass), otherwise any with PlaceableNetData.
        /// </summary>
        private static bool TryResolveAnchorByScan(PrefabSystem prefabSystem, out PrefabBase prefab, out UIObject ui)
        {
            prefab = default!;
            ui = default!;

            try
            {
                var fi = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
                var listObj = fi != null ? fi.GetValue(prefabSystem) as System.Collections.IEnumerable : null;
                if (listObj == null)
                {
#if DEBUG
                    Dbg("Scan fallback: m_Prefabs not accessible.");
#endif
                    return false;
                }

                PrefabBase? best = null;
                UIObject? bestUI = null;
                int bestScore = int.MinValue;

                foreach (var obj in listObj)
                {
                    var p = obj as PrefabBase;
                    if (p == null)
                        continue;

                    UIObject? uioMaybe;
                    if (!p.TryGet(out uioMaybe) || uioMaybe == null)
                        continue;

                    var group = uioMaybe.m_Group;
                    string groupName = group != null ? group.name : "(null)"; // avoid null to satisfy nullable analysis
                    if (!string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Score the candidate:
                    //  +1000 if name matches our favorites
                    //  +100  if it has PlaceableNetData
                    //  + priority as minor tiebreaker
                    int score = 0;
                    string n = p.name ?? string.Empty;
                    if (string.Equals(n, "Wide Sidewalk", StringComparison.OrdinalIgnoreCase))
                        score += 1000;
                    else if (string.Equals(n, "Crosswalk", StringComparison.OrdinalIgnoreCase))
                        score += 900;
                    else if (string.Equals(n, "Grass", StringComparison.OrdinalIgnoreCase))
                        score += 800;

                    if (prefabSystem.HasComponent<PlaceableNetData>(p))
                        score += 100;

                    score += uioMaybe.m_Priority;

#if DEBUG
                    string typeName = p.GetType().Name;
                    bool hasPlaceable = prefabSystem.HasComponent<PlaceableNetData>(p);
                    Dbg($"Scan candidate: {typeName}:{n}  score={score}  hasPlaceable={hasPlaceable}  priority={uioMaybe.m_Priority}");
#endif

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = p;
                        bestUI = uioMaybe;
                    }
                }

                if (best != null && bestUI != null)
                {
#if DEBUG
                    Dbg($"Anchor OK (scan): {best.name}  priority={bestUI.m_Priority}");
#endif
                    prefab = best;
                    ui = bestUI;
                    return true;
                }
            }
            catch (Exception ex)
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Warn("Scan fallback failed: " + ex);
            }

#if DEBUG
            Dbg("No RoadsServices candidate found by scan.");
#endif
            return false;
        }
    }
}
