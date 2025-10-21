// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  Renders the “Zoning Side” section in-game with three buttons. Reads values from ZoningControllerToolUISystem bindings
//  and calls its triggers when clicked.
//  Holds bindings for:
//    - ToolZoningMode (for standalone tool)
//    - RoadZoningMode (for future vanilla net placement use)
//    - IsRoadPrefab (UI decides where to render)
//  Exposes static helpers so hotkeys and other entry points can open the same mini panel.
//  Also exposes InvertZoningSideOnly(): flips Left<->Right without ever producing Both/None from RMB.

namespace AdvancedRoadTools.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        // === UI value bindings exposed to the web UI ===
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!;

        // === For checking active tool/prefab and toggling tool ===
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ToolSystem = null!;

        // Mini panel visibility (same behavior for GameTopLeft icon and Shift+Z)
        private static ZoningControllerToolUISystem? s_Instance;
        private bool m_PanelVisible;

        // Public helpers used by the tool system
        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

        // Convert "Both/Left/Right" to left/right depths (6 = on, 0 = off)
        public int2 ToolDepths
        {
            get => new(
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
            set
            {
                var newZoningMode = ZoningMode.Both;
                if (value.x == 0)
                    newZoningMode ^= ZoningMode.Left;
                if (value.y == 0)
                    newZoningMode ^= ZoningMode.Right;
                ChangeToolZoningMode((int)newZoningMode);
            }
        }

        public int2 RoadDepths
        {
            get => new(
                ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
            set
            {
                var newZoningMode = ZoningMode.Both;
                if (value.x == 0)
                    newZoningMode ^= ZoningMode.Left;
                if (value.y == 0)
                    newZoningMode ^= ZoningMode.Right;
                ChangeRoadZoningMode((int)newZoningMode);
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            // Bindings exposed to JS/TS (ids must match UI code)
            AddBinding(m_ToolZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "IsRoadPrefab", false));

            // Triggers callable from JS/TS
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "ToggleZoneControllerTool", ToggleTool));

            // Observe active tool/prefab to decide where to render the section in the UI
            m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_MainToolSystem.EventPrefabChanged += EventPrefabChanged;
            m_MainToolSystem.EventToolChanged += EventToolChanged;

            // For toggling our tool on demand from the floating button
            m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();

            s_Instance = this;
            m_PanelVisible = false;
        }

        protected override void OnDestroy()
        {
            if (s_Instance == this)
                s_Instance = null;
            base.OnDestroy();
        }

        private void EventToolChanged(ToolBaseSystem tool)
        {
            // Update IsRoadPrefab when the active tool changes
            m_IsRoadPrefab.Update(tool.GetPrefab() is RoadPrefab);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        private void EventPrefabChanged(PrefabBase prefab)
        {
            // Update IsRoadPrefab when the selected prefab changes
            m_IsRoadPrefab.Update(prefab is RoadPrefab);
        }

        private void ToggleTool()
        {
            // Hooked by floating button. Flip our tool on/off.
            m_ToolSystem.SetToolEnabled(m_MainToolSystem.activeTool != m_ToolSystem);
        }

        private void FlipToolBothMode()
        {
            if (ToolZoningMode == ZoningMode.Both)
                m_ToolZoningMode.Update((int)ZoningMode.None);
            else
                m_ToolZoningMode.Update((int)ZoningMode.Both);
        }

        private void FlipRoadBothMode()
        {
            if (RoadZoningMode == ZoningMode.Both)
                m_RoadZoningMode.Update((int)ZoningMode.None);
            else
                m_RoadZoningMode.Update((int)ZoningMode.Both);
        }

        private void ChangeToolZoningMode(int value)
        {
            m_ToolZoningMode.Update(value);
        }

        private void ChangeRoadZoningMode(int value)
        {
            m_RoadZoningMode.Update(value);
        }

        public void InvertZoningMode()
        {
            // Debug helper: flip both bits together (kept for compatibility with existing TS)
            var next = ToolZoningMode ^ ZoningMode.Both; // toggles both bits at once
            ChangeToolZoningMode((int)next);
        }

        /// <summary>
        /// Flip strictly Left <-> Right. If Both or None, pick Left first.
        /// Used by RMB when hovering an existing road.
        /// </summary>
        public void InvertZoningSideOnly()
        {
            var mode = ToolZoningMode;
            ZoningMode next;
            if (mode == ZoningMode.Left)
                next = ZoningMode.Right;
            else if (mode == ZoningMode.Right)
                next = ZoningMode.Left;
            else
                next = ZoningMode.Left; // Both or None -> Left
            ChangeToolZoningMode((int)next);
        }

        // ===== Mini panel visibility control (shared entry for hotkey + GameTopLeft) =====

        private void SetPanelVisible(bool visible)
        {
            if (m_PanelVisible == visible)
                return;
            m_PanelVisible = visible;
            // TS side listens to a binding or a trigger. If needed, add a ValueBinding<bool> later.
            // For now, rely on JS to query state via existing bindings on demand.
        }

        public static void ToggleMiniPanel()
        {
            var inst = s_Instance;
            if (inst == null)
                return;
            inst.SetPanelVisible(!inst.m_PanelVisible);
        }

        public static void EnsureMiniPanelVisible()
        {
            var inst = s_Instance;
            if (inst == null)
                return;
            inst.SetPanelVisible(true);
        }

        /// <summary>
        /// Hotkey bridge: previously toggled the tool; now opens the same mini panel as the GameTopLeft icon.
        /// </summary>
        public void ToggleFromHotkey()
        {
            ToggleMiniPanel();
        }
    }
}
