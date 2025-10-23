// File: src/Tools/KeybindHotkeySystem.cs
// Purpose: Shift+Z toggles the tool (same as GameTopLeft). Optional invert hotkey retained.
// Notes:   NRE-hardened; guarded logging in DEBUG; no #nullable usage.

namespace AdvancedRoadTools.Systems
{
    using Game;
    using Game.Input;
    using Game.Tools;
    using Unity.Entities;

    public sealed partial class KeybindHotkeySystem : GameSystemBase
    {
        private ZoningControllerToolUISystem m_UI = null!;
        private ZoningControllerToolSystem m_Tool = null!;
        private ProxyAction? m_Toggle;
        private ProxyAction? m_Invert;

#if DEBUG
        private static void dbg(string m)
        {
            try
            {
                var log = AdvancedRoadToolsMod.s_Log;
                if (log != null)
                    log.Info("[ART][Hotkeys] " + m);
            }
            catch { }
        }
#else
        private static void dbg(string m) { }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();
            m_UI = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_Tool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            m_Toggle = AdvancedRoadToolsMod.m_ToggleToolAction;
            m_Invert = AdvancedRoadToolsMod.m_InvertZoningAction;

#if DEBUG
            dbg("Created; hotkeys wired.");
#endif
        }

        protected override void OnUpdate()
        {
            // Toggle tool with Shift+Z (or whatever user bound)
            if (m_Toggle != null && m_Toggle.WasPressedThisFrame())
            {
                var toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
                bool willEnable = (toolSystem != null && m_Tool != null) && (toolSystem.activeTool != m_Tool);

#if DEBUG
                dbg($"Toggle pressed → willEnable={willEnable}");
#endif
                if (m_Tool != null)
                    m_Tool.SetToolEnabled(willEnable);
            }

            // Optional invert binding
            if (m_Invert != null && m_Invert.WasPressedThisFrame())
            {
#if DEBUG
                dbg("Invert pressed");
#endif
                if (m_UI != null)
                    m_UI.InvertZoningMode();
            }
        }
    }
}
