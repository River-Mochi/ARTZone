// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  Renders the “Zoning Side” section in-game with three buttons. Reads values from ZoningControllerToolUISystem bindings
//   and calls its triggers when clicked.
//   This is the UI system that exposes bindings to the JS/TS UI and holds two values:
//   - ToolZoningMode (for our standalone tool)
//   - RoadZoningMode (for future use while placing roads; original mod shows the same buttons)
//   And a flag:
//   - IsRoadPrefab (so the UI knows whether to render the section under the vanilla net tool or our tool)
//
//   The JS/TS (ZoningToolSections.tsx) reads these bindings and renders three buttons:
//     Both / Left / Right  (with icons)
//   It also calls the triggers to change them or flip "both".
//  ToolSystem reads ToolDepths (from UISystem) each frame, previews on hover,
//  and applies changes on LMB release. It also listens for the invert key and asks the UISystem to flip the mode.
//
//   NOTE: this does not deal with palette icon which is in ToolsHelper.cs via UIObject.
//
// Nullability:
//   using `= null!;` for fields that are set in OnCreate() to satisfy <Nullable>enable</Nullable>.

#nullable enable

using Colossal.UI.Binding;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using Unity.Mathematics;

namespace ARTZone.Tools
{
    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        // === UI value bindings exposed to the web UI ===
        private ValueBinding<int> toolZoningMode = null!;
        private ValueBinding<int> roadZoningMode = null!;
        private ValueBinding<bool> isRoadPrefab = null!;

        // === For checking active tool/prefab and toggling tool ===
        private ToolSystem mainToolSystem = null!;
        private ZoningControllerToolSystem toolSystem = null!;


        // Public helpers used by the tool system
        public ZoningMode ToolZoningMode => (ZoningMode)toolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)roadZoningMode.value;

        // Convert "Both/Left/Right" to left/right depths (6 = on, 0 = off)
        public int2 ToolDepths
        {
            get => new(
                ((ZoningMode)toolZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
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
            get => new(
                ((ZoningMode)roadZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
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

        protected override void OnCreate()
        {
            base.OnCreate();

            // Bindings exposed to JS/TS (ids must match your UI code)
            AddBinding(toolZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(roadZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(isRoadPrefab = new ValueBinding<bool>(ARTZoneMod.ModID, "IsRoadPrefab", false));

            // Triggers callable from JS/TS
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "ToggleZoneControllerTool", ToggleTool));

            // Observe active tool/prefab to decide where to render the section in the UI
            mainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            mainToolSystem.EventPrefabChanged += EventPrefabChanged;
            mainToolSystem.EventToolChanged += EventToolChanged;

            // For toggling our tool on demand from the floating button
            toolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();


        }

        private void EventToolChanged(ToolBaseSystem tool)
        {
            // Update IsRoadPrefab when the active tool changes
            isRoadPrefab.Update(tool.GetPrefab() is RoadPrefab);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

        }


        private void EventPrefabChanged(PrefabBase prefab)
        {
            // Update IsRoadPrefab when the selected prefab changes
            isRoadPrefab.Update(prefab is RoadPrefab);
        }

        private void ToggleTool()
        {
            // This is hooked by the floating button. Flip our tool on/off.
            toolSystem.SetToolEnabled(mainToolSystem.activeTool != toolSystem);
        }

        private void FlipToolBothMode()
        {
            if (ToolZoningMode == ZoningMode.Both)
                toolZoningMode.Update((int)ZoningMode.None);
            else
                toolZoningMode.Update((int)ZoningMode.Both);
        }

        private void FlipRoadBothMode()
        {
            if (RoadZoningMode == ZoningMode.Both)
                roadZoningMode.Update((int)ZoningMode.None);
            else
                roadZoningMode.Update((int)ZoningMode.Both);
        }

        private void ChangeToolZoningMode(int value)
        {
            // (ZoningMode) cast kept for readability in debug, but we only store the int
            toolZoningMode.Update(value);
        }

        private void ChangeRoadZoningMode(int value)
        {
            roadZoningMode.Update(value);
        }

        public void SetRoadZoningMode(ZoningMode mode) => roadZoningMode.Update((int)mode);


        public void InvertZoningMode()
        {
            // Keybind toggles tool-side bitmask
            ChangeToolZoningMode((int)~ToolZoningMode);
        }
    }
}
