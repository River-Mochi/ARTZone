// File: src/Tools/ZoneControlBridgeUI.Actions.cs
// Purpose:
//  • UI trigger handlers and small helper actions for ZoneControlBridgeUI.
//  • Contains CycleMode() (RMB behavior) and toggle/apply UI bindings.
//  • Contour line snap.

namespace EasyZoning.Tools
{
    using Game.Tools;   // Snap, ToolSystem

    public partial class ZoneControlBridgeUI
    {
        // UI -> C# log bridge (writes console problems to EasyZoning.log).
        private void UILogWarn(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                    return;

                Mod.s_Log?.Warn("[EZ][UI] " + message);
            }
            catch
            {
            }
        }

        // Toggle EZ tool on/off.
        // PhotoMode enable-guard enforced inside ZoningControllerToolSystem.SetToolEnabled().
        private void ToggleTool( )
        {
            try
            {
                if (m_MainToolSystem == null || m_ZoningTool == null)
                    return;

                bool isActive = (m_MainToolSystem.activeTool == m_ZoningTool);
                bool enable = !isActive;

                m_ZoningTool.SetToolEnabled(enable);

#if DEBUG
                Dbg("ToggleTool → enable=" + enable);
#endif
            }
            catch
            {
                // Silent by design: UI triggers must never break the frame.
            }
        }

        // Flip tool depth mode: Both <-> None.
        private void FlipToolBothMode( )
        {
            try
            {
                ZoningMode next = (ToolZoningMode == ZoningMode.Both) ? ZoningMode.None : ZoningMode.Both;
                m_ToolZoningMode.Update((int) next);

#if DEBUG
                Dbg($"FlipToolBothMode → Tool={ModeToStr(next)}");
                LogToolDepths("FlipToolBothMode");
#endif
            }
            catch
            {
            }
        }

        // Flip road placement depth mode: Both <-> None.
        private void FlipRoadBothMode( )
        {
            try
            {
                ZoningMode next = (RoadZoningMode == ZoningMode.Both) ? ZoningMode.None : ZoningMode.Both;
                m_RoadZoningMode.Update((int) next);

#if DEBUG
                Dbg($"FlipRoadBothMode → Road={ModeToStr(next)}");
#endif
            }
            catch
            {
            }
        }

        // Set tool mode from UI (raw int from bindings).
        private void ChangeToolZoningMode(int value)
        {
            try
            {
                m_ToolZoningMode.Update(value);

#if DEBUG
                Dbg($"ChangeToolZoningMode → Tool={ModeToStr((ZoningMode) value)} rawValue={value}");
                LogToolDepths("ChangeToolZoningMode");
#endif
            }
            catch
            {
            }
        }

        // Set road placement mode from UI (raw int from bindings).
        private void ChangeRoadZoningMode(int value)
        {
            try
            {
                m_RoadZoningMode.Update(value);

#if DEBUG
                Dbg($"ChangeRoadZoningMode → Road={ModeToStr((ZoningMode) value)} rawValue={value}");
#endif
            }
            catch
            {
            }
        }

        // Programmatic tool mode set (used by ToolDepths setter).
        public void SetToolZoningMode(ZoningMode mode)
        {
            try
            {
                m_ToolZoningMode.Update((int) mode);

#if DEBUG
                Dbg($"SetToolZoningMode → Tool={ModeToStr(mode)}");
                LogToolDepths("SetToolZoningMode");
#endif
            }
            catch
            {
            }
        }

        /// <summary>
        /// Update panel: RMB cycle behavior
        /// - Default (LegacyRightClickCycle OFF) do all: Both → Left → Right → None → ...
        /// - Legacy (LegacyRightClickCycle ON) do 2 sets: Left <-> Right "OR" Both <-> None
        /// </summary>
        public void CycleMode( )
        {
            try
            {
                // Read setting each time (supports live toggle, click icon, instant highlight)
                bool legacy = Mod.Settings != null && Mod.Settings.LegacyRightClickCycle;

                ZoningMode current = ToolZoningMode;

                ZoningMode next;
                if (legacy)
                {
                    next = current switch
                    {
                        ZoningMode.Left => ZoningMode.Right,
                        ZoningMode.Right => ZoningMode.Left,
                        ZoningMode.Both => ZoningMode.None,
                        ZoningMode.None => ZoningMode.Both,
                        _ => ZoningMode.Both
                    };
                }
                else
                {
                    next = current switch
                    {
                        ZoningMode.Both => ZoningMode.Left,
                        ZoningMode.Left => ZoningMode.Right,
                        ZoningMode.Right => ZoningMode.None,
                        ZoningMode.None => ZoningMode.Both,
                        _ => ZoningMode.Both
                    };
                }

                m_ToolZoningMode.Update((int) next);

#if DEBUG
                Dbg($"CycleMode → Tool={ModeToStr(next)} legacy={legacy}");
                LogToolDepths("CycleMode");
#endif
            }
            catch
            {
            }
        }

        // Toggle contour line snapping on the currently active tool.
        private void ToggleContourLines( )
        {
            try
            {
                // Update mod binding first (UI state).
                bool next = !ContourEnabled;
                m_ContourEnabled.Update(next);

                // Apply to the active tool snap flags.
                ToolSystem toolSystem = m_MainToolSystem;
                if (toolSystem == null)
                    return;

                ToolBaseSystem active = toolSystem.activeTool;
                if (active == null)
                    return;

                try
                {
                    Snap snap = active.selectedSnap;

                    if (next)
                        snap |= Snap.ContourLines;
                    else
                        snap &= ~Snap.ContourLines;

                    active.selectedSnap = snap;

#if DEBUG
                    Dbg($"ToggleContourLines → {(next ? "ON" : "OFF")} selectedSnap={snap}");
#endif
                }
                catch
                {
                }
            }
            catch
            {
            }
        }
    }
}
