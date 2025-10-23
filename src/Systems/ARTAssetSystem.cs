// File: src/Systems/ArtAssetSystem.cs
// Purpose: PAF-style owner system. Owns PrefabSystem; finds Road Services donor(s)
//          at the right time and instantiates our tool prefab via ToolsHelper.

using Colossal.Serialization.Entities;
using Game;
using Game.Prefabs;
using Unity.Entities;

namespace AdvancedRoadTools.Tools
{
    public sealed partial class ArtAssetSystem : GameSystemBase
    {
        private PrefabSystem _prefabSystem = null!;
        private bool _armed;           // set after OnGameLoadingComplete
        private bool _instantiated;    // set after ToolsHelper.InstantiateTools succeeds
        private int _tries;           // retry counter
        private const int MaxTries = 600; // ~10s @ 60fps (guard against slow loads)

        protected override void OnCreate()
        {
            base.OnCreate();
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);
#if DEBUG
            AdvancedRoadToolsMod.s_Log.Info("[ART] ArtAssetSystem: OnGameLoadingComplete — arming prefab init");
#endif
            _armed = true;
            _instantiated = false;
            _tries = 0;
        }

        protected override void OnUpdate()
        {
            if (!_armed || _instantiated)
                return;

            // Try until anchors exist (PAF-style robustness).
            if (AnchorsLikelyAvailable())
            {
#if DEBUG
                AdvancedRoadToolsMod.s_Log.Info($"[ART] ArtAssetSystem: anchors available after {_tries} tries; instantiating tools");
#endif
                ToolsHelper.Initialize(force: false);
                ToolsHelper.InstantiateTools(logIfNoAnchor: true);
                _instantiated = true;
                return;
            }

            _tries++;
            if (_tries % 60 == 0) // once a second
            {
#if DEBUG
                AdvancedRoadToolsMod.s_Log.Info($"[ART] ArtAssetSystem: waiting for anchors… try #{_tries}");
#endif
            }
            if (_tries > MaxTries)
            {
                AdvancedRoadToolsMod.s_Log.Error("[ART] ArtAssetSystem: giving up looking for anchors (timeout)");
                _armed = false;
            }
        }

        // Lightweight readiness probe so we don't spam Instantiation until donors exist.
        private bool AnchorsLikelyAvailable()
        {
            if (_prefabSystem == null)
                return false;

            return TryHasUIObject(new PrefabID("PrefabBase", "Wide Sidewalk"))
                 || TryHasUIObject(new PrefabID("PrefabBase", "Crosswalk"))
                 || TryHasUIObject(new PrefabID("PrefabBase", "Sound Barrier")); // safe fallback
        }

        private bool TryHasUIObject(PrefabID id)
        {
            if (_prefabSystem.TryGetPrefab(id, out PrefabBase p) && p != null)
            {
                bool ok = p.TryGet(out UIObject _);
#if DEBUG
                AdvancedRoadToolsMod.s_Log.Info($"[ART] Probe {id.GetName()}: prefab {(p != null ? "found" : "missing")}, UIObject {(ok ? "present" : "absent")}");
#endif
                return ok;
            }
#if DEBUG
            AdvancedRoadToolsMod.s_Log.Info($"[ART] Probe {id.GetName()}: prefab missing");
#endif
            return false;
        }
    }
}
