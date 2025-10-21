// File: src/Tools/ToolsHelper.cs
// Purpose: Register and materialize tool prefabs + UI tiles in Road Services.
// - Anchor must be in group "RoadsServices"; new tile gets priority (anchor + 1).
// - Icon path comes from ToolDefinition.UI.ImagePath (set in Mod.cs).
// - No reflective fallback; minimal logging; no diagnostics defines.

using System;
using System.Collections.Generic;
using Colossal.Serialization.Entities;
using Game;
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
        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_Tools = new(4);

        private static bool s_Initialized;
        private static bool s_Instantiated;

        private static World? s_World;
        private static PrefabSystem? s_Prefabs;

        private static PrefabBase? s_AnchorPrefab;
        private static UIObject? s_AnchorUI;

        public static bool HasTool(ToolDefinition def) => ToolDefinitions.Contains(def);
        public static bool HasTool(string id) => ToolDefinitions.Exists(t => t.ToolID == id);

        public static void Initialize(bool force = false)
        {
            if (!s_Initialized || force)
            {
                if (force)
                {
                    ToolDefinitions = new(4);
                    s_Tools.Clear();
                }

                s_World = World.DefaultGameObjectInjectionWorld;
                s_Prefabs = s_World?.GetExistingSystemManaged<PrefabSystem>();
                s_Initialized = true;
            }
        }

        public static void RegisterTool(ToolDefinition def)
        {
            if (!s_Initialized)
                Initialize();

            if (HasTool(def))
            {
                AdvancedRoadToolsMod.s_Log.Warn($"[ART] Tool \"{def.ToolID}\" already registered; skipping.");
                return;
            }

            AdvancedRoadToolsMod.s_Log.Info($"[ART] Register tool \"{def.ToolID}\"");
            ToolDefinitions.Add(def);
        }

        public static void InstantiateTools(bool logIfNoAnchor = true)
        {
            if (!s_Initialized)
                Initialize();
            if (s_Instantiated)
            {
                AdvancedRoadToolsMod.s_Log.Info("[ART] InstantiateTools: already done; skip.");
                return;
            }

            s_World ??= World.DefaultGameObjectInjectionWorld;
            s_Prefabs ??= s_World?.GetExistingSystemManaged<PrefabSystem>();

            if (s_World is null || s_Prefabs is null)
            {
                AdvancedRoadToolsMod.s_Log.Error("[ART] InstantiateTools: missing world or PrefabSystem.");
                return;
            }

            if (ToolDefinitions.Count == 0)
            {
                AdvancedRoadToolsMod.s_Log.Warn("[ART] InstantiateTools: no definitions to create.");
                return;
            }

            if (!TryResolveAnchor(out s_AnchorPrefab!, out s_AnchorUI!))
            {
                if (logIfNoAnchor)
                    AdvancedRoadToolsMod.s_Log.Error("[ART] Could not find a Road Services anchor; tool tiles not created.");
                return;
            }

            foreach (var def in ToolDefinitions)
            {
                try
                {
                    var toolPrefab = Object.Instantiate(s_AnchorPrefab!);
                    toolPrefab.name = def.ToolID;

                    // Remove irrelevant components we don't want to copy as-is
                    toolPrefab.Remove<UIObject>();
                    toolPrefab.Remove<Unlockable>();
                    toolPrefab.Remove<NetSubObjects>();

                    // Create UIObject for the palette tile
                    var ui = ScriptableObject.CreateInstance<UIObject>();
                    ui.name = def.ToolID;
                    ui.m_Icon = def.ui.ImagePath;                // <- icon path set by Mod.cs
                    ui.m_Group = s_AnchorUI!.m_Group;            // must be RoadsServices
                    ui.m_IsDebugObject = s_AnchorUI!.m_IsDebugObject;
                    ui.m_Priority = s_AnchorUI!.m_Priority + 1;  // put us right after anchor
                    ui.active = s_AnchorUI!.active;

                    toolPrefab.AddComponentFrom(ui);

                    // Minimal requirement so the tool can be a prefab tool
                    var upgrade = ScriptableObject.CreateInstance<NetUpgrade>();
                    toolPrefab.AddComponentFrom(upgrade);

                    // Bind prefab to system
                    var sys = s_World!.GetOrCreateSystemManaged(def.Type) as ToolBaseSystem;
                    if (sys == null || !sys.TrySetPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"[ART] Failed binding prefab for \"{def.ToolID}\"");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    if (!s_Prefabs!.AddPrefab(toolPrefab))
                    {
                        AdvancedRoadToolsMod.s_Log.Error($"[ART] Could not add \"{def.ToolID}\" to PrefabSystem");
                        Object.DestroyImmediate(toolPrefab);
                        continue;
                    }

                    s_Tools[def] = (toolPrefab, ui);
                    AdvancedRoadToolsMod.s_Log.Info($"[ART] Tool \"{def.ToolID}\" created in RoadsServices with priority {ui.m_Priority}");
                }
                catch (Exception ex)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"[ART] Error creating tool \"{def.ToolID}\": {ex}");
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
                AdvancedRoadToolsMod.s_Log.Error("[ART] SetupToolsOnGameLoaded: PrefabSystem or anchor missing.");
                return;
            }

            AdvancedRoadToolsMod.s_Log.Info($"[ART] Finalize tool setup ({s_Tools.Count} items).");

            foreach (var kv in s_Tools)
            {
                var def = kv.Key;
                var prefab = kv.Value.Prefab;

                try
                {
                    // Copy basic placeable flags from anchor and apply the definition overrides
                    var placeable = s_Prefabs.GetComponentData<PlaceableNetData>(s_AnchorPrefab);
                    placeable.m_SetUpgradeFlags = def.SetFlags;
                    placeable.m_UnsetUpgradeFlags = def.UnsetFlags;
                    placeable.m_PlacementFlags = def.PlacementFlags;
                    if (def.Underground)
                        placeable.m_PlacementFlags |= Game.Net.PlacementFlags.UndergroundUpgrade;

                    s_Prefabs.AddComponentData(prefab, placeable);
                }
                catch (Exception ex)
                {
                    AdvancedRoadToolsMod.s_Log.Error($"[ART] Could not finalize \"{def.ToolID}\": {ex}");
                }
            }
        }

        // ---- Anchor resolution: probe a few common names, but REQUIRE group == RoadsServices ----
        private static bool TryResolveAnchor(out PrefabBase prefab, out UIObject ui)
        {
            prefab = null!;
            ui = null!;

            if (s_Prefabs is null)
                return false;

            // Minimal, high-confidence anchors in Road Services
            var candidates = new[]
            {
                "Wide Sidewalk",
                "Sound Barrier",
                "Grass",
                "Trees",
            };

            foreach (var name in candidates)
            {
                var id = new PrefabID(nameof(PrefabBase), name);
                if (s_Prefabs.TryGetPrefab(id, out PrefabBase? p) && p is not null)
                {
                    var u = p.GetComponent<UIObject>();
                    if (u != null && u.m_Group != null && string.Equals(u.m_Group.name, "RoadsServices", StringComparison.Ordinal))
                    {
                        prefab = p;
                        ui = u;
                        AdvancedRoadToolsMod.s_Log.Info($"[ART] Anchor: \"{name}\" in group=RoadsServices prio={u.m_Priority}");
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
