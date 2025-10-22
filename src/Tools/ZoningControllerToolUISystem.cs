// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  Renders the “Zoning Side” section in-game with three buttons. Exposes values/triggers to JS/TS.
//  Holds bindings for:
//    - ToolZoningMode (standalone tool)
//    - RoadZoningMode (future vanilla net placement)
//    - IsRoadPrefab (UI decides where to render)
//  Helpers for hotkeys and tool actions:
//    - InvertZoningMode(): toggles both bits (bitwise ^ Both) — used by Shift+Z
//    - FlipSmart(): Both↔None, otherwise Left↔Right — used by RMB when hovering a road
//    - InvertZoningSideOnly(): strict Left↔Right with fallback to Left

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

        // Public helpers used by the tool system
        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

        // Convert "Both/Left/Right" to left/right depths (6 = on, 0 = off)
        public int2 ToolDepths
        {
            get => new int2(
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
            get => new int2(
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

            // For toggling our tool on demand from the floating button / hotkey
            m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
        }

        protected override void OnDestroy()
        {
            if (m_MainToolSystem != null)
            {
                m_MainToolSystem.EventPrefabChanged -= EventPrefabChanged;
                m_MainToolSystem.EventToolChanged -= EventToolChanged;
            }
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
            // Called by the floating button; flip our tool on/off.
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
            // Bitwise flip both bits together (used by Shift+Z)
            var next = ToolZoningMode ^ ZoningMode.Both;
            ChangeToolZoningMode((int)next);
        }

        public void FlipSmart()
        {
            var current = ToolZoningMode;

            if (current == ZoningMode.Both)
            {
                ChangeToolZoningMode((int)ZoningMode.None);
                return;
            }
            if (current == ZoningMode.None)
            {
                ChangeToolZoningMode((int)ZoningMode.Both);
                return;
            }

            // Otherwise flip Left <-> Right
            if (current == ZoningMode.Left)
                ChangeToolZoningMode((int)ZoningMode.Right);
            else if (current == ZoningMode.Right)
                ChangeToolZoningMode((int)ZoningMode.Left);
            else
                ChangeToolZoningMode((int)ZoningMode.Both); // safety fallback
        }

        /// <summary>
        /// Flip strictly Left <-> Right. If Both or None, pick Left first.
        /// </summary>
        public void InvertZoningSideOnly()
        {
            var mode = ToolZoningMode;
            var next =
                (mode == ZoningMode.Left) ? ZoningMode.Right :
                (mode == ZoningMode.Right) ? ZoningMode.Left :
                ZoningMode.Left; // Both or None -> Left
            ChangeToolZoningMode((int)next);
        }

        /// <summary>
        /// Hotkey bridge (Shift+Z behavior): toggles the tool like the floating button.
        /// If your UI wants “open panel” instead, do it in JS — this just flips the tool on/off.
        /// </summary>
        public void ToggleFromHotkey()
        {
            ToggleTool();
        }
    }
}
