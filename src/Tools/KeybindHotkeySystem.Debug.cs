// Tools/KeybindHotkeySystem.Debug.cs
// Purpose: Dev-only diagnostics for KeybindHotkeySystem.
//          This file is only compiled in DEBUG builds.
//          It adds DebugInit() and reflection-based inspection of InputManager.

#if DEBUG
namespace ARTZone.Tools
{
    using System.Collections;
    using System.Reflection;
    using Game.Input;

    public sealed partial class KeybindHotkeySystem
    {
        /// <summary>
        /// Runs in DEBUG builds after OnCreate().
        /// Logs that hotkeys were wired and dumps a sample of input actions.
        /// In Release builds this method does not exist.
        /// </summary>
        private partial void DebugInit()
        {
            Dbg("Created; hotkeys wired (DEBUG build).");
            DumpInputActions();
        }

        /// <summary>
        /// Reflection probe: print a handful of input action IDs from InputManager.
        /// Diagnostic only. Never mutates the input system.
        /// </summary>
        private static void DumpInputActions()
        {
            try
            {
                var imType = typeof(InputManager);

                // static InputManager.instance
                var instanceProp = imType.GetProperty(
                    "instance",
                    BindingFlags.Public | BindingFlags.Static);

                object? inputManager = instanceProp?.GetValue(null);
                if (inputManager == null)
                {
                    return;
                }

                // private Dictionary m_Actions on InputManager
                var actionsField = imType.GetField(
                    "m_Actions",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (actionsField == null)
                {
                    return;
                }

                var dict = actionsField.GetValue(inputManager) as IDictionary;
                if (dict == null)
                {
                    return;
                }

                int shown = 0;
                foreach (var key in dict.Keys)
                {
                    if (shown++ > 25)
                    {
                        break;
                    }

                    Dbg("Action id: " + key);
                }
            }
            catch
            {
                // swallow completely — debug only, never crash the game
            }
        }
    }
}
#endif
