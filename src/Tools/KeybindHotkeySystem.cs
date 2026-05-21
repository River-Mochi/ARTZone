// File: src/Tools/KeybindHotkeySystem.cs
// Purpose: Default hotkey toggles EasyZoning.ZoningTool on/off.
// Notes:
//   - actual keybind (Ctrl+V, etc.) is defined in Setting.cs.
//   - this system gets/enables the ProxyAction because this is where it is used.
//   - release-edge handling acts like a normal click and avoids held-key repeats.
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

        // Input action registered by Setting.RegisterKeyBindings().
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

            // Cache the action where it is used, matching the CS2 keybind guidance.
            m_Toggle = GetToggleAction();

            // Debug wiring (partial method erased in Release).
            DebugInit();
        }

        protected override void OnUpdate( )
        {

            // Action can be assigned after OnCreate; refresh if needed.
            if (m_Toggle == null)
                m_Toggle = GetToggleAction();

            // Treat the hotkey like a click: toggle once when the player releases it.
            ProxyAction? toggle = m_Toggle;
            if (toggle == null || !toggle.WasReleasedThisFrame())
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

        private static ProxyAction? GetToggleAction( )
        {
            Setting? settings = Mod.Settings;
            if (settings == null)
                return null;

            ProxyAction? action = settings.GetAction(Mod.kToggleToolActionName);
            if (action != null)
                action.shouldBeEnabled = true;

            return action;
        }

        // DEBUG hook. Partial method erased in Release.
        partial void DebugInit( );
    }
}
