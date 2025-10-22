// File: src/Tools/KeybindHotkeySystem.cs
// Purpose: Poll CO ProxyActions and trigger UI/tool actions.
// Behavior:
//   • Toggle action (Shift+Z) -> open/close the same mini panel as the GameTopLeft icon
//   • Invert action         -> flip zoning mode (both bits) – RMB handles side-only flip

namespace AdvancedRoadTools.Tools
{
    using Game;
    using Game.Input;
    using Unity.Entities;

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        private ZoningControllerToolUISystem m_UI = null!;
        private ProxyAction? m_Toggle;
        private ProxyAction? m_Invert;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_UI = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_Toggle = AdvancedRoadToolsMod.m_ToggleToolAction;
            m_Invert = AdvancedRoadToolsMod.m_InvertZoningAction;
        }

        protected override void OnUpdate()
        {
            if (m_Toggle != null && m_Toggle.WasPressedThisFrame())
            {
                // Same as clicking GameTopLeft icon per current UX: open mini panel
                m_UI.ToggleFromHotkey();
            }

            if (m_Invert != null && m_Invert.WasPressedThisFrame())
            {
                // Keep "invert both bits" on the keybind; RMB handles side-only
                m_UI.InvertZoningMode();
            }
        }
    }
}
