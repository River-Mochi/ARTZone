// File: src/Tools/ZoneControlBridgeUI.Actions.cs
// Purpose:
//  • UI trigger handlers and small helper actions for ZoneControlBridgeUI.
//  • Contains CycleMode() (RMB behavior) and toggle/apply UI bindings.

namespace EasyZoning.Tools
{
    using Game.Tools;   // Snap, ToolSystem

    public partial class ZoneControlBridgeUI
    {
        // Toggle EZ tool on/off.
        // Safety: refuse enabling while PhotoMode is active (or PhotoMode state cannot be read).
        private void ToggleTool( )
        {
            try
            {
                // Required systems must exist.
                if (m_MainToolSystem == null || m_ZoningTool == null)
                    return;

                // Determine whether the request is enable vs disable.
                // Reference compare (not type): only treat the exact instance as "active".
                bool isActive = (m_MainToolSystem.activeTool == m_ZoningTool);

                // Disabling is always allowed (even during PhotoMode / even if PhotoMode check throws).
                if (isActive)
                {
                    m_ZoningTool.SetToolEnabled(false);

#if DEBUG
                    Dbg("ToggleTool → enable=False (disable request)");
#endif
                    return;
                }

                // Enabling: guard against Photo Mode.
                bool photoModeEnabled;
                try
                {
                    photoModeEnabled = (m_PhotoModeSystem != null && m_PhotoModeSystem.Enabled);
                }
                catch
                {
                    // Fail-closed for enabling: assume PhotoMode could be active if the state cannot be read.
                    photoModeEnabled = true;

#if DEBUG
                    Dbg("ToggleTool blocked (Photo Mode state read failed).");
#endif
                }

                if (photoModeEnabled)
                {
#if DEBUG
                    Dbg("ToggleTool blocked (Photo Mode).");
#endif
                    return;
                }

                // Enable request (non-Photo Mode).
                m_ZoningTool.SetToolEnabled(true);

#if DEBUG
                Dbg("ToggleTool → enable=True");
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
        /// - Default (LegacyRightClickCycle OFF): Both → Left → Right → None → ...
        /// - Legacy (LegacyRightClickCycle ON): Left <-> Right "OR" Both <-> None
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
