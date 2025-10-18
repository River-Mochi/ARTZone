// File: src/AdvancedRoadTools/Tools/ToolsHelper.cs
// Minimal + Original behavior (no Harmony/Traverse):
// - Extra probe logging when ART_DIAGNOSTICS is defined.

#nullable enable
using System;
using System.Collections.Generic;
using Colossal.Serialization.Entities;
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
        // ---- Public surface (same as Original) --------------------------------
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(8);
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

        // ---- Internals ---------------------------------------------------------
        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_ToolsLookup = new(8);

        public static bool Initialized
        {
            get; private set;
        }
        private static bool s_Instantiated;

        private static World? s_World;
        private static PrefabSystem? s_PrefabSystem;

        private static PrefabBase? s_AnchorPrefab;  // e.g., "Wide Sidewalk" or "Sound Barrier" etc.
        private static UIObject? s_AnchorUI;

        // Idempotent: does NOT clear ToolDefinitions unless you *really* want to force-reset.
        public static void Initialize(bool force = false)
        {
            if (!Initialized || force)
            {
                if (force)
                {
                    ToolDefinitions = new(8);
                    s_ToolsLookup.Clear();
                }

                s_World = World.DefaultGameObjectInjectionWorld;
                s_PrefabSystem = s_World?.GetExistingSystemManaged<PrefabSystem>();
                Initialized = true;
            }
        }

        public static void InstantiateTools()
        {
            if (!Initialized)
                Initialize();

            if (s_Instantiated)
            {
                AdvancedRoadToolsMod.s_Log.Info("InstantiateTools skipped (already instantiated).");
                return;
            }

            s_World ??= World.DefaultGameObjectInjectionWorld;
            if (s_World is null)
            {
                AdvancedRoadToolsMod.s_Log.Error("InstantiateTools: DefaultGameObjectInjectionWorld is null.");
                return;
            }

            s_PrefabSystem ??= s_World.GetExistingSystemManaged<PrefabSystem>();
            if (s_PrefabSystem is null)
            {
                AdvancedRoadToolsMod.s_Log.Error("InstantiateTools: PrefabSystem not available.");
                return;
            }

            AdvancedRoadToolsMod.s_Log.Info($"Creating tools UI. {ToolDefinitions.Count} registered tools");
            if (ToolDefinitions.Count == 0)
            {
                AdvancedRoadToolsMod.s_Log.Error("[ART] No tools are registered. Ensure ToolsHelper.RegisterTool(...) ran before InstantiateTools().");
                return;
            }

            if (!TryResolveAnchor(out s_AnchorPrefab!, out s_AnchorUI!))
            {
                AdvancedRoadToolsMod.s_Log.Error("Could not find Road Services anchor (Wide Sidewalk / Sound Barrier / Traffic Lights / Crosswalk). Tools will not be created.");
                return;
            }

            foreach (var definition in ToolDefinitions)
            {
                try
                {
                    // Clone the anchor prefab.
                    var toolPrefab = Object.Instantiate(s_AnchorPrefab!);
                    toolPrefab.name = definition.ToolID;

                    // Strip anchor-only components.
                    toolPrefab.Remove<UIObject>();
                    toolPrefab.Remove<Unlockable>();
                    toolPrefab.Remove<NetSubObjects>();

                    // Create our UI tile in the same group as the anchor.
                    var uiObject = ScriptableObject.CreateInstance<UIObject>();
                    uiObject.m_Icon = definition.ui.ImagePath; // e.g. coui://AdvancedRoadTools/images/ZoneControllerTool.svg
                    uiObject.name = definition.ToolID;
                    uiObject.m_IsDebugObject = s_AnchorUI!.m_IsDebugObject;
                    uiObject.m_Priority = definition.Priority; // Original behavior uses the definition's priority
                    uiObject.m_Group = s_AnchorUI!.m_Group;
                    uiObject.active = s_AnchorUI!.active;
                    toolPrefab.AddComponentFrom(uiObject);

                    // Behave like a net upgrade (same as Original).
                    var netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(netUpgrade);

                    // Wire the prefab to the tool system.
                    if (s_World.GetOrCreateSystemManaged(definition.Type) is not ToolBaseSystem toolSystem
                        || !toolSystem.TrySetPrefab(toolPrefab))
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

        private static void SetupToolsOnGameLoaded(Purpose purpose, GameMode mode)
        {
            if (s_PrefabSystem is null || s_AnchorPrefab is null)
            {
                AdvancedRoadToolsMod.s_Log.Error("SetupToolsOnGameLoaded: missing PrefabSystem or anchor prefab.");
                return;
            }

            AdvancedRoadToolsMod.s_Log.Info($"Setting up tools. {s_ToolsLookup.Count} registered tools");

            foreach (var (def, pair) in s_ToolsLookup)
            {
                var prefab = pair.Prefab;

                try
                {
                    // Copy anchor flags (Original approach), then apply tool-specific flags.
                    var placeable = s_PrefabSystem.GetComponentData<PlaceableNetData>(s_AnchorPrefab);
                    placeable.m_SetUpgradeFlags = def.SetFlags;
                    placeable.m_UnsetUpgradeFlags = def.UnsetFlags;
                    placeable.m_PlacementFlags = def.PlacementFlags;

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

        // ---- Anchor resolution (no reflection, public names only) --------------
        private static bool TryResolveAnchor(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;

            if (s_PrefabSystem is null)
                return false;

            // Candidates: prefer these in order. We probe both spaced and compact spellings.
            // (Based on your dnSpy list and in-game labels.)
            var baseNames = new[]
            {
                "Wide Sidewalk",
                "Sound Barrier",
                "Traffic Lights",
                "Crosswalk",
            };

            var nameCandidates = new List<string>(baseNames.Length * 2);
            foreach (var n in baseNames)
            {
                nameCandidates.Add(n);                 // spaced (as shown in UI)
                nameCandidates.Add(n.Replace(" ", "")); // compact
            }

            var typeCandidates = new[]
            {
                "RoadPrefab",
                "NetPrefab",
                "ContentPrefab",
                nameof(PrefabBase),
            };

            foreach (var key in nameCandidates)
            {
                foreach (var typeName in typeCandidates)
                {
#if ART_DIAGNOSTICS
                    AdvancedRoadToolsMod.s_Log.Info($"[ART] Probe anchor: \"{key}\" ({typeName})");
#endif
                    var id = new PrefabID(typeName, key);
                    if (s_PrefabSystem.TryGetPrefab(id, out PrefabBase? p) && p is not null)
                    {
                        if (p.GetComponent<UIObject>() is UIObject u)
                        {
#if ART_DIAGNOSTICS
                            AdvancedRoadToolsMod.s_Log.Info($"[ART] Anchor resolved: {key} ({typeName}), group={u.m_Group?.name ?? "(null)"} prio={u.m_Priority}");
#endif
                            prefab = p;
                            ui = u;
                            return true;
                        }
#if ART_DIAGNOSTICS
                        else
                        {
                            AdvancedRoadToolsMod.s_Log.Warn($"[ART] Prefab \"{key}\" found but has no UIObject; trying next.");
                        }
#endif
                    }
                }
            }

            return false;
        }
    }
}
