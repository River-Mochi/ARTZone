// File: src/Tools/KeybindHotkeySystem.cs
// Purpose: Shift+Z toggles the tool (same as GameTopLeft). Optional invert hotkey retained.
// Notes:   NRE-hardened; guarded logging in DEBUG; tiny DEBUG-only action dump for diagnostics.

namespace AdvancedRoadTools.Systems
{
    using Game;           // GameSystemBase
    using Game.Input;     // ProxyAction
    using Game.Tools;     // ToolSystem
    using Unity.Entities; // World

#if DEBUG
    using System.Collections;
    using System.Reflection;
#endif

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        private ZoningControllerToolUISystem m_UI = null!;
        private ZoningControllerToolSystem m_Tool = null!;
        private ProxyAction? m_Toggle; // Shift+Z (or user binding) to toggle
        private ProxyAction? m_Invert; // Optional invert binding

#if DEBUG
        private static void Dbg(string m)
        {
            try
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Info("[ART][Hotkeys] " + m);
            }
            catch { /* swallow early logger issues */ }
        }
#else
        private static void Dbg(string m) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            // These GetOrCreate* calls are safe during world init; we still guard usage later.
            m_UI = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_Tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();

            // Read current bindings registered by the mod.
            m_Toggle = AdvancedRoadToolsMod.m_ToggleToolAction;
            m_Invert = AdvancedRoadToolsMod.m_InvertZoningAction;

#if DEBUG
            Dbg("Created; hotkeys wired.");

            // Tiny, safe reflection probe to list some input actions. Best-effort and throttled.
            // Helps verify the exact ID used for RMB (“Tool Secondary”) on a given build.
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
                            break; // throttle
                        Dbg($"Action id: {key}");
                    }
                }
                else
                {
                    Dbg("InputManager actions dictionary not accessible (DEBUG dump skipped).");
                }
            }
            catch
            {
                // Reflection is best-effort; never throw or spam here.
            }
#endif
        }

        protected override void OnUpdate()
        {
            // Toggle tool with Shift+Z (or whatever the user bound).
            ProxyAction? toggle = m_Toggle;
            if (toggle != null && toggle.WasPressedThisFrame())
            {
                var toolSystem = World.GetOrCreateSystemManaged<ToolSystem>(); // safe call
                bool willEnable = toolSystem != null && m_Tool != null && toolSystem.activeTool != m_Tool;

#if DEBUG
                Dbg($"Toggle pressed → willEnable={willEnable}");
#endif
                if (m_Tool != null)
                    m_Tool.SetToolEnabled(willEnable);
            }

            // Optional invert binding (kept for convenience).
            ProxyAction? invert = m_Invert;
            if (invert != null && invert.WasPressedThisFrame())
            {
#if DEBUG
                Dbg("Invert pressed");
#endif
                if (m_UI != null)
                    m_UI.FlipToolBothOrNone();
            }
        }
    }
}
