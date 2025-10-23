// File: src/Tools/ToolsHelper.cs
// Purpose: Duplicate a RoadsServices donor and create our tile(s) safely.
// Notes:
//   • Probes include NetUpgradePrefab + PrefabBase fallbacks, then LOCK the first hit.
//   • We require UIGroup 'RoadsServices' and the candidate's UIObject to be in it.
//   • All logging is guarded; never throw from logs.
//   • No #nullable; all nullability is handled explicitly.

namespace AdvancedRoadTools.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

        // First successful probe is remembered for future runs.
        private static string? s_LockedTypeName;
        private static string? s_LockedName;

        public static bool IsReady => s_Instantiated && s_ToolsLookup.Count > 0;

#if DEBUG
        [Conditional("DEBUG")]
        private static void Dbg(string message)
        {
            try
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                {
                    log.Info(message);
                }
            }
            catch { }
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

            s_AnchorPrefab = null;
            s_AnchorUI = null;
            s_LockedTypeName = null;
            s_LockedName = null;
        }

        public static void RegisterTool(ToolDefinition toolDefinition)
        {
            if (HasTool(toolDefinition))
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Error($"Tool \"{toolDefinition.ToolID}\" already registered");
                return;
            }
#if DEBUG
            Dbg($"[ART][Tools] Registering tool \"{toolDefinition.ToolID}\" for {toolDefinition.Type.Name}");
#endif
            ToolDefinitions.Add(toolDefinition);
        }

        /// <summary>
        /// Find a donor with UIObject in UI group "RoadsServices".
        /// </summary>
        public static bool TryResolveAnchor(PrefabSystem prefabSystem, out PrefabBase? prefab, out UIObject? ui)
        {
            prefab = null;
            ui = null;

            if (prefabSystem == null)
                return false;

            // Require the RoadsServices group to exist; otherwise keep waiting.
            bool roadsGroupPresent = false;
            {
                PrefabBase groupPrefab;
                if (prefabSystem.TryGetPrefab(new PrefabID("UIGroupPrefab", "RoadsServices"), out groupPrefab) && groupPrefab != null)
                {
                    roadsGroupPresent = true;
#if DEBUG
                    Dbg("[ART][Tools] UIGroupPrefab 'RoadsServices' is present.");
#endif
                }
                else
                {
#if DEBUG
                    Dbg("[ART][Tools] UIGroupPrefab 'RoadsServices' not present yet; will keep waiting.");
#endif
                }
            }

            // Build the probe list (prefer last known good → NetUpgradePrefab → fallbacks).
            var probes = new List<(string typeName, string name)>(6);

            if (!string.IsNullOrEmpty(s_LockedTypeName) && !string.IsNullOrEmpty(s_LockedName))
                probes.Add((s_LockedTypeName!, s_LockedName!));

            // Primary, known-good anchors:
            probes.Add(("NetUpgradePrefab", "Wide Sidewalk"));
            probes.Add(("NetUpgradePrefab", "Crosswalk"));

            // Optional extra seen on some setups; harmless to try:
            probes.Add(("NetUpgradePrefab", "Grass"));

            // Fallbacks observed in some builds/locales:
            probes.Add(("PrefabBase", "Crosswalk"));
            probes.Add(("PrefabBase", "Wide Sidewalk"));
            probes.Add(("PrefabBase", "Sound Barrier"));

            for (int i = 0; i < probes.Count; i++)
            {
                string typeName = probes[i].typeName;
                string name = probes[i].name;

                var id = new PrefabID(typeName, name);

                // Prefab lookup with explicit null test.
                PrefabBase? candidatePrefabTemp;
                bool hasPrefab = prefabSystem.TryGetPrefab(id, out candidatePrefabTemp) && candidatePrefabTemp != null;

#if DEBUG
                Dbg($"[ART][Tools] Probe {typeName}:{name}: {(hasPrefab ? "FOUND" : "missing")}");
#endif
                if (!hasPrefab)
                    continue;

                // Promote to a non-null local so the analyzer is satisfied for the rest of this scope.
                PrefabBase candidatePrefab = candidatePrefabTemp!;

                // Try to get a UIObject; use a nullable temp and promote after the check.
                UIObject? uiMaybe;
                bool hasUI = candidatePrefab.TryGet(out uiMaybe) && uiMaybe != null;
                if (!hasUI)
                {
#if DEBUG
                    Dbg("  …found prefab but it has no UIObject → skip");
#endif
                    continue;
                }

                UIObject candidateUI = uiMaybe!; // safe: we just checked uiMaybe != null

                // Guard group access completely (group itself can be null).
                UIGroupPrefab? group = candidateUI.m_Group;
                string groupName = group != null ? group.name : string.Empty;

                // Must be RoadsServices AND that group must exist, to avoid false positives.
                bool inRoads = roadsGroupPresent
                               && group != null
                               && string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase);

                if (!inRoads)
                {
#if DEBUG
                    Dbg($"  …UIObject group is '{(group != null ? group.name : "(null)")}', not 'RoadsServices' OR group not ready → skip");
#endif
                    continue;
                }

#if DEBUG
                Dbg($"[ART][Tools] Anchor OK: {name}  Group={groupName}  Priority={candidateUI.m_Priority}");
#endif
                // Lock in the first success so future sessions probe this first.
                s_LockedTypeName = typeName;
                s_LockedName = name;

                prefab = candidatePrefab;
                ui = candidateUI;
                return true;
            }

#if DEBUG
            Dbg("[ART][Tools] No acceptable RoadsServices anchor found this frame.");
