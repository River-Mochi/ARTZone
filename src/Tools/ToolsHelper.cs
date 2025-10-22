// File: src/Tools/ToolsHelper.cs
// Purpose: Minimal helper to spawn our Road Services tile next to a known anchor.
// Anchor order: "Wide Sidewalk" -> "Crosswalk" -> "Sound Barrier" (must be in group "RoadsServices").
// UI priority: anchor.m_Priority + 1. Icon path is passed in via ToolDefinition.UI (CouiRoot/images/...).

using System;
using System.Collections.Generic;
using Colossal.Serialization.Entities; // Purpose, GameMode
using Game;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AdvancedRoadTools.Tools
{
    public static class ToolsHelper
    {
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(4);
        public static bool HasTool(ToolDefinition def) => ToolDefinitions.Contains(def);
        public static bool HasTool(string id) => ToolDefinitions.Exists(t => t.ToolID == id);

        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_Tools = new(4);
        private static bool s_Initialized;
        private static bool s_Instantiated;

        private static World? s_World;
        private static PrefabSystem? s_Prefabs;

        private static PrefabBase? s_AnchorPrefab;
        private static UIObject? s_AnchorUI;

        public static void Initialize(bool force = false)
        {
            if (!s_Initialized || force)
            {
                if (force)
                {
                    ToolDefinitions = new(4);
                    s_Tools.Clear();
                    s_Instantiated = false;
                    s_AnchorPrefab = null;
                    s_AnchorUI = null;
                }
                s_World = World.DefaultGameObjectInjectionWorld;
                s_Prefabs = s_World?.GetExistingSystemManaged<PrefabSystem>();
                s_Initialized = true;
            }
        }

        public static void RegisterTool(ToolDefinition def)
        {
            if (HasTool(def))
            {
                AdvancedRoadToolsMod.s_Log.Warn($"Tool \"{def.ToolID}\" already registered; skipping.");
                return;
            }
            AdvancedRoadToolsMod.s_Log.Info($"Register tool \"{def.ToolID}\"");
            ToolDefinitions.Add(def);
        }

        public static void InstantiateTools(bool logIfNoAnchor = true)
        {
            if (!s_Initialized)
                Initialize();
            if (s_Instantiated)
            {
                AdvancedRoadToolsMod.s_Log.Info("InstantiateTools skipped (already done).");
                return;
            }

            s_World ??= World.DefaultGameObjectInjectionWorld;
            s_Prefabs ??= s_World?.GetExistingSystemManaged<PrefabSystem>();

            if (s_World is null || s_Prefabs is null)
            {
                if (logIfNoAnchor)
                    AdvancedRoadToolsMod.s_Log.Error("InstantiateTools: World or PrefabSystem is null.");
                return;
            }

            if (!TryResolveAnchor(out s_AnchorPrefab!, out s_AnchorUI!))
            {
                if (logIfNoAnchor)
                    AdvancedRoadToolsMod.s_Log.Warn("InstantiateTools: Road Services anchor not ready yet.");
                return;
            }

            if (ToolDefinitions.Count == 0)
            {
                AdvancedRoadToolsMod.s_Log.Warn("InstantiateTools: no ToolDefinitions registered.");
                return;
            }

            foreach (var def in ToolDefinitions)
            {
                try
                {
                    var toolPrefab = Object.Instantiate(s_AnchorPrefab!);
                    toolPrefab.name = def.ToolID;

                    // Strip copied components we don't want to keep as-is
                    toolPrefab.Remove<UIObject>();
                    toolPrefab.Remove<Unlockable>();
                    toolPrefab.Remove<NetSubObjects>();

                    // Tile UI
                    var ui = ScriptableObject.CreateInstance<UIObject>();
                    ui.name = def.ToolID;
                    ui.m_Icon = def.ui.ImagePath;                 // e.g., coui://AdvancedRoadTools/images/ToolsIcon.png
                    ui.m_Group = s_AnchorUI!.m_Group;             // must be RoadsServices
                    ui.m_IsDebugObject = s_AnchorUI!.m_IsDebugObject;
                    ui.m_Priority = s_AnchorUI!.m_Priority + 1;   // sit right after anchor
                    ui.active = s_AnchorUI!.active;
                    toolPrefab.AddComponentFrom(ui);

                    // Minimal NetUpgrade
                    var upgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(upgrade);

                    // Bind to tool system
                    if (s_World.GetOrCreateSystemManaged(def.Type) is not ToolBaseSystem sys
                        || !sys.TrySetPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Failed to set up tool prefab for type \"{def.Type}\"");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    // Register prefab
                    if (!s_Prefabs.AddPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Tool \"{def.ToolID}\" could not be added to PrefabSystem");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    s_Tools[def] = (toolPrefab, ui);
                    AdvancedRoadToolsMod.s_Log.Info($"\tTool \"{def.ToolID}\" created next to \"{s_AnchorPrefab!.name}\"");
                }
                catch (Exception ex)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tTool \"{def.ToolID}\" could not be created: {ex}");
                }
            }

            if (GameManager.instance != null)
                GameManager.instance.onGameLoadingComplete += SetupToolsOnGameLoaded;

            s_Instantiated = true;
        }

        private static void SetupToolsOnGameLoaded(Purpose purpose, GameMode mode)
        {
            if (s_Prefabs is null || s_AnchorPrefab is null)
            {
                AdvancedRoadToolsMod.s_Log.Error("SetupToolsOnGameLoaded: missing PrefabSystem or anchor.");
                return;
            }

            AdvancedRoadToolsMod.s_Log.Info($"Setting up tools ({s_Tools.Count}).");

            foreach (var kv in s_Tools)
            {
                var def = kv.Key;
                var prefab = kv.Value.Prefab;
                try
                {
                    // Copy PlaceableNetData from anchor and apply our flags
                    var placeable = s_Prefabs.GetComponentData<PlaceableNetData>(s_AnchorPrefab);
                    placeable.m_SetUpgradeFlags = def.SetFlags;
                    placeable.m_UnsetUpgradeFlags = def.UnsetFlags;
                    placeable.m_PlacementFlags = def.PlacementFlags;

                    if (def.Underground)
                        placeable.m_PlacementFlags |= PlacementFlags.UndergroundUpgrade;

                    s_Prefabs.AddComponentData(prefab, placeable);
                }
                catch (Exception ex)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tCould not setup tool {def.ToolID}: {ex}");
                }
            }
        }

        // ---- Anchor resolution (strict to RoadsServices) ----
        private static bool TryResolveAnchor(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;
            if (s_Prefabs is null)
                return false;

            if (TryPrefabInRoadsServices("Wide Sidewalk", out prefab, out ui))
                return true;
            if (TryPrefabInRoadsServices("Crosswalk", out prefab, out ui))
                return true;
            if (TryPrefabInRoadsServices("Sound Barrier", out prefab, out ui))
                return true;

            return false;
        }

        private static bool TryPrefabInRoadsServices(string name, out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;
            var id = new PrefabID(nameof(PrefabBase), name);
            if (s_Prefabs!.TryGetPrefab(id, out PrefabBase? p) && p is not null)
            {
                var u = p.GetComponent<UIObject>();
                if (u != null && u.m_Group != null && string.Equals(u.m_Group.name, "RoadsServices", StringComparison.Ordinal))
                {
                    prefab = p;
                    ui = u;
                    return true;
                }
            }
            return false;
        }
    }
}
