// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  Exposes “Zoning Side” values/bindings to the web UI, and offers triggers the TS calls.
//  Holds two bitmasks (ToolZoningMode / RoadZoningMode) and a bool (IsRoadPrefab).
//  Also reacts to the optional ToggleTool keybind (surfaced via ARTZoneMod.m_ToggleToolAction).

namespace ARTZone.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        // === Bindings visible to JS/TS ===
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!;

        // === Tool systems we interact with ===
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ToolSystem = null!;

        // Public helpers for other systems
        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

        // Convert bitmask to per-side depths (6 = on, 0 = off)
        public int2 ToolDepths
        {
            get => new(
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
            set
            {
                ZoningMode mode = ZoningMode.Both;
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
                ZoningMode mode = ZoningMode.Both;
                if (value.x == 0)
                    mode ^= ZoningMode.Left;
                if (value.y == 0)
                    mode ^= ZoningMode.Right;
                ChangeRoadZoningMode((int)mode);
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            // Value bindings (ids used by TS):
            AddBinding(m_ToolZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(ARTZoneMod.ModID, "IsRoadPrefab", false));

            // Triggers TS calls:
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "ToggleZoneControllerTool", ToggleTool));

            // Observe vanilla tool/prefab, so TS knows when to show the section
            m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
            m_MainToolSystem.EventToolChanged -= OnToolChanged;
            m_MainToolSystem.EventPrefabChanged += OnPrefabChanged;
            m_MainToolSystem.EventToolChanged += OnToolChanged;

            // Our tool instance
            m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
        }

        protected override void OnDestroy()
        {
            if (m_MainToolSystem != null)
            {
                m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
                m_MainToolSystem.EventToolChanged -= OnToolChanged;
            }
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Optional keyboard: Toggle tool (Shift+Z) — handled via Colossal action created by Setting.cs
            Game.Input.ProxyAction? toggle = ARTZoneMod.m_ToggleToolAction;
            if (toggle != null && toggle.WasPressedThisFrame())
            {
                ToggleTool();
                // Keep the orange-panel buttons in sync with the tool mode when toggled on
                m_ToolZoningMode.Update(m_RoadZoningMode.value);
            }
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            m_IsRoadPrefab.Update(tool.GetPrefab() is RoadPrefab);
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            m_IsRoadPrefab.Update(prefab is RoadPrefab);
        }

        private void ToggleTool()
        {
            if (m_MainToolSystem == null || m_ToolSystem == null)
                return;

            var wantEnable = m_MainToolSystem.activeTool != m_ToolSystem;
            m_ToolSystem.SetToolEnabled(wantEnable);
        }

        private void FlipToolBothMode()
        {
            m_ToolZoningMode.Update(ToolZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
        }

        private void FlipRoadBothMode()
        {
            m_RoadZoningMode.Update(RoadZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
        }

        private void ChangeToolZoningMode(int value) => m_ToolZoningMode.Update(value);
        private void ChangeRoadZoningMode(int value) => m_RoadZoningMode.Update(value);

        public void SetRoadZoningMode(ZoningMode mode) => m_RoadZoningMode.Update((int)mode);

        public void InvertZoningMode()
        {
            // Tool-side invert (bitwise)
            ChangeToolZoningMode((int)~ToolZoningMode);
        }
    }
}
