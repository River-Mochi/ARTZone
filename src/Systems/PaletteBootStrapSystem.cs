// File: src/Systems/ARTPaletteBootstrapSystem.cs
// Purpose:
//      Only Job: wait until a RoadsServices donor/anchor prefab exists, then call builder's PaletteBuilder.InstantiateTools()
//      Arms only after a real game load, and turn off after calling builder.
//         BootstrapSystem = waiter (when is it safe?)
//         PaletteBuilder  = worker (do the cloning now)

namespace ARTZone.Systems
{
    using Colossal.Serialization.Entities; // Purpose, GameMode
    using Game;
    using Game.Prefabs;
    using Unity.Entities;

    public sealed partial class PaletteBootstrapSystem : GameSystemBase
    {
        // --- RETRY TUNING ----------------------------------------------------
        private const int MaxTries = 2000;    // Poll up to kMaxTries frames looking for a donor tile.
        private const int LogEvery = 50;      // Log every kLogEvery tries in DEBUG.

        // --- State -----------------------------------------------------------
        private PrefabSystem m_Prefabs = null!;
        private bool m_Armed;
        private bool m_Done;
        private int m_Tries;

#if DEBUG
        private static void Dbg(string msg)
        {
            var log = ARTZoneMod.s_Log;
            if (log != null)
            {
                try
                {
                    log.Info("[ART][Bootstrap] " + msg);
                }
                catch { }
            }
        }
#else
        private static void Dbg(string msg) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Prefabs = World.DefaultGameObjectInjectionWorld
                .GetOrCreateSystemManaged<PrefabSystem>();

            m_Armed = false;
            m_Done = false;
            m_Tries = 0;

            // Stay off until we're actually in a playable map.
            Enabled = false;
        }

        // Called by the engine when a game/save finishes loading.
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            // Only arm when entering an actual playable city, not main menu, editor, etc.
            bool realGame =
                mode == GameMode.Game &&
                (purpose == Purpose.LoadGame || purpose == Purpose.NewGame);

            if (!realGame)
            {
                m_Armed = false;
                m_Done = true;
                m_Tries = 0;
                Enabled = false;
#if DEBUG
                Dbg($"OnGameLoadingComplete(mode={mode}, purpose={purpose}) → not gameplay; staying disarmed.");
#endif
                return;
            }

            // Arm and start polling each frame.
            m_Armed = true;
            m_Done = false;
            m_Tries = 0;
            Enabled = true;

#if DEBUG
            Dbg("OnGameLoadingComplete → armed; will begin polling for RoadsServices donor …");
#endif
        }

        protected override void OnUpdate()
        {
            // If we're not armed, or already done, or missing PrefabSystem, nothing to do.
            if (!m_Armed || m_Done || m_Prefabs == null)
                return;

            // First: can we resolve a donor?
            if (PaletteBuilder.TryResolveDonor(m_Prefabs, out PrefabBase? donor, out UIObject? donorUI))
            {
#if DEBUG
                if (donorUI != null)
                {
                    string groupName = (donorUI.m_Group != null)
                        ? donorUI.m_Group.name
                        : "(null)";

                    Dbg($"Donor found: '{(donor != null ? donor.name : "(null)")}' group='{groupName}' priority={donorUI.m_Priority}");
                }
#endif
                // We have a donor, now build tiles.
                PaletteBuilder.InstantiateTools(logIfNoDonor: true);

                // We're done bootstrapping. Turn this system off.
                m_Done = true;
                Enabled = false;
                return;
            }

            // Still waiting for donor.
            m_Tries++;

#if DEBUG
            if ((m_Tries % LogEvery) == 0)
                Dbg($"Still waiting for RoadsServices donor… tries={m_Tries}");
#endif

            if (m_Tries >= MaxTries)
            {
                ARTZoneMod.s_Log.Error("[ART][Bootstrap] Giving up; RoadsServices donor never appeared.");
                m_Armed = false;
                Enabled = false;
            }
        }
    }
}
