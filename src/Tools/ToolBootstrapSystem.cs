// File: src/Tools/ToolBootstrapSystem.cs
// Purpose: Retry ToolsHelper.InstantiateTools() until our tool prefab exists,
//          then disable. Uses DOTS World.Time (no UnityEngine).

namespace ARTZone.Tools
{
    using Game;              // GameSystemBase
    using Unity.Entities;    // World.Time

    public sealed partial class ToolBootstrapSystem : GameSystemBase
    {
        private const int kLogEvery = 120;       // ~2s @ 60 FPS
        private const double kSoftWarnAfter = 8; // seconds

        private bool m_Done;
        private int m_Ticks;
        private double m_StartElapsed; // World.Time.ElapsedTime at start

        protected override void OnCreate()
        {
            base.OnCreate();
            m_Done = false;
            m_Ticks = 0;
            // Store current DOTS elapsed time (seconds) as baseline
            m_StartElapsed = World.Time.ElapsedTime;
        }

        protected override void OnUpdate()
        {
            if (m_Done)
                return;

            // If our tool already holds a prefab, we’re done.
            ZoningControllerToolSystem? tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            if (tool != null && tool.GetPrefab() != null)
            {
                ARTZoneMod.s_Log.Info("[ART] Bootstrap: tool prefab present; nothing to do.");
                m_Done = true;
                Enabled = false;
                return;
            }

            // Ask the helper to try (quietly) — no error spam while anchor isn’t ready yet.
            ToolsHelper.InstantiateTools(logIfNoAnchor: false);

            // Check again after trying.
            if (tool != null && tool.GetPrefab() != null)
            {
                ARTZoneMod.s_Log.Info("[ART] Bootstrap: tools created.");
                m_Done = true;
                Enabled = false;
                return;
            }

            // Throttled, soft logging after we’ve waited a bit.
            m_Ticks++;
            var waited = World.Time.ElapsedTime - m_StartElapsed;
            if (waited > kSoftWarnAfter && (m_Ticks % kLogEvery) == 0)
            {
                ARTZoneMod.s_Log.Warn("[ART] Bootstrap: still waiting for Road Services to initialize …");
            }
        }
    }
}
