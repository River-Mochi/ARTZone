// Tools/ZoningControllerToolUISystem.cs

namespace AdvancedRoadTools.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!;

        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

        public int2 ToolDepths
        {
            get
            {
                int left = ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0;
                int right = ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0;
                return new int2(left, right);
            }
            set
            {
                ZoningMode newZoningMode = ZoningMode.Both;
                if (value.x == 0)
                    newZoningMode ^= ZoningMode.Left;
                if (value.y == 0)
                    newZoningMode ^= ZoningMode.Right;
                ChangeToolZoningMode((int)newZoningMode);
            }
        }

        public int2 RoadDepths
        {
            get
            {
                int left = ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0;
                int right = ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0;
                return new int2(left, right);
            }
            set
            {
                ZoningMode newZoningMode = ZoningMode.Both;
                if (value.x == 0)
                    newZoningMode ^= ZoningMode.Left;
                if (value.y == 0)
                    newZoningMode ^= ZoningMode.Right;
                ChangeRoadZoningMode((int)newZoningMode);
            }
        }

        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ToolSystem = null!;

        protected override void OnCreate()
        {
            base.OnCreate();

            AddBinding(m_ToolZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "IsRoadPrefab", false));

            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "ToggleZoneControllerTool", ToggleTool));

            m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_MainToolSystem.EventPrefabChanged += EventPrefabChanged;
            m_MainToolSystem.EventToolChanged += EventToolChanged;

            m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
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
            // Flip only Left/Right bits, keep within Both mask
            ZoningMode flipped = ZoningMode.Both ^ ToolZoningMode;
            ChangeToolZoningMode((int)flipped);
        }
    }
}
