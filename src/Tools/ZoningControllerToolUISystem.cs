// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose bindings the UI reads/writes (ToolZoningMode, RoadZoningMode, IsRoadPrefab).
//  • Handle triggers (Change/Flip/Toggle).
//  • NEW: Instantiate our prefab/tile AFTER game load so Road Services anchors exist.
//
namespace AdvancedRoadTools.Tools
{
    using Colossal.Serialization.Entities;
    using Colossal.UI.Binding;
    using Game;
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

        // IMPORTANT: defer prefab/tile creation here — after RoadsServices has loaded.
        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            base.OnGameLoadingComplete(purpose, mode);

            AdvancedRoadToolsMod.s_Log.Info("[ART] UISystem OnGameLoadingComplete: instantiating tools");
            ToolsHelper.Initialize(force: false);
            ToolsHelper.InstantiateTools(logIfNoAnchor: true);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Optional keyboard: Shift+Z toggle (via Colossal action)
            var toggle = AdvancedRoadToolsMod.m_ToggleToolAction;
            if (toggle != null && toggle.WasPressedThisFrame())
            {
                ToggleTool();
                // keep panel state consistent when toggled on
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

            bool enable = m_MainToolSystem.activeTool != m_ToolSystem;
            m_ToolSystem.SetToolEnabled(enable);
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

        // Helpers used by tool & hotkeys
        public void InvertZoningMode()
        {
            var next = ToolZoningMode ^ ZoningMode.Both; // Both<->None
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
    }
}
