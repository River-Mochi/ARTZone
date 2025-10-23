// File: src/Tools/ToolsHelper.cs
// Purpose: Duplicate a RoadsServices donor (Wide Sidewalk / Crosswalk) and create our tile
//          with the same UI group and priority+1. Donor prefab is never modified.
// Notes:
//   • Primary lookup: PrefabID("FencePrefab","Wide Sidewalk") then ("FencePrefab","Crosswalk").
//   • If both miss, DEBUG-only reflection scan over PrefabSystem to find best RoadsServices candidate.
//   • DuplicatePrefab + AddComponentFrom(UIObject, NetUpgrade) + UpdatePrefab to lock in components.
//   • Icon path comes from definition.ui.ImagePath.
//   • Cached donor so we don’t keep probing; guarded DEBUG logging; no #nullable directive used.

namespace AdvancedRoadTools.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Net;          // PlacementFlags
    using Game.Prefabs;
    using Game.SceneFlow;
    using Game.Tools;
    using Unity.Entities;
    using UnityEngine;
#if DEBUG
    using System.Reflection;
    using AdvancedRoadTools.Systems;
#endif

    public static class ToolsHelper
    {
        public static List<ToolDefinition> ToolDefinitions { get; private set; } = new(4);
        public static bool HasTool(ToolDefinition tool) => ToolDefinitions.Contains(tool);
        public static bool HasTool(string toolId) => ToolDefinitions.Exists(t => t.ToolID == toolId);

        private static readonly Dictionary<ToolDefinition, (PrefabBase Prefab, UIObject UI)> s_ToolsLookup = new(4);

        // Make these nullable and ALWAYS null-check before use → no CS8618 and no CS8602
        private static World? s_World;
        private static PrefabSystem? s_PrefabSystem;

        // Cached donor from RoadsServices (kept for placeable copy)
        private static PrefabBase? s_AnchorPrefab;
        private static UIObject? s_AnchorUI;

        // Allows bootstrapper to know when a tile has been created.
        public static bool IsReady => s_Instantiated && s_ToolsLookup.Count > 0;
        private static bool s_Instantiated;

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
            catch { /* swallow early logger hiccups */ }
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
            s_PrefabSystem = (s_World != null) ? s_World.GetExistingSystemManaged<PrefabSystem>() : null;
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
            s_PrefabSystem ??= (s_World != null) ? s_World.GetExistingSystemManaged<PrefabSystem>() : null;

            if (s_PrefabSystem == null)
            {
                AdvancedRoadToolsMod.s_Log.Error("[ART][Tools] PrefabSystem not available.");
                return;
            }

            // Resolve donor (cached on success). Out params are nullable to avoid CS8625 on failure path.
            if ((s_AnchorPrefab == null || s_AnchorUI == null) &&
                !TryResolveAnchor(s_PrefabSystem, out s_AnchorPrefab, out s_AnchorUI))
            {
                if (logIfNoAnchor)
                    AdvancedRoadToolsMod.s_Log.Error("[ART][Tools] Could not find RoadsServices anchor. Will retry later.");
                return;
            }

            // Local non-null copies for clarity (we only get here when anchor is found)
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
                    uiObject.m_Icon = definition.ui.ImagePath;            // single source of truth
                    uiObject.m_IsDebugObject = anchorUI.m_IsDebugObject;
                    uiObject.m_Priority = anchorUI.m_Priority + 1;
                    uiObject.m_Group = anchorUI.m_Group;                  // "RoadsServices"
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
        /// Probe for a stable RoadsServices donor (FencePrefab/Wide Sidewalk primary, Crosswalk fallback).
        /// Returns true only when both prefab & UIObject are found and group == "RoadsServices".
        /// Caches success so we don’t keep probing.
        /// </summary>
        public static bool TryResolveAnchor(PrefabSystem prefabSystem, out PrefabBase? prefab, out UIObject? ui)
        {
            prefab = null;
            ui = null;

            if (prefabSystem == null)
                return false;

            // 0) Return cached match immediately
            if (s_AnchorPrefab != null && s_AnchorUI != null)
            {
#if DEBUG
                var cachedGroup = s_AnchorUI.m_Group != null ? s_AnchorUI.m_Group.name : "(null)";
                Dbg($"[Anchor] cached: {s_AnchorPrefab.name}  group='{cachedGroup}'");
#endif
                prefab = s_AnchorPrefab;
                ui = s_AnchorUI;
                return true;
            }

            // 1) Locked donors (type+name exact)
            var locked = new (string typeName, string name)[]
            {
                ("FencePrefab", "Wide Sidewalk"),
                ("FencePrefab", "Crosswalk"),
            };

            for (int i = 0; i < locked.Length; i++)
            {
                var typeName = locked[i].typeName;
                var name = locked[i].name;
                var id = new PrefabID(typeName, name);

                PrefabBase? candidate;
                bool found = prefabSystem.TryGetPrefab(id, out candidate) && candidate is not null;

#if DEBUG
                Dbg($"Probe {typeName}:{name}: {(found ? "FOUND" : "missing")}");
#endif
                if (!found)
                    continue;

                UIObject? candidateUI;
                bool hasUI = candidate!.TryGet(out candidateUI) && candidateUI is not null;
                if (!hasUI)
                {
#if DEBUG
                    Dbg("  …found prefab but it has no UIObject → skip");
#endif
                    continue;
                }

                string groupName = candidateUI!.m_Group != null ? candidateUI.m_Group.name : "(null)";
                if (!string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase))
                {
#if DEBUG
                    Dbg($"  …UIObject group is '{groupName}', not 'RoadsServices' → skip");
#endif
                    continue;
                }

#if DEBUG
                Dbg($"Anchor OK (locked): {name}  group='{groupName}'  priority={candidateUI.m_Priority}");
#endif
                s_AnchorPrefab = candidate;
                s_AnchorUI = candidateUI;
                prefab = candidate;
                ui = candidateUI;
                return true;
            }

            // 2) DEBUG-only scan (reflection) — only if locked donors fail
#if DEBUG
            try
            {
                var all = GetAllPrefabsUnsafe(prefabSystem);
                PrefabBase? bestP = null;
                UIObject? bestU = null;
                int bestScore = int.MinValue;

                foreach (var p in all)
                {
                    if (p == null)
                        continue;

                    UIObject? uComp;
                    if (!p.TryGet(out uComp) || uComp == null)
                        continue;

                    string groupName = uComp.m_Group != null ? uComp.m_Group.name : "(null)";
                    if (!string.Equals(groupName, "RoadsServices", StringComparison.OrdinalIgnoreCase))
                        continue;

                    int score = 0;
                    string n = p.name ?? string.Empty;
                    if (n.IndexOf("Wide Sidewalk", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 1000;
                    else if (n.IndexOf("Crosswalk", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 900;
                    else if (n.IndexOf("Grass", StringComparison.OrdinalIgnoreCase) >= 0)
                        score += 800;

                    // small bias by priority (higher is better)
                    score += uComp.m_Priority;

                    bool hasPlaceable = prefabSystem.TryGetComponentData(p, out PlaceableNetData _);
                    Dbg($"Scan candidate: {p.GetType().Name}:{n}  score={score}  group='{groupName}'  hasPlaceable={hasPlaceable}  priority={uComp.m_Priority}");

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestP = p;
                        bestU = uComp;
                    }
                }

                if (bestP != null && bestU != null)
                {
                    s_AnchorPrefab = bestP;
                    s_AnchorUI = bestU;
                    prefab = bestP;
                    ui = bestU;
                    var g = bestU.m_Group != null ? bestU.m_Group.name : "(null)";
                    Dbg($"Anchor OK (scan): {bestP.name}  group='{g}'  priority={bestU.m_Priority}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Dbg($"DEBUG scan failed: {ex}");
            }
#endif

            // Not found (out params remain null by design)
            return false;
        }

#if DEBUG
        // DEBUG helper: reflect PrefabSystem to get the raw list of prefabs.
        private static List<PrefabBase> GetAllPrefabsUnsafe(PrefabSystem ps)
        {
            // Prefer a hidden property if present
            try
            {
                var prop = typeof(PrefabSystem).GetProperty("prefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    var ie = prop.GetValue(ps) as IEnumerable<PrefabBase>;
                    if (ie != null)
                        return new List<PrefabBase>(ie);
                }
            }
            catch { }

            // Fallback to known private field (dnSpy)
            try
            {
                var fi = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.NonPublic | BindingFlags.Instance);
                var list = fi != null ? (fi.GetValue(ps) as List<PrefabBase>) : null;
                return (list != null) ? new List<PrefabBase>(list) : new List<PrefabBase>(0);
            }
            catch
            {
                return new List<PrefabBase>(0);
            }
        }
#endif
    }
}
