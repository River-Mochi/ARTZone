// File: src/Tools/ZoningControllerToolUISystem.Actions.cs
// Purpose:
//  • UI trigger handlers and small helper actions for ZoningControllerToolUISystem.
//  • Contains CycleMode() (RMB behavior) and toggle/apply UI bindings.

namespace EasyZoning.Tools
{
    using Game.Tools;

    public partial class ZoningControllerToolUISystem
    {
        private void ToggleTool( )
        {
            try
            {
                if (m_MainToolSystem == null || m_ZoningTool == null)
                    return;

                bool enable = m_MainToolSystem.activeTool != m_ZoningTool;
                m_ZoningTool.SetToolEnabled(enable);

#if DEBUG
                Dbg($"ToggleTool → enable={enable}");
#endif
            }
            catch { }
        }

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
            catch { }
        }

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
            catch { }
        }

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
            catch { }
        }

        private void ChangeRoadZoningMode(int value)
        {
            try
            {
                m_RoadZoningMode.Update(value);

#if DEBUG
                Dbg($"ChangeRoadZoningMode → Road={ModeToStr((ZoningMode) value)} rawValue={value}");
#endif
            }
            catch { }
        }

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
            catch { }
        }

        /// <summary>
        /// RMB cycle behavior for the update tool:
        /// - Default (LegacyRightClickCycle OFF): Both → Left → Right → None → ...
        /// - Legacy (LegacyRightClickCycle ON): Left ↔ Right, Both ↔ None
        /// </summary>
        public void CycleMode( )
        {
            try
            {
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

        private void ToggleContourLines( )
        {
            try
            {
                bool next = !ContourEnabled;
                m_ContourEnabled.Update(next);

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
                    Dbg($"ToggleContourLines → {(next ? "ON" : "OFF")}  selectedSnap={snap}");
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
