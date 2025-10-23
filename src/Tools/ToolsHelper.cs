// File: src/Tools/ToolsHelper.cs
// Purpose: Duplicate a RoadsServices donor tile (after it exists) and build our own tool tile.
//          Never modify the donor. Loud DEBUG logging for anchor discovery and prefab edits.

namespace AdvancedRoadTools.Tools
{
    using System;
    using System.Collections.Generic;
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

        private static PrefabBase? s_AnchorPrefab; // donor from RoadsServices
        private static UIObject? s_AnchorUI;

        public static void Initialize(bool force = false)
        {
            if (!force && s_World != null)
                return;

            ToolDefinitions = new(4);
            s_ToolsLookup.Clear();
            s_Instantiated = false;

            s_World = World.DefaultGameObjectInjectionWorld;
            s_PrefabSystem = s_World?.GetExistingSystemManaged<PrefabSystem>();
        }

        public static void RegisterTool(ToolDefinition toolDefinition)
        {
            if (HasTool(toolDefinition))
            {
                AdvancedRoadToolsMod.s_Log.Error($"Tool \"{toolDefinition.ToolID}\" already registered");
                return;
            }

            AdvancedRoadToolsMod.s_Log.Info($"Registering tool \"{toolDefinition.ToolID}\" with system \"{toolDefinition.Type.Name}\"");
            ToolDefinitions.Add(toolDefinition);
        }

