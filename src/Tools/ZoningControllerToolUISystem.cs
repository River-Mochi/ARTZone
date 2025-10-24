// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose bindings the UI reads/writes (ToolZoningMode, RoadZoningMode, IsRoadPrefab).
//  • Handle triggers (Change/Flip/Toggle).
//  • Robust NRE guards and DEBUG logs in all event handlers.
//  • NEW: public helpers SetToolZoningMode(...) and FlipToolBothOrNone() so RMB logic
//         can live entirely inside the tool system.
//
// Note: The palette tile / prefab creation is handled by ARTPaletteBootstrapSystem + ToolsHelper
// after a real gameplay load completes. This UISystem just binds and reacts to tool changes.

namespace AdvancedRoadTools.Systems
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

        // NEW: expose icon path to TS/JS (so top-left button uses the same icon)
        private ValueBinding<string> m_MainIconPath = null!;

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
            var log = AdvancedRoadToolsMod.s_Log;
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
            AddBinding(m_ToolZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "IsRoadPrefab", false));
            // Expose the single source-of-truth icon path to the UI
            AddBinding(m_MainIconPath = new ValueBinding<string>(
                AdvancedRoadToolsMod.ModID, "MainIconPath", AdvancedRoadToolsMod.PaletteIconPath));


            // Triggers (from TS)
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "ToggleZoneControllerTool", ToggleTool));




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
            catch { /* defensive: UI still functional without these */ }

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
            // UI bindings only; hotkey toggle is handled in KeybindHotkeySystem.
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

        /// <summary>Set exact ToolZoningMode (updates the bound value & UI).</summary>
        public void SetToolZoningMode(ZoningMode mode)
        {
            try
            {
                m_ToolZoningMode.Update((int)mode);
            }
            catch { }
        }

        /// <summary>Toggle strictly Both &lt;-&gt; None.</summary>
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

        /// <summary>Flip strictly Left &lt;-&gt; Right. If Both/None, pick Left first.</summary>
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
    }
}
