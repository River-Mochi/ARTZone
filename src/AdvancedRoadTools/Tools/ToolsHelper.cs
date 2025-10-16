// File: src/AdvancedRoadTools/Tools/ToolsHelper.cs
// Purpose: Public-API-only tool instantiation helper (no Harmony, no reflection).
// Notes:
//  - Resolves a *vanilla road prefab* (from a candidate list) to copy UI group + PlaceableNetData.
//  - Ensures the tool appears under Road Services and behaves like a net upgrade.
//  - Does NOT depend on “Wide Sidewalk”.

using System;
using System.Collections.Generic;
using Colossal.Serialization.Entities; // Purpose
using Game;                           // GameMode
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
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new List<ToolDefinition>();

        private static readonly Dictionary<ToolDefinition, (PrefabBase prefab, UIObject ui)> s_ToolsLookup =
            new Dictionary<ToolDefinition, (PrefabBase, UIObject)>();

        private static bool s_Initialized;
        private static bool s_Instantiated;
        private static bool s_SetupSubscribed;

        private static World s_World = null!;
        private static PrefabSystem s_PrefabSystem = null!;

        private static PrefabBase s_TemplatePrefab = null!;
        private static UIObject s_TemplateUI = null!;

        public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
        public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

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

        public static void Initialize(bool force = false)
        {
            if (s_Initialized && !force)
            {
                AdvancedRoadToolsMod.s_Log.Info("ToolsHelper.Initialize skipped (already initialized).");
                return;
            }

            ToolDefinitions = new List<ToolDefinition>(8);
            s_ToolsLookup.Clear();
            s_Initialized = true;
            s_Instantiated = false;
            s_SetupSubscribed = false;

            s_World = World.DefaultGameObjectInjectionWorld;
            if (s_World == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("DefaultGameObjectInjectionWorld is null; will retry when InstantiateTools() runs.");
                return;
            }

            s_PrefabSystem = s_World.GetExistingSystemManaged<PrefabSystem>();
            if (s_PrefabSystem == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("PrefabSystem not available yet; will retry when InstantiateTools() runs.");
                return;
            }
        }

        public static void InstantiateTools()
        {
            if (s_Instantiated)
            {
                AdvancedRoadToolsMod.s_Log.Info("InstantiateTools skipped (already instantiated).");
                return;
            }

            if (s_World == null)
                s_World = World.DefaultGameObjectInjectionWorld;

            if (s_World == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("InstantiateTools: DefaultGameObjectInjectionWorld is still null.");
                return;
            }

            s_PrefabSystem ??= s_World.GetExistingSystemManaged<PrefabSystem>();
            if (s_PrefabSystem == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("InstantiateTools: PrefabSystem is still not available.");
                return;
            }

            AdvancedRoadToolsMod.s_Log.Info($"Creating tools UI. {ToolDefinitions.Count} registered tools");

            // >>> NEW: resolve a safe vanilla road template (no dependency on a specific asset name)
            if (!TryResolveTemplatePrefab(out s_TemplatePrefab, out s_TemplateUI))
            {
                AdvancedRoadToolsMod.s_Log.Error("Could not resolve template prefab/UI. Tools will not be created.");
                return;
            }

            foreach (ToolDefinition definition in ToolDefinitions)
            {
                try
                {
                    PrefabBase toolPrefab = Object.Instantiate(s_TemplatePrefab);
                    toolPrefab.name = definition.ToolID;

                    // Strip template-only bits; we only keep what we explicitly re-add.
                    toolPrefab.Remove<UIObject>();
                    toolPrefab.Remove<Unlockable>();
                    toolPrefab.Remove<NetSubObjects>();

                    // UI tile: keep the template's group so we appear in Road Services; use our own icon & priority.
                    UIObject ui = ScriptableObject.CreateInstance<UIObject>();
                    ui.m_Icon = definition.ui.ImagePath;   // e.g. "coui://AdvancedRoadTools/images/ToolsIcon.png"
                    ui.name = definition.ToolID;
                    ui.m_IsDebugObject = s_TemplateUI.m_IsDebugObject;
                    ui.m_Priority = definition.Priority;
                    ui.m_Group = s_TemplateUI.m_Group;
                    ui.active = s_TemplateUI.active;
                    toolPrefab.AddComponentFrom(ui);

                    // Treat this like a net upgrade so placement UX matches expectations.
                    NetUpgrade upgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(upgrade);

                    ToolBaseSystem? system = s_World.GetOrCreateSystemManaged(definition.Type) as ToolBaseSystem;
                    if (system == null)
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Failed to get or create tool system: {definition.Type}");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    if (!system.TrySetPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Failed to set up tool prefab for type \"{definition.Type}\"");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    if (!s_PrefabSystem.AddPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"Tool \"{definition.ToolID}\" could not be added to {nameof(PrefabSystem)}");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    s_ToolsLookup[definition] = (toolPrefab, ui);
                    AdvancedRoadToolsMod.s_Log.Info($"\tTool \"{definition.ToolID}\" was successfully created");
                }
                catch (Exception e)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tTool \"{definition.ToolID}\" could not be created: {e}");
                }
            }

            if (!s_SetupSubscribed && GameManager.instance != null)
            {
                GameManager.instance.onGameLoadingComplete += SetupToolsOnGameLoaded;
                s_SetupSubscribed = true;
            }

            s_Instantiated = true;
        }

        private static void SetupToolsOnGameLoaded(Purpose purpose, GameMode mode)
        {
            if (s_PrefabSystem == null)
            {
                s_PrefabSystem = s_World.GetExistingSystemManaged<PrefabSystem>();
                if (s_PrefabSystem == null)
                {
                    AdvancedRoadToolsMod.s_Log.Error("SetupToolsOnGameLoaded: PrefabSystem unavailable.");
                    return;
                }
            }

            AdvancedRoadToolsMod.s_Log.Info($"Setting up tools. {s_ToolsLookup.Count} registered tools");

            foreach (KeyValuePair<ToolDefinition, (PrefabBase prefab, UIObject ui)> kvp in s_ToolsLookup)
            {
                ToolDefinition def = kvp.Key;
                PrefabBase prefab = kvp.Value.prefab;

                try
                {
                    if (!TryGetPlaceableNetDataFromTemplate(out PlaceableNetData placeable))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"\tCould not obtain PlaceableNetData for {def.ToolID}.");
                        continue;
                    }

                    // Behave like an upgrade; keep other fields inherited from the template.
                    placeable.m_PlacementFlags |= Game.Net.PlacementFlags.IsUpgrade;
                    placeable.m_PlacementFlags |= Game.Net.PlacementFlags.UndergroundUpgrade;

                    s_PrefabSystem.AddComponentData(prefab, placeable);
                }
                catch (Exception e)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tCould not setup tool {def.ToolID}: {e}");
                }
            }
        }

        // --- Template resolution (NEW): try many common vanilla roads; we only need a prefab with a UIObject ---
        private static bool TryResolveTemplatePrefab(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;

            string[] nameCandidates =
            {
                "Small Road",
                "Two-Lane Road",
                "2 Lane Road",
                "Asphalt Road",
                "Basic Road",
                "Gravel Road",
                "Medium Road",
                "Large Road",
                "Avenue",
                "Highway",
                "Pedestrian Street",
                "Pedestrian Street Small",
                "Pedestrian Street Medium",
                "Pedestrian Street Large",
                // keep the former one as last fallback
                "Wide Sidewalk",
            };

            string[] typeCandidates =
            {
                nameof(RoadPrefab),
                nameof(NetPrefab),
                "ContentPrefab",
                nameof(PrefabBase)
            };

            foreach (string roadName in nameCandidates)
            {
                foreach (string typeName in typeCandidates)
                {
                    if (s_PrefabSystem.TryGetPrefab(new PrefabID(typeName, roadName), out PrefabBase? p) && p != null)
                    {
                        UIObject uio = p.GetComponent<UIObject>();
                        if (uio != null)
                        {
                            AdvancedRoadToolsMod.s_Log.Info($"Template resolved: {roadName} ({typeName})");
                            prefab = p!;
                            ui = uio!;
                            return true;
                        }
                    }
                }
            }

            AdvancedRoadToolsMod.s_Log.Error(
                "No suitable road template found via PrefabSystem.TryGetPrefab(..). " +
                "Tool button cannot be created in the Road Services palette.");
            return false;
        }

        private static bool TryGetPlaceableNetDataFromTemplate(out PlaceableNetData data)
        {
            data = default;

            if (s_TemplatePrefab == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("PlaceableNetData: template prefab is null.");
                return false;
            }

            try
            {
                data = s_PrefabSystem.GetComponentData<PlaceableNetData>(s_TemplatePrefab);
                return true;
            }
            catch (Exception e)
            {
                AdvancedRoadToolsMod.s_Log.Error($"Failed to read PlaceableNetData from template: {e}");
                return false;
            }
        }
    }
}
