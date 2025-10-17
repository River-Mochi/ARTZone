﻿// File: src/AdvancedRoadTools/Tools/ZoningControllerToolUISystem.cs
// Purpose: Expose UI bindings for Zoning Side buttons and toggle the tool via the floating button.

using Colossal.UI.Binding;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using Unity.Mathematics;

namespace AdvancedRoadTools.Tools
{
    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        private ValueBinding<int> toolZoningMode;
        private ValueBinding<int> roadZoningMode;

        private ValueBinding<bool> isRoadPrefab;

        public ZoningMode ToolZoningMode => (ZoningMode)toolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)roadZoningMode.value;

        public int2 ToolDepths
        {
            get => new(((ZoningMode)toolZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                       ((ZoningMode)toolZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
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
            get => new(((ZoningMode)roadZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                       ((ZoningMode)roadZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
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

        private ToolSystem mainToolSystem;
        private ZoningControllerToolSystem toolSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            AddBinding(toolZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(roadZoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(isRoadPrefab = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "IsRoadPrefab", false));

            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "ToggleZoneControllerTool", ToggleTool));

            mainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            mainToolSystem.EventPrefabChanged += EventPrefabChanged;
            mainToolSystem.EventToolChanged += EventToolChanged;

            toolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
        }

        private void EventToolChanged(ToolBaseSystem obj)
        {
            isRoadPrefab.Update(obj.GetPrefab() is RoadPrefab);
        }

        private void EventPrefabChanged(PrefabBase obj)
        {
            isRoadPrefab.Update(obj is RoadPrefab);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        private void ToggleTool()
        {
            toolSystem.SetToolEnabled(mainToolSystem.activeTool != toolSystem);
        }

        private void FlipToolBothMode()
        {
            toolZoningMode.Update(ToolZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
        }

        private void FlipRoadBothMode()
        {
            roadZoningMode.Update(RoadZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
        }

        private void ChangeToolZoningMode(int value) => toolZoningMode.Update(value);
        private void ChangeRoadZoningMode(int value) => roadZoningMode.Update(value);

        public void InvertZoningMode() => ChangeToolZoningMode((int)~ToolZoningMode);
    }
}
