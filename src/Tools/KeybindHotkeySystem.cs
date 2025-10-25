// Purpose: Shift+Z toggles the tool (same as the top-left button).
// Notes:   RMB is handled by ToolBaseSystem.cancelAction in the tool system (not here).

namespace ARTZone.Tools
{
    using Game;
    using Game.Input;
    using Game.Tools;

#if DEBUG
    using System.Collections;
    using System.Reflection;
#endif

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        private ZoningControllerToolSystem m_Tool = null!;
        private ProxyAction? m_Toggle; // Shift+Z (or user binding) to toggle

#if DEBUG
        private static void Dbg(string m)
        {
            try
            {
                var log = ARTZoneMod.s_Log;
                if (log != null)
                    log.Info("[ART][Hotkeys] " + m);
            }
            catch { }
        }
#else
        private static void Dbg(string m) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            m_Tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            m_Toggle = ARTZoneMod.m_ToggleToolAction;

#if DEBUG
            Dbg("Created; hotkeys wired.");

            // Small reflection probe for diagnostics (best-effort).
            try
            {
                var imType = typeof(Game.Input.InputManager);
                var instanceProp = imType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
                var im = instanceProp?.GetValue(null);

                var actionsField = imType.GetField("m_Actions", BindingFlags.NonPublic | BindingFlags.Instance);
                var dict = actionsField?.GetValue(im) as IDictionary;

                if (dict != null)
                {
                    int shown = 0;
                    foreach (var key in dict.Keys)
                    {
                        if (shown++ > 25)
                            break;
                        Dbg($"Action id: {key}");
                    }
                }
            }
            catch { }
#endif
        }

        protected override void OnUpdate()
        {
            ProxyAction? toggle = m_Toggle;
            if (toggle != null && toggle.WasPressedThisFrame())
            {
                var toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
                bool willEnable = toolSystem != null && m_Tool != null && toolSystem.activeTool != m_Tool;

#if DEBUG
                Dbg($"Toggle pressed → willEnable={willEnable}");
#endif
                if (m_Tool != null)
                    m_Tool.SetToolEnabled(willEnable);
            }
        }
    }
}
