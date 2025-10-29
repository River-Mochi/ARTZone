// File: src/Tools/KeybindHotkeySystem.Debug.cs
// Purpose: Dev-only diagnostics for KeybindHotkeySystem.
//          Adds DebugInit() and reflection-based inspection of InputManager.


#if DEBUG
namespace EasyZoning.Tools
{
    using System.Collections;
    using System.Reflection;
    using Game.Input;

    public sealed partial class KeybindHotkeySystem
    {
        partial void DebugInit()
        {
            Dbg("Created; hotkeys wired (DEBUG build).");
            DumpInputActions();
        }

        private static void DumpInputActions()
        {
            try
            {
                var imType = typeof(InputManager);

                var instanceProp = imType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                object? inputManager = instanceProp?.GetValue(null);
                if (inputManager == null)
                    return;

                var actionsField = imType.GetField("m_Actions", BindingFlags.NonPublic | BindingFlags.Instance);
                if (actionsField == null)
                    return;

                var dict = actionsField.GetValue(inputManager) as IDictionary;
                if (dict == null)
                    return;

                int shown = 0;
                foreach (var key in dict.Keys)
                {
                    if (shown++ > 25)
                        break;
                    Dbg("Action id: " + key);
                }
            }
            catch
            {
                // debug only
            }
        }
    }
}
#endif
