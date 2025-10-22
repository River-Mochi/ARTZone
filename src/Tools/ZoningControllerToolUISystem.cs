// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Exposes bindings the UI reads/writes (ToolZoningMode, RoadZoningMode, IsRoadPrefab, ShowMiniPanel).
//  • Hotkey + GameTopLeft button toggle the same mini panel via ShowMiniPanel.
//  • Helpers for RMB flip (Left<->Right), Both/None flip, and tool toggle.

namespace AdvancedRoadTools.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        // === Value bindings exposed to the web UI ===
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!;
        private ValueBinding<bool> m_ShowMiniPanel = null!; // NEW: drives the small panel visibility

        // === For checking active tool/prefab and toggling tool ===
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ToolSystem = null!;

        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

        // Convert mode to depths (6 = on, 0 = off)
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

            // Value bindings (ids must match UI code)
            AddBinding(m_ToolZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "IsRoadPrefab", false));
            AddBinding(m_ShowMiniPanel = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "ShowMiniPanel", false)); // NEW

            // Triggers callable from JS/TS
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "ToggleZoneControllerTool", ToggleTool));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "ToggleMiniPanel", ToggleFromHotkey));



            // Observe active tool/prefab to decide where to render the section under vanilla tool
            m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_MainToolSystem.EventPrefabChanged += EventPrefabChanged;
            m_MainToolSystem.EventToolChanged += EventToolChanged;

            m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
        }

        protected override void OnDestroy()
        {
            m_MainToolSystem.EventPrefabChanged -= EventPrefabChanged;
            m_MainToolSystem.EventToolChanged -= EventToolChanged;
            base.OnDestroy();
        }

        private void EventToolChanged(ToolBaseSystem tool)
        {
            m_IsRoadPrefab.Update(tool.GetPrefab() is RoadPrefab);
        }

        private void EventPrefabChanged(PrefabBase prefab)
        {
            m_IsRoadPrefab.Update(prefab is RoadPrefab);
        }

        private void ToggleTool()
        {
            // Floating button legacy behavior (kept): flip our tool on/off.
            m_ToolSystem.SetToolEnabled(m_MainToolSystem.activeTool != m_ToolSystem);
        }

        private void ToggleMiniPanel()
        {
            // Single source of truth for UI panel visibility
            m_ShowMiniPanel.Update(!m_ShowMiniPanel.value);
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

        private void ChangeToolZoningMode(int value) => m_ToolZoningMode.Update(value);
        private void ChangeRoadZoningMode(int value) => m_RoadZoningMode.Update(value);

        // == Helpers used from tool & hotkeys ==

        public void InvertZoningMode()
        {
            // classic invert both bits (Both<->None)
            var next = ToolZoningMode ^ ZoningMode.Both;
            ChangeToolZoningMode((int)next);
        }

        /// <summary>Flip strictly Left &lt;-&gt; Right. If Both/None, pick Left first.</summary>
        public void InvertZoningSideOnly()
        {
            var mode = ToolZoningMode;
            ZoningMode next =
                mode == ZoningMode.Left ? ZoningMode.Right :
                mode == ZoningMode.Right ? ZoningMode.Left :
                ZoningMode.Left;
            ChangeToolZoningMode((int)next);
        }

        /// <summary>Hotkey bridge: open/close the same mini panel as the GameTopLeft icon.</summary>
        public void ToggleFromHotkey()
        {
            ToggleMiniPanel();
        }
    }
}
