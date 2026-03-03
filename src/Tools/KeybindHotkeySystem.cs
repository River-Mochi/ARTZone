// File: src/Tools/KeybindHotkeySystem.cs
// Purpose: Ctrl+Z toggles EasyZoning.ZoningTool on/off.
// Notes:   RMB preview flip is handled inside ZoningControllerToolSystem via cancelAction.
//          Debug-only helpers live in KeybindHotkeySystem.Debug.cs.


namespace EasyZoning.Tools
{
    using Colossal.Logging;
    using Game;
    using Game.Input;
    using Game.Tools;

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        private ZoningControllerToolSystem m_Tool = null!;
        private ProxyAction? m_Toggle;

#if DEBUG
        private static void Dbg(string message)
        {
            try
            {
                ILog log = Mod.s_Log;
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

            m_Tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            m_Toggle = Mod.ToggleToolAction;

            DebugInit(); // becomes a no-op in Release
        }

        protected override void OnUpdate( )
        {
            if (m_Toggle == null)
                m_Toggle = Mod.ToggleToolAction;

            ProxyAction? toggle = m_Toggle;
            if (toggle == null || !toggle.WasPressedThisFrame())
                return;

            ToolSystem toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            bool willEnable =
                toolSystem != null &&
                m_Tool != null &&
                toolSystem.activeTool != m_Tool;

#if DEBUG
            Dbg("Toggle pressed → willEnable=" + willEnable);
#endif

            if (m_Tool != null)
                m_Tool.SetToolEnabled(willEnable);
        }

        // DEBUG hook. Partial method erased in Release.
        partial void DebugInit( );
    }
}
