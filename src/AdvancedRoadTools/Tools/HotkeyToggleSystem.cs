// File: src/AdvancedRoadTools/Tools/HotkeyToggleSystem.cs
// Purpose: Poll the CO ProxyAction each frame (no Unity input) and toggle our tool.

namespace AdvancedRoadTools.Tools
{
    using Game;
    using Game.Input;
    using Game.Tools;
    using Unity.Entities;

    public sealed partial class HotkeyToggleSystem : GameSystemBase
    {
        private ToolSystem _toolSystem = null!;
        private DefaultToolSystem _defaultTool = null!;
        private ZoningControllerToolSystem _zoneTool = null!;
        private ProxyAction? _toggle;

        protected override void OnCreate()
        {
            base.OnCreate();
            _toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            _defaultTool = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            _zoneTool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            _toggle = AdvancedRoadToolsMod.m_ToggleToolAction;
        }

        protected override void OnUpdate()
        {
            // action is registered in Mod.OnLoad; just poll WasPerformedThisFrame()
            if (_toggle != null && _toggle.WasPerformedThisFrame())
            {
                // Toggle between our tool and vanilla default tool
                _toolSystem.activeTool = (_toolSystem.activeTool == _zoneTool) ? _defaultTool : _zoneTool;
            }
        }
    }
}
