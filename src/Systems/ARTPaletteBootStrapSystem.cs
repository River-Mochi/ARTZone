// File: src/Systems/ARTPaletteBootstrapSystem.cs
// Purpose: Wait until RoadsServices anchor exists, then call ToolsHelper.InstantiateTools().
// Arms on OnGameLoadingComplete only when entering a playable city (Game + Load/New).

namespace AdvancedRoadTools.Systems
{
    using AdvancedRoadTools.Tools;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Prefabs;
    using Unity.Entities;

    public sealed partial class ARTPaletteBootstrapSystem : GameSystemBase
    {
        private PrefabSystem? m_Prefabs;
        private bool m_Armed;
        private bool m_Done;
        private int m_Tries;

        private const int kMaxTries = 2000;
        private const int kLogEvery = 50;

#if DEBUG
        private static void dbg(string m)
        {
            try
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Info("[ART][Bootstrap] " + m);
            }
            catch { }
        }
#else
        private static void dbg(string m) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            var world = World.DefaultGameObjectInjectionWorld;
            m_Prefabs = world != null ? world.GetOrCreateSystemManaged<PrefabSystem>() : null;

            m_Armed = false;
            m_Done = false;
            m_Tries = 0;
            Enabled = false; // no per-frame update until we arm
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            // Only arm when actually entering a playable city.
            bool isPlayable = mode == GameMode.Game &&
                              (purpose == Purpose.LoadGame || purpose == Purpose.NewGame);

            if (!isPlayable)
            {
                m_Armed = false;
                m_Done = true;   // OnUpdate becomes a no-op
                m_Tries = 0;
                Enabled = false;
#if DEBUG
                dbg($"GameLoadingComplete(mode={mode}, purpose={purpose}) → not a playable city; staying disarmed.");
#endif
                return;
            }

            m_Armed = true;
            m_Done = false;
            m_Tries = 0;
            Enabled = true;
#if DEBUG
            dbg("Armed; will begin polling for RoadsServices anchors…");
#endif
        }

        protected override void OnUpdate()
        {
            if (!m_Armed || m_Done || m_Prefabs == null)
                return;

            // Probe for a donor tile (Wide Sidewalk / Crosswalk) inside RoadsServices.
            PrefabBase? donor;
            UIObject? donorUI;
            bool ok = ToolsHelper.TryResolveAnchor(m_Prefabs, out donor, out donorUI);

            if (ok && donor != null && donorUI != null)
            {
#if DEBUG
                var groupName = donorUI.m_Group != null ? donorUI.m_Group.name : "(null)";
                dbg($"Donor found: '{donor.name}' Group='{groupName}' Priority={donorUI.m_Priority}");
#endif
                ToolsHelper.InstantiateTools(logIfNoAnchor: true);
                m_Done = true;
                Enabled = false; // stop per-frame work after success
                return;
            }

            m_Tries++;
#if DEBUG
            if ((m_Tries % kLogEvery) == 0)
                dbg($"Still waiting for RoadsServices anchors… tries={m_Tries}");
#endif

            if (m_Tries >= kMaxTries)
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Error("[ART][Bootstrap] Giving up; RoadsServices donors never appeared.");
                m_Armed = false;
                Enabled = false;
            }
        }
    }
}