#endif
            return false;
        }



        /// <summary>
        /// Duplicate donor, add UIObject + NetUpgrade, and register with our ToolBaseSystem.
        /// Safe to call repeatedly; if donor not found, logs (once per call) and returns.
        /// </summary>
        public static void InstantiateTools(bool logIfNoAnchor = true)
        {
            if (s_Instantiated)
                return;

            if (s_World == null)
                s_World = World.DefaultGameObjectInjectionWorld;
            if (s_PrefabSystem == null && s_World != null)
                s_PrefabSystem = s_World.GetExistingSystemManaged<PrefabSystem>();

            if (s_PrefabSystem == null)
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Error("[ART][Tools] PrefabSystem not available.");
                return;
            }

            if ((s_AnchorPrefab == null || s_AnchorUI == null) &&
                !TryResolveAnchor(s_PrefabSystem, out s_AnchorPrefab, out s_AnchorUI))
            {
                if (logIfNoAnchor)
                {
                    var log = AdvancedRoadToolsMod.s_Log;
                    if (log != null)
                        log.Error("[ART][Tools] Could not find RoadsServices anchor. Will retry later.");
                }
                return;
            }

            PrefabBase? anchorPrefab = s_AnchorPrefab;
            UIObject? anchorUI = s_AnchorUI;
            if (anchorPrefab == null || anchorUI == null)
                return;

#if DEBUG
            Dbg($"[ART][Tools] Creating tools UI. {ToolDefinitions.Count} registered tools");
#endif

            foreach (ToolDefinition definition in ToolDefinitions)
            {
                try
                {
                    PrefabBase toolPrefab = s_PrefabSystem.DuplicatePrefab(anchorPrefab, definition.ToolID);
                    if (toolPrefab == null)
                    {
                        var log = AdvancedRoadToolsMod.s_Log;
                        if (log != null)
                            log.Error($"[ART][Tools] DuplicatePrefab returned null for \"{definition.ToolID}\"");
                        continue;
                    }

                    if (toolPrefab.Has<Unlockable>())
                        toolPrefab.Remove<Unlockable>();
                    if (toolPrefab.Has<NetSubObjects>())
                        toolPrefab.Remove<NetSubObjects>();

                    var uiObject = ScriptableObject.CreateInstance<UIObject>();
                    uiObject.name = definition.ToolID;
                    uiObject.m_Icon = definition.ui.ImagePath; // single source of truth
                    uiObject.m_IsDebugObject = anchorUI.m_IsDebugObject;
                    uiObject.m_Priority = anchorUI.m_Priority + 1;
                    uiObject.m_Group = anchorUI.m_Group; // "RoadsServices"
                    uiObject.active = anchorUI.active;

#if DEBUG
                    string grp = (anchorUI.m_Group != null) ? anchorUI.m_Group.name : "(null)";
                    Dbg($"[ART][Tools] UIObject: group={grp}, priority={uiObject.m_Priority}, icon={uiObject.m_Icon}");
#endif
                    toolPrefab.AddComponentFrom(uiObject);

                    var netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(netUpgrade);

                    ToolBaseSystem? toolSystem = (s_World != null)
                        ? s_World.GetOrCreateSystemManaged(definition.Type) as ToolBaseSystem
                        : null;

                    if (toolSystem == null || !toolSystem.TrySetPrefab(toolPrefab))
                    {
                        var log = AdvancedRoadToolsMod.s_Log;
                        if (log != null)
                            log.Error($"[ART][Tools] Failed to set up tool prefab for type \"{definition.Type}\"");
                        continue;
                    }

                    s_ToolsLookup[definition] = (toolPrefab, uiObject);
#if DEBUG
                    Dbg($"\t[ART][Tools] Tool \"{definition.ToolID}\" created");
#endif
                }
                catch (Exception ex)
                {
                    var log = AdvancedRoadToolsMod.s_Log;
                    if (log != null)
                        log.Error($"\t[ART][Tools] Tool \"{definition.ToolID}\" could not be created: {ex}");
                }
            }

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
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Error("[ART][Tools] SetupToolsOnGameLoaded: missing PrefabSystem or anchor prefab.");
                return;
            }

            foreach (var kv in s_ToolsLookup)
            {
                var def = kv.Key;
                var pair = kv.Value;

                try
                {
                    PlaceableNetData basePlaceable;
                    if (!s_PrefabSystem.TryGetComponentData(s_AnchorPrefab, out basePlaceable))
                        basePlaceable = default;

                    basePlaceable.m_SetUpgradeFlags = def.SetFlags;
                    basePlaceable.m_UnsetUpgradeFlags = def.UnsetFlags;
                    basePlaceable.m_PlacementFlags = def.PlacementFlags;
                    if (def.Underground)
                        basePlaceable.m_PlacementFlags |= PlacementFlags.UndergroundUpgrade;

                    PlaceableNetData existing;
                    if (s_PrefabSystem.TryGetComponentData(pair.Prefab, out existing))
                        s_PrefabSystem.RemoveComponent<PlaceableNetData>(pair.Prefab);

                    s_PrefabSystem.AddComponentData(pair.Prefab, basePlaceable);
#if DEBUG
                    Dbg($"[ART][Tools] Applied PlaceableNetData to {def.ToolID} (Underground={def.Underground})");
#endif
                }
                catch (Exception ex)
                {
                    var log = AdvancedRoadToolsMod.s_Log;
                    if (log != null)
                        log.Error($"[ART][Tools] Could not setup tool {def.ToolID}: {ex}");
                }
            }
        }
    }
}
