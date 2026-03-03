// File: src/Tools/KeybindHotkeySystem.Debug.cs
// Purpose: Dev-only diagnostics for KeybindHotkeySystem.
//          Adds DebugInit() and reflection-based inspection of InputManager.


#if DEBUG
namespace EasyZoning.Tools
{
    using Game.Input;
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed partial class KeybindHotkeySystem
    {
        partial void DebugInit( )
        {
            Dbg("Created; hotkeys wired (DEBUG build).");


            if (m_Toggle == null)
            {
                Dbg("Toggle action is null; dumping InputManager actions for debugging.");
                DumpInputActions();
            }

        }

        private static void DumpInputActions( )
        {
            try
            {
                Type imType = typeof(InputManager);

                PropertyInfo instanceProp = imType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                object? inputManager = instanceProp?.GetValue(null);
                if (inputManager == null)
                    return;

                FieldInfo actionsField = imType.GetField("m_Actions", BindingFlags.NonPublic | BindingFlags.Instance);
                if (actionsField == null)
                    return;

                if (actionsField.GetValue(inputManager) is not IDictionary dict)
                    return;

                int shown = 0;
                foreach (object? key in dict.Keys)
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