        public static void InstantiateTools(bool logIfNoAnchor = true)
        {
            if (s_Instantiated)
                return;

            s_World ??= World.DefaultGameObjectInjectionWorld;
            s_PrefabSystem ??= s_World?.GetExistingSystemManaged<PrefabSystem>();

            if (s_PrefabSystem is null)
            {
                AdvancedRoadToolsMod.s_Log.Error("InstantiateTools: PrefabSystem not available.");
                return;
            }

            if (!TryResolveAnchor(out s_AnchorPrefab!, out s_AnchorUI!))
            {
                if (logIfNoAnchor)
                    AdvancedRoadToolsMod.s_Log.Error("Could not find Road Services anchor. Tools will not be created.");
                return;
            }

#if DEBUG
            AdvancedRoadToolsMod.s_Log.Info($"[ART] Creating {ToolDefinitions.Count} tool(s) using donor: {s_AnchorPrefab!.name}");
#endif

            foreach (ToolDefinition definition in ToolDefinitions)
            {
                try
                {
                    PrefabBase toolPrefab = s_PrefabSystem.DuplicatePrefab(s_AnchorPrefab!, definition.ToolID);

                    // Strip donor-only bits on THE DUPLICATE ONLY; donor remains untouched.
                    if (toolPrefab.Has<Unlockable>())
                    {
                        toolPrefab.Remove<Unlockable>();
                        LogDbg($"Removed Unlockable from {definition.ToolID}");
                    }
                    if (toolPrefab.Has<NetSubObjects>())
                    {
                        toolPrefab.Remove<NetSubObjects>();
                        LogDbg($"Removed NetSubObjects from {definition.ToolID}");
                    }
                    if (toolPrefab.Has<UIObject>())
                    {
                        toolPrefab.Remove<UIObject>();
                        LogDbg($"Removed donor UIObject from {definition.ToolID}");
                    }

                    // Fresh UIObject — keep donor group, change only name/icon/priority.
                    var uiObject = ScriptableObject.CreateInstance<UIObject>();
                    uiObject.name = definition.ToolID;
                    uiObject.m_Icon = definition.ui.ImagePath;          // e.g., coui://ui-mods/images/grid-color.svg
                    uiObject.m_IsDebugObject = s_AnchorUI!.m_IsDebugObject;
                    uiObject.m_Priority = s_AnchorUI!.m_Priority + 1;    // place just after donor
                    uiObject.m_Group = s_AnchorUI!.m_Group;              // stay inside RoadsServices palette
                    uiObject.active = s_AnchorUI!.active;
                    toolPrefab.AddComponentFrom(uiObject);
                    LogDbg($"Added UIObject (icon={uiObject.m_Icon}, group={uiObject.m_Group?.name ?? "(null)"} priority={uiObject.m_Priority})");

                    // Make the tile behave like a tool selector (vanilla uses this).
                    var netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(netUpgrade);
                    LogDbg("Added NetUpgrade marker");

                    // Hand the prefab to the tool system so it can Activate/Deactivate it.
                    if (s_World!.GetOrCreateSystemManaged(definition.Type) is not ToolBaseSystem toolSystem
                        || !toolSystem.TrySetPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Failed to set up tool prefab for type \"{definition.Type}\"");
                        continue;
                    }

                    // Ensure ECS sees our freshly added components.
                    s_PrefabSystem.UpdatePrefab(toolPrefab);
                    LogDbg("UpdatePrefab called");

                    s_ToolsLookup[definition] = (toolPrefab, uiObject);
                    AdvancedRoadToolsMod.s_Log.Info($"\tTool \"{definition.ToolID}\" created");
                }
                catch (Exception e)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tTool \"{definition.ToolID}\" could not be created: {e}");
                }
            }

            // Defer PlaceableNetData wiring until RoadsServices is fully loaded.
            if (GameManager.instance != null)
            {
                GameManager.instance.onGameLoadingComplete -= SetupToolsOnGameLoaded;
                GameManager.instance.onGameLoadingComplete += SetupToolsOnGameLoaded;
            }

            s_Instantiated = true;
        }

        private static void SetupToolsOnGameLoaded(Colossal.Serialization.Entities.Purpose purpose, GameMode mode)
        {
            if (s_PrefabSystem is null || s_AnchorPrefab is null)
            {
                AdvancedRoadToolsMod.s_Log.Error("SetupToolsOnGameLoaded: missing PrefabSystem or anchor prefab.");
                return;
            }

            foreach ((ToolDefinition def, (PrefabBase Prefab, UIObject UI) pair) in s_ToolsLookup)
            {
                try
                {
                    var placeable = s_PrefabSystem.GetComponentData<PlaceableNetData>(s_AnchorPrefab);
                    placeable.m_SetUpgradeFlags = def.SetFlags;
                    placeable.m_UnsetUpgradeFlags = def.UnsetFlags;
                    placeable.m_PlacementFlags = def.PlacementFlags;

                    if (def.Underground)
                        placeable.m_PlacementFlags |= PlacementFlags.UndergroundUpgrade;

                    s_PrefabSystem.AddComponentData(pair.Prefab, placeable);
                    s_PrefabSystem.UpdatePrefab(pair.Prefab);
                    LogDbg($"PlaceableNetData cloned & updated for {def.ToolID}");
                }
                catch (Exception e)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tCould not setup tool {def.ToolID}: {e}");
                }
            }
        }

        private static bool TryResolveAnchor(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;

            if (s_PrefabSystem is null)
                return false;

            var candidates = new[]
            {
                new PrefabID("PrefabBase", "Wide Sidewalk"),
                new PrefabID("PrefabBase", "Crosswalk"),
                new PrefabID("PrefabBase", "Sound Barrier"), // fallback; never modified
            };

            foreach (var id in candidates)
            {
                bool found = s_PrefabSystem.TryGetPrefab(id, out PrefabBase? p) && p is not null;
#if DEBUG
                AdvancedRoadToolsMod.s_Log.Info($"[ART] Anchor probe: {id.GetName()} => {(found ? "found" : "missing")}");
#endif
                if (!found)
                    continue;

                bool hasUI = p!.TryGet(out UIObject u) && u is not null;
#if DEBUG
                AdvancedRoadToolsMod.s_Log.Info($"[ART]   UIObject on {id.GetName()} => {(hasUI ? "present" : "absent")}");
#endif
                if (!hasUI)
                    continue;

                prefab = p!;
                ui = u!;
                AdvancedRoadToolsMod.s_Log.Info($"[ART] Anchor resolved: {id.GetName()}");
                return true;
            }

            AdvancedRoadToolsMod.s_Log.Error("[ART] No anchor prefab with UIObject found in RoadsServices.");
            return false;
        }

#if DEBUG
        private static void LogDbg(string msg) => AdvancedRoadToolsMod.s_Log.Info("[ART][DBG] " + msg);
#else
        private static void LogDbg(string _) { }
#endif
    }
}
