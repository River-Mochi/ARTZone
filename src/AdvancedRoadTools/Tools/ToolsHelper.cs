// File: src/AdvancedRoadTools/Tools/ToolsHelper.cs
// Purpose: Public-API-only tool instantiation helper (no Harmony, no reflection).
// - Resolves Road Services template (Sound Barrier; fallback Wide Sidewalk) to copy UI group.
// - Places our tile right AFTER the template by priority+1 so it appears in the same tab.
// - Behaves like a net upgrade (PlacementFlags merged on setup).

using System;
using System.Collections.Generic;
using Colossal.Serialization.Entities;  // Purpose
using Game;                            // GameMode
using Game.Net;                        // PlacementFlags
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

            // Road Services template: Sound Barrier (preferred) -> Wide Sidewalk (fallback)
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

                    // UI tile: inherit the template GROUP and place our icon right AFTER it in the palette.
                    UIObject ui = ScriptableObject.CreateInstance<UIObject>();
                    ui.m_Icon = string.IsNullOrEmpty(definition.ui.ImagePath)
                        ? ToolDefinition.UI.PathPrefix + "ToolsIcon.png"
                        : definition.ui.ImagePath;
                    ui.name = definition.ToolID;
                    ui.m_IsDebugObject = s_TemplateUI.m_IsDebugObject;
                    ui.m_Group = s_TemplateUI.m_Group;
                    ui.active = s_TemplateUI.active;
                    ui.m_Priority = s_TemplateUI.m_Priority + 1; // after Sound Barrier/Wide Sidewalk
                    toolPrefab.AddComponentFrom(ui);

                    // Behave like a net upgrade so placement UX matches expectations.
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
                    // Start from the template’s placeable data, then merge our flags.
                    PlaceableNetData placeable = s_PrefabSystem.GetComponentData<PlaceableNetData>(s_TemplatePrefab);

                    placeable.m_SetUpgradeFlags = def.SetFlags;
                    placeable.m_UnsetUpgradeFlags = def.UnsetFlags;

                    // Merge placement flags + ensure upgrade behavior.
                    placeable.m_PlacementFlags |= def.PlacementFlags;
                    placeable.m_PlacementFlags |= PlacementFlags.IsUpgrade;
                    if (def.Underground)
                        placeable.m_PlacementFlags |= PlacementFlags.UndergroundUpgrade;

                    s_PrefabSystem.AddComponentData(prefab, placeable);
                }
                catch (Exception e)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"\tCould not setup tool {def.ToolID}: {e}");
                }
            }
        }

        // --- Template resolution: probe *only* Road Services tiles verified with dnSpy ---
        private static bool TryResolveTemplatePrefab(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;

            // *Road Services* palette tiles we want to anchor to.
            // these match dnSpy’s NetPieceRequirements,
            // but the PrefabSystem.TryGetPrefab must probe by **string name**.
            // Order = preference for where our tile should appear “after”.
            string[] nameCandidates =
            {
        "Sound Barrier",   // prefer appearing right after this
        "Wide Sidewalk",   // exact
        "Wide Sidewalks",  // plural variant found on some installs
        "Traffic Lights",  // in the same Road Services group
        "Crosswalk"
    };

            // Try common prefab type keys. The game registers nets under several type ids.
            string[] typeCandidates =
            {
        nameof(RoadPrefab),
        nameof(NetPrefab),
        "ContentPrefab",
        nameof(PrefabBase)
    };

            foreach (string name in nameCandidates)
            {
                foreach (string typeName in typeCandidates)
                {
                    AdvancedRoadToolsMod.s_Log.Info($"[ART] Probe prefab: \"{name}\" ({typeName})");
                    if (s_PrefabSystem.TryGetPrefab(new PrefabID(typeName, name), out PrefabBase? p) && p != null)
                    {
                        UIObject uio = p.GetComponent<UIObject>();
                        if (uio != null)
                        {
                            AdvancedRoadToolsMod.s_Log.Info(
                                $"[ART] Template resolved: {name} ({typeName}), group={uio.m_Group?.name ?? "(null)"} priority={uio.m_Priority}"
                            );
                            prefab = p!;
                            ui = uio!;
                            return true;
                        }
                        else
                        {
                            AdvancedRoadToolsMod.s_Log.Warn($"[ART] Prefab \"{name}\" found but has no UIObject; trying next.");
                        }
                    }
                }
            }

            AdvancedRoadToolsMod.s_Log.Error(
                "[ART] No Road Services template resolved (Sound Barrier / Wide Sidewalk / Traffic Lights / Crosswalk). " +
                "Tool button cannot be created in the Road Services palette."
            );
            return false;
        }

    }
}
