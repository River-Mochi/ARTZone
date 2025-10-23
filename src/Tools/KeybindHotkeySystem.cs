// File: src/Tools/KeybindHotkeySystem.cs
// Purpose: Shift+Z toggles the tool (same as GameTopLeft). Optional invert hotkey retained.

namespace AdvancedRoadTools.Tools
{
    using Game;
    using Game.Input;
    using Game.Tools;
    using Unity.Entities;

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        private ZoningControllerToolUISystem m_UI = null!;
        private ZoningControllerToolSystem m_Tool = null!;
        private ProxyAction? m_Toggle;
        private ProxyAction? m_Invert;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_UI = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_Tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            m_Toggle = AdvancedRoadToolsMod.m_ToggleToolAction;
            m_Invert = AdvancedRoadToolsMod.m_InvertZoningAction;
        }
        protected override void OnUpdate()
        {
            if (m_Toggle != null && m_Toggle.WasPressedThisFrame())
            {
                var toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
                bool willEnable = toolSystem.activeTool != m_Tool;

                if (willEnable)
                {
                    // Activate our prefab tool — this opens the RoadsServices palette and selects ART tile
                    m_Tool.SetToolEnabled(true);
                }
                else
                {
                    m_Tool.SetToolEnabled(false);
                }
            }

            if (m_Invert != null && m_Invert.WasPressedThisFrame())
            {
                m_UI.InvertZoningMode();
            }
        }

    }
}
