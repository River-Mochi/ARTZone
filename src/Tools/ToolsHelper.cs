// File: src/Tools/ToolsHelper.cs
// Purpose: Register and instantiate tool prefabs + their Road Services palette tiles
// without Harmony or reflection in Release build, using a short list of robust anchors.

using System;
using System.Collections.Generic;
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
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(8);
        public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
        public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_ToolsLookup = new(8);

        private static bool s_Instantiated;
        private static World? s_World;
        private static PrefabSystem? s_PrefabSystem;

        private static PrefabBase? s_AnchorPrefab;
        private static UIObject? s_AnchorUI;

        public static void Initialize(bool force = false)
        {
            if (!force && s_World != null)
                return;

            ToolDefinitions = new(8);
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

            AdvancedRoadToolsMod.s_Log.Info($"Creating tools UI. {ToolDefinitions.Count} registered tools");
            foreach (ToolDefinition definition in ToolDefinitions)
            {
                try
                {
                    PrefabBase toolPrefab = Object.Instantiate(s_AnchorPrefab!);
                    toolPrefab.name = definition.ToolID;

                    // remove UI bits we will recreate
                    toolPrefab.Remove<UIObject>();
                    toolPrefab.Remove<Unlockable>();
                    toolPrefab.Remove<NetSubObjects>();

                    // Create our palette tile UI object: copy group; set priority anchor+1
                    UIObject uiObject = ScriptableObject.CreateInstance<UIObject>();
                    uiObject.m_Icon = definition.ui.ImagePath; // keep path as passed
                    uiObject.name = definition.ToolID;
                    uiObject.m_IsDebugObject = s_AnchorUI!.m_IsDebugObject;
                    uiObject.m_Priority = s_AnchorUI!.m_Priority + 1;
                    uiObject.m_Group = s_AnchorUI!.m_Group;     // <- keeps it inside RoadsServices
                    uiObject.active = s_AnchorUI!.active;
                    toolPrefab.AddComponentFrom(uiObject);

                    // Upgrade marker to make the button functional
                    NetUpgrade netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(netUpgrade);

                    // Give the tool system its prefab
                    if (s_World!.GetOrCreateSystemManaged(definition.Type) is not ToolBaseSystem toolSystem
                        || !toolSystem.TrySetPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Failed to set up tool prefab for type \"{definition.Type}\"");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    if (!s_PrefabSystem!.AddPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Tool \"{definition.ToolID}\" could not be added to {nameof(PrefabSystem)}");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    s_ToolsLookup[definition] = (toolPrefab, uiObject);
                    AdvancedRoadToolsMod.s_Log.Info($"\tTool \"{definition.ToolID}\" was successfully created");
                }
                catch (Exception e)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tTool \"{definition.ToolID}\" could not be created: {e}");
                }
            }

            if (GameManager.instance != null)
                GameManager.instance.onGameLoadingComplete += SetupToolsOnGameLoaded;

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

            // Short list of robust RoadsServices items (order matters)
            var candidates = new[]
            {
                ("PrefabBase", "Wide Sidewalk"),
                ("PrefabBase", "Crosswalk"),
                ("PrefabBase", "Sound Barrier"),
            };

            foreach (var (type, name) in candidates)
            {
                var id = new PrefabID(type, name);
                if (s_PrefabSystem.TryGetPrefab(id, out PrefabBase? p) && p is not null)
                {
                    UIObject? u = p.GetComponent<UIObject>();
                    if (u is not null)
                    {
                        prefab = p;
                        ui = u;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
