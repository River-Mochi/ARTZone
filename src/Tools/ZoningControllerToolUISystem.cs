// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose bindings the UI reads/writes (ToolZoningMode, RoadZoningMode, IsRoadPrefab).
//  • Handle triggers (Change/Flip/Toggle).
//  • NRE guards and DEBUG logs in all event handlers.
//  • Public helpers SetToolZoningMode(...), FlipToolBothOrNone(), InvertZoningSideOnly().

namespace ARTZone.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        // ---- Bindings (UI state) ---------------------------------------------
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!;

        // ---- Tool access ------------------------------------------------------
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ToolSystem = null!;

        // ---- Public helpers (used by other systems) ---------------------------
        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

        // Convert mode<->depths (6 = on, 0 = off)
        public int2 ToolDepths
        {
            get => new(
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
            set
            {
                var mode = ZoningMode.Both;
                if (value.x == 0)
                    mode ^= ZoningMode.Left;
                if (value.y == 0)
                    mode ^= ZoningMode.Right;
                SetToolZoningMode(mode);
            }
        }

        public int2 RoadDepths
        {
            get => new(
                ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
            set
            {
                var mode = ZoningMode.Both;
                if (value.x == 0)
                    mode ^= ZoningMode.Left;
                if (value.y == 0)
                    mode ^= ZoningMode.Right;
                ChangeRoadZoningMode((int)mode);
            }
        }

#if DEBUG
        private static void Dbg(string msg)
        {
            var log = ARTZoneMod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info("[ART][UI] " + msg);
            }
            catch { }
        }
#else
        private static void Dbg(string msg) { }
#endif

        // ---- Lifecycle --------------------------------------------------------
        protected override void OnCreate()
        {
            base.OnCreate();

            // Bindings (IDs must match TS)
            AddBinding(m_ToolZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(ARTZoneMod.ModID, "IsRoadPrefab", false));

            // Triggers (from TS)
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "ToggleZoneControllerTool", ToggleTool));

            // Observe vanilla tool/prefab to decide when to show the UI section
            try
            {
                m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
                if (m_MainToolSystem != null)
                {
                    m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged -= OnToolChanged;
                    m_MainToolSystem.EventPrefabChanged += OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged += OnToolChanged;

                    // Seed initial visibility based on current context.
                    UpdateIsRoadPrefabFromContext();
                }
            }
            catch { /* defensive */ }

            // Our tool instance
            try
            {
                m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            }
            catch { /* guard in ToggleTool() anyway */ }

#if DEBUG
            Dbg("UISystem created and bindings registered.");
#endif
        }

        protected override void OnDestroy()
        {
            try
            {
                if (m_MainToolSystem != null)
                {
                    m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged -= OnToolChanged;
                }
            }
            catch { }
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            // UI bindings only; hotkey toggle is handled elsewhere.
            base.OnUpdate();
        }

        // ---- Event handlers ---------------------------------------------------
        private void OnToolChanged(ToolBaseSystem tool)
        {
            try
            {
                // Show section whenever our tool is active OR the prefab is a road.
                var prefab = tool != null ? tool.GetPrefab() : null;
                var shouldShow = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(shouldShow);
#if DEBUG
                Dbg($"OnToolChanged: activeTool={(tool != null ? tool.GetType().Name : "(null)")}, show={shouldShow}");
#endif
            }
            catch { }
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            try
            {
                // Show section whenever our tool is active OR the prefab is a road.
                var tool = m_MainToolSystem != null ? m_MainToolSystem.activeTool : null;
                var shouldShow = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(shouldShow);
#if DEBUG
                Dbg($"OnPrefabChanged: prefab={(prefab != null ? prefab.name : "(null)")}, show={shouldShow}");
#endif
            }
            catch { }
        }

        private void ToggleTool()
        {
            try
            {
                if (m_MainToolSystem == null || m_ToolSystem == null)
                    return;

                bool enable = m_MainToolSystem.activeTool != m_ToolSystem;
                m_ToolSystem.SetToolEnabled(enable);

                // Immediately reflect intended visibility so users see the section
                // even before the next vanilla tool event comes in.
                if (enable)
                {
                    m_IsRoadPrefab.Update(true);
#if DEBUG
                    Dbg("ToggleTool → enable=true, forcing section visible");
#endif
                }
                else
                {
                    // When disabling, fall back to current prefab context.
                    UpdateIsRoadPrefabFromContext();
#if DEBUG
                    Dbg("ToggleTool → enable=false, recomputed section visibility");
#endif
                }
            }
            catch { }
        }

        private void FlipToolBothMode()
        {
            try
            {
                m_ToolZoningMode.Update(ToolZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
#if DEBUG
                Dbg($"FlipToolBothMode → {(ZoningMode)m_ToolZoningMode.value}");
#endif
            }
            catch { }
        }

        private void FlipRoadBothMode()
        {
            try
            {
                m_RoadZoningMode.Update(RoadZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
#if DEBUG
                Dbg($"FlipRoadBothMode → {(ZoningMode)m_RoadZoningMode.value}");
#endif
            }
            catch { }
        }

        private void ChangeToolZoningMode(int value)
        {
            try
            {
                m_ToolZoningMode.Update(value);
            }
            catch { }
        }

        private void ChangeRoadZoningMode(int value)
        {
            try
            {
                m_RoadZoningMode.Update(value);
            }
            catch { }
        }

        // ---- Public helpers for the Tool (RMB logic lives there) --------------
        public void SetToolZoningMode(ZoningMode mode)
        {
            try
            {
                m_ToolZoningMode.Update((int)mode);
            }
            catch { }
        }

        public void FlipToolBothOrNone()
        {
            try
            {
                var next = ToolZoningMode == ZoningMode.Both ? ZoningMode.None :
                           ToolZoningMode == ZoningMode.None ? ZoningMode.Both : ToolZoningMode;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"FlipToolBothOrNone → {next}");
#endif
            }
            catch { }
        }

        /// <summary>
        /// Toggle Left <-> Right. If called while mode is Both/None, set Left (seed the side-only toggle).
        /// </summary>
        public void InvertZoningSideOnly()
        {
            try
            {
                var mode = ToolZoningMode;
                ZoningMode next =
                    mode == ZoningMode.Left ? ZoningMode.Right :
                    mode == ZoningMode.Right ? ZoningMode.Left :
                    ZoningMode.Left;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"InvertZoningSideOnly → {next}");
#endif
            }
            catch { }
        }

        /// <summary>RMB preview toggle: Left<->Right if on a side; otherwise Both<->None.</summary>
        public void RmbPreviewToggle()
        {
            try
            {
                if (ToolZoningMode == ZoningMode.Left || ToolZoningMode == ZoningMode.Right)
                    InvertZoningSideOnly();      // strict Left↔Right
                else
                    FlipToolBothOrNone();        // strict Both↔None
            }
            catch { }
        }

        // ---- HELPERS ----------------------------------------------------------
        private void UpdateIsRoadPrefabFromContext()
        {
            try
            {
                var tool = m_MainToolSystem != null ? m_MainToolSystem.activeTool : null;
                var prefab = tool != null ? tool.GetPrefab() : null;
                var shouldShow = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(shouldShow);
#if DEBUG
                Dbg($"UpdateIsRoadPrefabFromContext → show={shouldShow}");
#endif
            }
            catch { }
        }

        // Show the section if our tool is active OR the prefab is a road.
        private static bool ShouldShowFor(ToolBaseSystem tool, PrefabBase prefab)
        {
            if (tool is ZoningControllerToolSystem)
                return true;

            if (prefab is RoadPrefab)
                return true;

            return false;
        }
    }
}
