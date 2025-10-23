// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose bindings the UI reads/writes (ToolZoningMode, RoadZoningMode, IsRoadPrefab).
//  • Handle triggers (Change/Flip/Toggle).
//  • Robust NRE guards and DEBUG logs in all event handlers.
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
                ChangeToolZoningMode((int)mode);
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
        private static void dbg(string msg)
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
        private static void dbg(string msg) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            // Bindings (IDs must match TS)
            AddBinding(m_ToolZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "IsRoadPrefab", false));

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
            catch { /* be defensive; UI still functional without these */ }

            // Our tool instance
            try
            {
                m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            }
            catch { /* guard in ToggleTool() anyway */ }

#if DEBUG
            dbg("UISystem created and bindings registered.");
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
            catch { /* ignore */ }
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Optional keyboard: Shift+Z toggle (via Colossal action)
            var toggle = AdvancedRoadToolsMod.m_ToggleToolAction;
            try
            {
                if (toggle != null && toggle.WasPressedThisFrame())
                {
                    ToggleTool();
                    // keep panel state consistent when toggled on
                    m_ToolZoningMode.Update(m_RoadZoningMode.value);
                }
            }
            catch { /* ignore input hiccups */ }
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            try
            {
                m_IsRoadPrefab.Update(tool is not null && tool.GetPrefab() is RoadPrefab);
#if DEBUG
                dbg($"OnToolChanged: activeTool={(tool != null ? tool.GetType().Name : "(null)")}  isRoad={(tool != null && tool.GetPrefab() is RoadPrefab)}");
#endif
            }
            catch { /* guard against transient nulls */ }
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            try
            {
                m_IsRoadPrefab.Update(prefab is RoadPrefab);
#if DEBUG
                dbg($"OnPrefabChanged: prefab={(prefab != null ? prefab.name : "(null)")}  isRoad={(prefab is RoadPrefab)}");
#endif
            }
            catch { /* guard */ }
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
                dbg($"ToggleTool → enable={enable}");
#endif
            }
            catch { /* guard */ }
        }

        private void FlipToolBothMode()
        {
            try
            {
                m_ToolZoningMode.Update(ToolZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
#if DEBUG
                dbg($"FlipToolBothMode → {(ZoningMode)m_ToolZoningMode.value}");
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
                dbg($"FlipRoadBothMode → {(ZoningMode)m_RoadZoningMode.value}");
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

        // Helpers used by tool & hotkeys
        public void InvertZoningMode()
        {
            try
            {
                var next = ToolZoningMode ^ ZoningMode.Both; // Both<->None
                ChangeToolZoningMode((int)next);
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
                ChangeToolZoningMode((int)next);
            }
            catch { }
        }
    }
}
