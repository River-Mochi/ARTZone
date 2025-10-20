// File: src/Tools/ToolBootstrapSystem.cs
// Purpose:
//   After vanilla prefabs & UI exist, attach a UIObject to our tool prefab so it
//   shows in the Road Services palette right after a donor item (e.g., "Wide Sidewalk").
//   Uses only public PrefabSystem APIs (no FindPrefab; no UIObject.m_UITag). One-shot.

namespace ARTZone.Tools
{
    using Game;
    using Game.Prefabs;   // PrefabSystem, PrefabBase, UIObject, PrefabRef
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

            var ourPrefab = TryFindPrefabByName(ZoningControllerToolSystem.ToolID);
            if (ourPrefab == null)
            {
                // Not created yet; try again next frame.
                return;
            }

            // Prefer items you listed; first found wins.
            var donors = new[] { "Wide Sidewalk", "Grass", "Sound Barrier", "Crosswalk", "Quay" };
            bool ok = TryAttachPaletteTileAfterDonor(_prefabSystem, ourPrefab, donors);

            if (!ok)
                ARTZoneMod.s_Log.Warn("[ART] No donor in RoadsServices found; palette tile not created.");

            _done = true; // run once
        }

        // Find a PrefabBase by its name by scanning PrefabRef+UIObject entities.
        private PrefabBase? TryFindPrefabByName(string name)
        {
            var q = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PrefabRef, UIObject>()
                .Build(this);

            using var entities = q.ToEntityArray(Allocator.Temp);
            foreach (var e in entities)
            {
                // Compare prefab name via API (entity itself has no .name)
                string prefabName = _prefabSystem.GetPrefabName(e);
                if (prefabName == name)
                {
                    return _prefabSystem.GetPrefab<PrefabBase>(e);
                }
            }
            return null;
        }

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
                // ui.m_Group will be copied from donor below (UIGroupPrefab, not string).
            }

            // 2) Find a donor with UIObject in the RoadsServices group
            UIObject? donorUI = null;

            var q = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PrefabRef, UIObject>()
                .Build(prefabSystem);

            using var entities = q.ToEntityArray(Allocator.Temp);
            var prLookup = prefabSystem.GetComponentLookup<PrefabRef>(true);

            foreach (string donorName in donorNamesOrdered)
            {
                foreach (var e in entities)
                {
                    string prefabName = prefabSystem.GetPrefabName(e);
                    if (prefabName != donorName)
                        continue;

                    var pr = prLookup[e];
                    var donor = prefabSystem.GetPrefab<PrefabBase>(pr);
                    var dui = donor.GetComponent<UIObject>();
                    if (dui != null && dui.m_Group != null && dui.m_Group.name == "RoadsServices")
                    {
                        donorUI = dui;
                        ARTZoneMod.s_Log.Info($"[ART] Using donor \"{donorName}\" group={dui.m_Group.name} prio={dui.m_Priority}");
                        break;
                    }
                }
                if (donorUI != null)
                    break;
            }

            if (donorUI == null)
                return false;

            // 3) Copy donor group, place just after donor
            ui.m_Group = donorUI.m_Group;            // UIGroupPrefab
            ui.m_Priority = donorUI.m_Priority + 1;    // appear right after donor

            ARTZoneMod.s_Log.Info($"[ART] UI tile attached: group={ui.m_Group.name} prio={ui.m_Priority}");
            return true;
        }
    }
}
