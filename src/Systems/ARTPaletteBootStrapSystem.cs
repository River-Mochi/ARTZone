// File: src/Systems/ARTPaletteBootstrapSystem.cs
// Purpose: Wait until a RoadsServices anchor exists, then call ToolsHelper.InstantiateTools().
// Arms only after a real game load (LoadGame/NewGame). Logs periodically in DEBUG.

namespace AdvancedRoadTools.Systems
{
    using AdvancedRoadTools.Tools;
    using Colossal.Serialization.Entities; // Purpose
    using Game;                            // GameMode, GameSystemBase
    using Game.Prefabs;                    // PrefabSystem
    using Unity.Entities;

    public sealed partial class ARTPaletteBootstrapSystem : GameSystemBase
    {
        private PrefabSystem m_Prefabs = null!;
        private bool m_Armed;
        private bool m_Done;
        private int m_Tries;

        private const int kMaxTries = 2000;
        private const int kLogEvery = 50;

#if DEBUG
        private static void dbg(string m)
        {
            var log = AdvancedRoadToolsMod.s_Log;
            if (log != null)
            {
                try
                {
                    log.Info("[ART][Bootstrap] " + m);
                }
                catch { /* swallow */ }
            }
        }
#else
        private static void dbg(string m) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Prefabs = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();
            m_Armed = false;
            m_Done = false;
            m_Tries = 0;

            // Stay idle until a real game load finishes.
            Enabled = false;
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            // Only arm when actually entering a playable game world.
            bool realGame = mode == GameMode.Game &&
                            (purpose == Purpose.LoadGame || purpose == Purpose.NewGame);

            if (!realGame)
            {
                m_Armed = false;
                m_Done = true;
                m_Tries = 0;
                Enabled = false;
#if DEBUG
                dbg($"GameLoadingComplete(mode={mode}, purpose={purpose}) → not gameplay; staying disarmed.");
#endif
                return;
            }

            m_Armed = true;
            m_Done = false;
            m_Tries = 0;
            Enabled = true;

#if DEBUG
            dbg("GameLoadingComplete → armed; will begin polling for RoadsServices anchors…");
#endif
        }

        protected override void OnUpdate()
        {
            if (!m_Armed || m_Done || m_Prefabs == null)
                return;

            // Probe for a donor tile (Wide Sidewalk / Crosswalk) already in RoadsServices.
            if (ToolsHelper.TryResolveAnchor(m_Prefabs, out var donor, out var donorUI))
            {
#if DEBUG
                string groupName = donorUI.m_Group?.name ?? "(null)";
                dbg($"Donor found: '{donor?.name}' Group='{groupName}' Priority={donorUI.m_Priority}");
#endif
                ToolsHelper.InstantiateTools(logIfNoAnchor: true);
                m_Done = true;
                Enabled = false;         // no more per-frame updates needed
                return;
            }

            m_Tries++;
#if DEBUG
            if ((m_Tries % kLogEvery) == 0)
                dbg($"Still waiting for RoadsServices anchors… tries={m_Tries}");
#endif

            if (m_Tries >= kMaxTries)
            {
                AdvancedRoadToolsMod.s_Log?.Error("[ART][Bootstrap] Giving up; RoadsServices donors never appeared.");
                m_Armed = false;
                Enabled = false;
            }
        }
    }
}
