// File: src/Tools/ToolBootstrapSystem.cs
// Purpose:
//   After vanilla prefabs & UI exist, attach a UIObject to our tool prefab so it
//   shows in the Road Services palette (group="RoadsServices") right after a donor
//   item (e.g., "Wide Sidewalk"). Runs once and disables itself.

namespace ARTZone.Tools
{
    using Game;
    using Game.Prefabs;   // PrefabSystem, PrefabBase
    using Game.UI;        // UIObject
    using Unity.Collections;
    using Unity.Entities;

    public partial class ToolBootstrapSystem : GameSystemBase
    {
        private PrefabSystem _prefabSystem = null!;
        private bool _done;

        protected override void OnCreate()
        {
            base.OnCreate();
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            _done = false;
        }

        protected override void OnUpdate()
        {
            if (_done)
                return;

            // Our tool prefab is created when ZoningControllerToolSystem registers its ToolDefinition.
            var ourPrefab = TryFindPrefabByName(ZoningControllerToolSystem.ToolID);
            if (ourPrefab == null)
            {
                // Not ready yet; try again next frame.
                return;
            }

            // Try to attach a palette tile after the first available donor in this list.
            var donors = new[] { "Wide Sidewalk", "Grass", "Sound Barrier", "Crosswalk", "Quay" };
            bool ok = TryAttachPaletteTileAfterDonor(_prefabSystem, ourPrefab, donors);

            if (!ok)
                ARTZoneMod.s_Log.Warn("[ART] No donor in RoadsServices found; palette tile not created.");

            _done = true; // one-shot; we did our job.
        }

        private PrefabBase? TryFindPrefabByName(string name)
        {
            try
            {
                // Most builds provide this generic lookup:
                var p = _prefabSystem.FindPrefab<PrefabBase>(name);
                if (p != null)
                    return p;
            }
            catch
            {
                // Fall through to slow path
            }

            // Fallback: scan for a PrefabRef with a UIObject
            var q = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PrefabRef, UIObject>()
                .Build(this);

            using var entities = q.ToEntityArray(Allocator.Temp);
            var prefLookup = GetComponentLookup<PrefabRef>(true);

            foreach (var e in entities)
            {
                var pr = prefLookup[e];
                if (pr.m_Prefab != null && pr.m_Prefab.name == name)
                    return pr.m_Prefab;
            }

            return null;
        }

        // --- Your helper, wrapped here so it compiles and is reusable ---

        private static bool TryAttachPaletteTileAfterDonor(
            PrefabSystem prefabSystem,
            PrefabBase ourPrefab,
            string[] donorNamesOrdered)
        {
            // 1) Ensure our prefab has a UIObject
            var ui = ourPrefab.GetComponent<UIObject>();
            if (ui == null)
            {
                ui = ourPrefab.AddComponent<UIObject>();
                ui.m_Priority = 0;
                ui.m_Group = ""; // set below
            }

            // 2) Find a donor in RoadsServices
            UIObject? donorUI = null;
            foreach (string donorName in donorNamesOrdered)
            {
                var donor = prefabSystem.FindPrefab<PrefabBase>(donorName);
                if (donor == null)
                    continue;

                var dui = donor.GetComponent<UIObject>();
                if (dui == null)
                    continue;

                if (string.Equals(dui.m_Group, "RoadsServices", System.StringComparison.OrdinalIgnoreCase))
                {
                    donorUI = dui;
                    ARTZoneMod.s_Log.Info($"[ART] Using donor \"{donorName}\" group={dui.m_Group} prio={dui.m_Priority}");
                    break;
                }
            }

            if (donorUI == null)
                return false;

            // 3) Copy donor group, place just after donor
            ui.m_Group = donorUI.m_Group;           // "RoadsServices"
            ui.m_Priority = donorUI.m_Priority + 1; // appear right after donor
            ui.m_UITag = "ARTZone:ZoneController";  // optional tag for debugging

            ARTZoneMod.s_Log.Info($"[ART] UI tile attached: group={ui.m_Group} prio={ui.m_Priority}");
            return true;
        }
    }
}
