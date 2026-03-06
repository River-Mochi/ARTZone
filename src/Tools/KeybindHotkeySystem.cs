// File: src/Tools/KeybindHotkeySystem.cs
// Purpose: Default hotkey toggles EasyZoning.ZoningTool on/off.
// Notes:
//   - actual keybind (Shift+V, etc.) is defined where Mod.ToggleToolAction is created.
//   - This system only listens for the action and applies the toggle.
//   - RMB cycling is handled inside ZoningControllerToolSystem.
//   - Debug-only helpers live in KeybindHotkeySystem.Debug.cs.

namespace EasyZoning.Tools
{
    using Game;         // GameSystemBase
    using Game.Input;   // ProxyAction
    using Game.Tools;   // ToolSystem

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        // Cached EZ tool instance (used for SetToolEnabled calls).
        private ZoningControllerToolSystem m_Tool = null!;

        // Input action created elsewhere (typically Mod.cs).
        private ProxyAction? m_Toggle;

#if DEBUG
        private static void Dbg(string message)
        {
            try
            {
                Colossal.Logging.ILog log = Mod.s_Log;
                if (log != null)
                    log.Info("[EZ][Hotkeys] " + message);
            }
            catch { }
        }
#else
        private static void Dbg(string message)
        {
        }
#endif

        protected override void OnCreate( )
        {
            base.OnCreate();

            // Tool instance used for toggling.
            m_Tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();

            // ProxyAction reference (may be null early; refresh in OnUpdate).
            m_Toggle = Mod.ToggleToolAction;

            // Debug wiring (partial method erased in Release).
            DebugInit();
        }

        protected override void OnUpdate( )
        {
            // Action can be assigned after OnCreate; refresh if needed.
            if (m_Toggle == null)
                m_Toggle = Mod.ToggleToolAction;

            // Only act on an actual press edge.
            ProxyAction? toggle = m_Toggle;
            if (toggle == null || !toggle.WasPressedThisFrame())
                return;

            // Determine whether current press should enable or disable.
            ToolSystem toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            bool willEnable =
                toolSystem != null &&
                m_Tool != null &&
                toolSystem.activeTool != m_Tool;

#if DEBUG
            Dbg("Toggle pressed → willEnable=" + willEnable);
#endif

            // Apply toggle.
            if (m_Tool != null)
                m_Tool.SetToolEnabled(willEnable);
        }

        // DEBUG hook. Partial method erased in Release.
        partial void DebugInit( );
    }
}
