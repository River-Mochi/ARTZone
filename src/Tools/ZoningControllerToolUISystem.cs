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
        // === UI bindings ===
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!;

        // === Tool access ===
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ToolSystem = null!;

        // Public helpers (used by other systems)
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

        private void OnToolChanged(ToolBaseSystem tool)
        {
            try
            {
                m_IsRoadPrefab.Update(tool is not null && tool.GetPrefab() is RoadPrefab);
#if DEBUG
                Dbg($"OnToolChanged: activeTool={(tool != null ? tool.GetType().Name : "(null)")}  isRoad={(tool != null && tool.GetPrefab() is RoadPrefab)}");
#endif
            }
            catch { }
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            try
            {
                m_IsRoadPrefab.Update(prefab is RoadPrefab);
#if DEBUG
                Dbg($"OnPrefabChanged: prefab={(prefab != null ? prefab.name : "(null)")}  isRoad={(prefab is RoadPrefab)}");
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
#if DEBUG
                Dbg($"ToggleTool → enable={enable}");
#endif
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

        // === Public helpers for the Tool (RMB logic lives there) ===

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

    }
}
