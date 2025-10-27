// Tools/KeybindHotkeySystem.cs
// Purpose: Shift+Z (or the player's bound hotkey) toggles ARTZone.ZoningToolID on/off.
// Notes:   RMB preview flip is handled inside ZoningControllerToolSystem via cancelAction.
//          Release-safe - Debug-only helpers live in KeybindHotkeySystem.Debug.cs.

namespace ARTZone.Tools
{
    using Game;
    using Game.Input;
    using Game.Tools;

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        // Tool instance toggle
        private ZoningControllerToolSystem m_Tool = null!;

        // User-bindable hotkey action (default Shift+Z, can be rebound)
        private ProxyAction? m_Toggle;

#if DEBUG
        private static void Dbg(string message)
        {
            try
            {
                var log = ARTZoneMod.s_Log;
                if (log != null)
                {
                    log.Info("[ART][Hotkeys] " + message);
                }
            }
            catch
            {
                // swallow errors from logger early init
            }
        }
#else
        private static void Dbg(string message)
        {
        }
#endif

        /// <summary>
        /// Called by the engine once when the system is created.
        /// Sets up references and then calls DebugInit(), which only exists in DEBUG builds.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            m_Tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            m_Toggle = ARTZoneMod.ToggleToolAction;

            // In DEBUG builds, this becomes a real method that logs + inspects bindings.
            // In Release, DebugInit() is compiled out
            DebugInit();
        }

        /// <summary>
        /// Called every frame. If the toggle hotkey was pressed this frame,
        /// enable or disable our zoning tool.
        /// </summary>
        protected override void OnUpdate()
        {
            var toggle = m_Toggle;
            if (toggle == null || !toggle.WasPressedThisFrame())
            {
                return;
            }

            var toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

            // Decide whether to enable the tool:
            // true  = activate ARTZone.ZoningToolID
            // false = deactivate (return to vanilla tool)
            bool willEnable =
                toolSystem != null &&
                m_Tool != null &&
                toolSystem.activeTool != m_Tool;

#if DEBUG
            Dbg("Toggle pressed → willEnable=" + willEnable);
#endif

            // Guard: m_Tool can theoretically still be null if creation failed.
            if (m_Tool != null)
            {
                m_Tool.SetToolEnabled(willEnable);
            }
        }

        // DEBUG hook. In Release builds this method is removed at compile time
        // because partial void with no implementation disappears.
        partial void DebugInit();
    }
}
