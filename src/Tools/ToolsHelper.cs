// File: src/Tools/ToolsHelper.cs
// Original behavior, no Harmony. Broader anchor probe + DEBUG-only reflective fallback.
//
// - Don’t wipe ToolDefinitions after registration.
// - Try spaced + compact names for many Road Services items.
// - If not found, DEBUG scan PrefabSystem.m_Prefabs and auto-pick a good anchor.
// - Clone anchor, copy UI group, use definition.Priority, add NetUpgrade, copy PlaceableNetData.
//
// Build: define ART_DIAGNOSTICS in Debug (your csproj already does).

#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using Colossal.Serialization.Entities;
using Game;
using Game.Net;
using Game.Prefabs;
using Game.SceneFlow;
using Game.Tools;
using Unity.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ARTZone.Tools
{
    public static class ToolsHelper
    {
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(8);
        public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
        public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

        public static void RegisterTool(ToolDefinition toolDefinition)
        {
            if (HasTool(toolDefinition))
            {
                ARTZoneMod.s_Log.Error($"Tool \"{toolDefinition.ToolID}\" already registered");
                return;
            }
            ARTZoneMod.s_Log.Info($"Registering tool \"{toolDefinition.ToolID}\" with system \"{toolDefinition.Type.Name}\"");
            ToolDefinitions.Add(toolDefinition);
        }

        // ---- internals ----
        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_ToolsLookup = new(8);

        public static bool Initialized
        {
            get; private set;
        }
        private static bool s_Instantiated;

        private static World? s_World;
        private static PrefabSystem? s_PrefabSystem;

        private static PrefabBase? s_AnchorPrefab;
        private static UIObject? s_AnchorUI;

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

        public static void InstantiateTools(bool logIfNoAnchor = true)
        {
            if (!Initialized)
                Initialize();

            if (s_Instantiated)
            {
                ARTZoneMod.s_Log.Info("InstantiateTools skipped (already instantiated).");
                return;
            }

            s_World ??= World.DefaultGameObjectInjectionWorld;
            if (s_World is null)
            {
                ARTZoneMod.s_Log.Error("InstantiateTools: DefaultGameObjectInjectionWorld is null.");
                return;
            }

            s_PrefabSystem ??= s_World.GetExistingSystemManaged<PrefabSystem>();
            if (s_PrefabSystem is null)
            {
                ARTZoneMod.s_Log.Error("InstantiateTools: PrefabSystem not available.");
                return;
            }

            ARTZoneMod.s_Log.Info($"Creating tools UI. {ToolDefinitions.Count} registered tools");
            if (ToolDefinitions.Count == 0)
            {
                ARTZoneMod.s_Log.Error("[ART] No tools are registered. Ensure RegisterTool(...) ran before InstantiateTools().");
                return;
            }


            if (!TryResolveAnchor(out s_AnchorPrefab!, out s_AnchorUI!))
            {
                if (logIfNoAnchor)
                    ARTZoneMod.s_Log.Error("Could not find Road Services anchor. Tools will not be created.");
                return;
            }

            foreach (ToolDefinition definition in ToolDefinitions)
            {
                try
                {
                    PrefabBase toolPrefab = Object.Instantiate(s_AnchorPrefab!);
                    toolPrefab.name = definition.ToolID;

                    toolPrefab.Remove<UIObject>();
                    toolPrefab.Remove<Unlockable>();
                    toolPrefab.Remove<NetSubObjects>();

                    UIObject uiObject = ScriptableObject.CreateInstance<UIObject>();

                    // Road Services Palette tile icon – served by the webpack host
                    uiObject.m_Icon = definition.ui.ImagePath;   // use the path from ToolDefinition
                    uiObject.name = definition.ToolID;
                    uiObject.m_IsDebugObject = s_AnchorUI!.m_IsDebugObject;
                    uiObject.m_Priority = definition.Priority;        // Original behavior
                    uiObject.m_Group = s_AnchorUI!.m_Group;
                    uiObject.active = s_AnchorUI!.active;
                    toolPrefab.AddComponentFrom(uiObject);

                    NetUpgrade netUpgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(netUpgrade);

                    if (s_World.GetOrCreateSystemManaged(definition.Type) is not ToolBaseSystem toolSystem
                        || !toolSystem.TrySetPrefab(toolPrefab))
                    {
                        ARTZoneMod.s_Log.Error($"Failed to set up tool prefab for type \"{definition.Type}\"");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    if (!s_PrefabSystem.AddPrefab(toolPrefab))
                    {
                        ARTZoneMod.s_Log.Error($"Tool \"{definition.ToolID}\" could not be added to {nameof(PrefabSystem)}");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    s_ToolsLookup[definition] = (toolPrefab, uiObject);
                    ARTZoneMod.s_Log.Info($"\tTool \"{definition.ToolID}\" was successfully created");
                }
                catch (Exception e)
                {
                    ARTZoneMod.s_Log.Error($"\tTool \"{definition.ToolID}\" could not be created: {e}");
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
                ARTZoneMod.s_Log.Error("SetupToolsOnGameLoaded: missing PrefabSystem or anchor prefab.");
                return;
            }

            ARTZoneMod.s_Log.Info($"Setting up tools. {s_ToolsLookup.Count} registered tools");

            foreach ((ToolDefinition def, (PrefabBase Prefab, UIObject UI) pair) in s_ToolsLookup)
            {
                PrefabBase prefab = pair.Prefab;
                try
                {
                    PlaceableNetData placeable = s_PrefabSystem.GetComponentData<PlaceableNetData>(s_AnchorPrefab);
                    placeable.m_SetUpgradeFlags = def.SetFlags;
                    placeable.m_UnsetUpgradeFlags = def.UnsetFlags;
                    placeable.m_PlacementFlags = def.PlacementFlags;

                    if (def.Underground)
                        placeable.m_PlacementFlags |= PlacementFlags.UndergroundUpgrade;

                    s_PrefabSystem.AddComponentData(prefab, placeable);
                }
                catch (Exception e)
                {
                    ARTZoneMod.s_Log.Error($"\tCould not setup tool {def.ToolID}: {e}");
                }
            }
        }

        // -------- Anchor resolution ----------
        private static bool TryResolveAnchor(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;

            if (s_PrefabSystem is null)
                return false;

            // Your Road Services names (probe spaced + compact)
            var uiNames = new[]
            {
                "Wide Sidewalk",
                "Sound Barrier",
                "Traffic Lights",
                "Crosswalk",
                "Quay",
                "Retaining Wall",
                "Elevated",
                "Tunnel",
                "Road Maintenance Depot",

            };

            var candidates = new List<string>(uiNames.Length * 2);
            foreach (var n in uiNames)
            {
                candidates.Add(n);
                candidates.Add(n.Replace(" ", "")); // compact
            }

            var types = new[]
            {
                "RoadPrefab",
                "NetPrefab",
                "ContentPrefab",
                nameof(PrefabBase),
            };

#if ART_DIAGNOSTICS
            ARTZoneMod.s_Log.Info("[ART] Begin anchor probe (Road Services candidates) …");
#endif
            foreach (var name in candidates)
            {
                foreach (var type in types)
                {
#if ART_DIAGNOSTICS
                    ARTZoneMod.s_Log.Info($"[ART] Probe anchor: \"{name}\" ({type})");
#endif
                    var id = new PrefabID(type, name);
                    if (s_PrefabSystem.TryGetPrefab(id, out PrefabBase? p) && p is not null)
                    {
                        UIObject? u = p.GetComponent<UIObject>();
                        if (u is not null)
                        {
#if ART_DIAGNOSTICS
                            ARTZoneMod.s_Log.Info($"[ART] Anchor resolved: {name} ({type}), group={u.m_Group?.name ?? "(null)"} prio={u.m_Priority}");
#endif
                            prefab = p;
                            ui = u;
                            return true;
                        }
#if ART_DIAGNOSTICS
                        else
                        {
                            ARTZoneMod.s_Log.Warn($"[ART] Prefab \"{name}\" found but has no UIObject.");
                        }
#endif
                    }
                }
            }

            // DEBUG-only reflective fallback: enumerate PrefabSystem’s list and auto-pick.
#if ART_DIAGNOSTICS
            if (TryDebugReflectivePick(out prefab, out ui))
            {
                return true;
            }
#endif
            return false;
        }

#if ART_DIAGNOSTICS
        private static bool TryDebugReflectivePick(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;

            try
            {
                if (s_PrefabSystem is null)
                    return false;

                FieldInfo field = s_PrefabSystem.GetType().GetField("m_Prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null)
                {
                    ARTZoneMod.s_Log.Warn("[ART] Reflective scan: cannot find m_Prefabs.");
                    return false;
                }

                if (field.GetValue(s_PrefabSystem) is not System.Collections.IEnumerable list)
                    return false;

                ARTZoneMod.s_Log.Info("[ART] Reflective scan: listing prefabs with UIObject …");

                var bestScore = int.MinValue;
                PrefabBase? best = null;
                UIObject? bestUI = null;

                foreach (var obj in list)
                {
                    if (obj is not PrefabBase p)
                        continue;

                    UIObject? u = p.GetComponent<UIObject>();
                    if (u is null)
                        continue;

                    var name = p.name ?? "";
                    var norm = Normalize(name);
                    var groupName = u.m_Group != null ? u.m_Group.name ?? "" : "";

                    ARTZoneMod.s_Log.Info($"[ART] UI Prefab: name=\"{name}\" group={groupName} prio={u.m_Priority}");

                    var score = ScoreForRoadServices(norm, groupName);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = p;
                        bestUI = u;
                    }
                }

                if (best != null && bestUI != null && bestScore >= 50)
                {
                    ARTZoneMod.s_Log.Info($"[ART] Reflective pick: \"{best.name}\" group={bestUI.m_Group?.name ?? "(null)"} prio={bestUI.m_Priority} (score {bestScore})");
                    prefab = best;
                    ui = bestUI;
                    return true;
                }

                ARTZoneMod.s_Log.Warn("[ART] Reflective scan found no strong Road Services anchor.");
                return false;
            }
            catch (Exception ex)
            {
                ARTZoneMod.s_Log.Error($"[ART] Reflective scan error: {ex}");
                return false;
            }
        }

        private static int ScoreForRoadServices(string normName, string groupName)
        {
            // Very simple, portable scoring (no culture-specific APIs).
            // Prefer obvious Road Services items.
            if (normName.Contains("widesidewalk"))
                return 100;
            if (normName.Contains("soundbarrier"))
                return 95;
            if (normName.Contains("trafficlights"))
                return 90;
            if (normName.Contains("crosswalk"))
                return 85;

            if (normName.Contains("retainingwall"))
                return 70;
            if (normName.Contains("quay"))
                return 65;
            if (normName.Contains("tunnel"))
                return 65;
            if (normName.Contains("lighting"))
                return 60;
            if (normName.Contains("trees"))
                return 60;
            if (normName.Contains("grass"))
                return 58;
            if (normName.Contains("roadmaintenance"))
                return 58;
            if (normName.Contains("stopsigns"))
                return 58;

            // Nudge if the group label smells like services.
            var g = Normalize(groupName);
            if (g.Contains("service"))
                return 55;
            if (g.Contains("road"))
                return 52;

            return 0;
        }
#endif

        private static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            // lower, remove spaces/underscores only – keep it portable.
            s = s.ToLowerInvariant();
            s = s.Replace(" ", "");
            s = s.Replace("_", "");
            return s;
        }
    }
}
