// <copyright file="KeybindHotkeySystem.Debug.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: src/Tools/KeybindHotkeySystem.Debug.cs
// Purpose: Dev-only diagnostics for KeybindHotkeySystem.
//          Adds DebugInit() and reflection-based inspection of InputManager.

#if DEBUG
namespace EasyZoning.Tools
{
    using Game.Input;           // InputManager
    using System;               // Type
    using System.Collections;   // IDictionary
    using System.Reflection;    // BindingsFlags, PropertyInfo, FieldInfo

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
