// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose bindings the UI reads/writes (ToolZoningMode, RoadZoningMode, IsRoadPrefab).
//  • Handle triggers (Change/Flip/Toggle).
//  • NRE guards and DEBUG logs in all event handlers.
//  • Public helpers SetToolZoningMode(...), FlipToolBothOrNone(), InvertZoningSideOnly().

namespace ARTZone.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        // ----  UI Bindings ----------------------------------------------------
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!; // treated as "ShouldShowPanel"

        // ----  Tool Access ----------------------------------------------------
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ToolSystem = null!;

        // ----  Public Helpers -------------------------------------------------
        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

        // Convert mode<->depths (6 = on, 0 = off)
        public int2 ToolDepths
        {
            get => new(
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                ((ZoningMode)m_ToolZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
            set
            {
                var mode = ZoningMode.Both;
                if (value.x == 0)
                    mode ^= ZoningMode.Left;
                if (value.y == 0)
                    mode ^= ZoningMode.Right;
                SetToolZoningMode(mode);
            }
        }

        public int2 RoadDepths
        {
            get => new(
                ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Left) == ZoningMode.Left ? 6 : 0,
                ((ZoningMode)m_RoadZoningMode.value & ZoningMode.Right) == ZoningMode.Right ? 6 : 0);
            set
            {
                var mode = ZoningMode.Both;
                if (value.x == 0)
                    mode ^= ZoningMode.Left;
                if (value.y == 0)
                    mode ^= ZoningMode.Right;
                ChangeRoadZoningMode((int)mode);
            }
        }

        // ----  Debug ----------------------------------------------------------
#if DEBUG
        private static void Dbg(string msg)
        {
            var log = ARTZoneMod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info("[ART][UI] " + msg);
            }
            catch { }
        }
#else
        private static void Dbg(string msg) { }
#endif

        // ----  Lifecycle: OnCreate -------------------------------------------
        protected override void OnCreate()
        {
            base.OnCreate();

            // Bindings (IDs must match TS)
            AddBinding(m_ToolZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(ARTZoneMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(ARTZoneMod.ModID, "IsRoadPrefab", false));

            // Triggers (from TS)
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(ARTZoneMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(ARTZoneMod.ModID, "ToggleZoneControllerTool", ToggleTool));

            // Observe vanilla tool/prefab to decide when to show the UI section (robust, null-safe)
            try
            {
                m_MainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
                if (m_MainToolSystem != null)
                {
                    m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged -= OnToolChanged;
                    m_MainToolSystem.EventPrefabChanged += OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged += OnToolChanged;
                }
            }
            catch { /* defensive */ }

            // Our tool instance (may be null if creation failed; we guard later)
            try
            {
                m_ToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            }
            catch { /* guard in ToggleTool() anyway */ }

            // Initialize visibility once, based on current tool/prefab
            try
            {
                ToolBaseSystem? activeTool = null;
                PrefabBase? activePrefab = null;

                if (m_MainToolSystem != null)
                {
                    activeTool = m_MainToolSystem.activeTool;
                    try
                    {
                        activePrefab = activeTool != null ? activeTool.GetPrefab() : null;
                    }
                    catch { activePrefab = null; }
                }

                var show = ShouldShowFor(activeTool, activePrefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg($"Init visibility → show={show}, tool={(activeTool != null ? activeTool.GetType().Name : "(null)")}, prefab={(activePrefab != null ? activePrefab.name : "(null)")}");
#endif
            }
            catch { /* keep default false */ }

#if DEBUG
            Dbg("UISystem created and bindings registered.");
#endif
        }

        // ----  Lifecycle: OnDestroy ------------------------------------------
        protected override void OnDestroy()
        {
            try
            {
                if (m_MainToolSystem != null)
                {
                    m_MainToolSystem.EventPrefabChanged -= OnPrefabChanged;
                    m_MainToolSystem.EventToolChanged -= OnToolChanged;
                }
            }
            catch { }
            base.OnDestroy();
        }

        // ----  Lifecycle: OnUpdate -------------------------------------------
        protected override void OnUpdate()
        {
            // UI bindings only; hotkey toggle is handled elsewhere.
            base.OnUpdate();
        }

        // ----  Event Handlers -------------------------------------------------
        private void OnToolChanged(ToolBaseSystem tool)
        {
            try
            {
                // tool may or may not be our tool; prefab may be null
                PrefabBase? prefab = null;
                try
                {
                    prefab = tool != null ? tool.GetPrefab() : null;
                }
                catch { prefab = null; }

                bool show = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg($"OnToolChanged: show={show} activeTool={(tool != null ? tool.GetType().Name : "(null)")}, prefab={(prefab != null ? prefab.name : "(null)")}");
#endif
            }
            catch { }
        }

        private void OnPrefabChanged(PrefabBase prefab)
        {
            try
            {
                // Re-evaluate against current tool to keep logic consistent
                ToolBaseSystem? tool = null;
                try
                {
                    tool = (m_MainToolSystem != null) ? m_MainToolSystem.activeTool : null;
                }
                catch { tool = null; }

                bool show = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg($"OnPrefabChanged: show={show} prefab={(prefab != null ? prefab.name : "(null)")}, tool={(tool != null ? tool.GetType().Name : "(null)")}");
#endif
            }
            catch { }
        }

        // ----  Triggers from TS -----------------------------------------------------
        private void ToggleTool()
        {
            try
            {
                if (m_MainToolSystem == null || m_ToolSystem == null)
                    return;

                bool enable = m_MainToolSystem.activeTool != m_ToolSystem;
                m_ToolSystem.SetToolEnabled(enable);
#if DEBUG
                Dbg($"ToggleTool → enable={enable} active={(m_MainToolSystem?.activeTool != null ? m_MainToolSystem.activeTool.GetType().Name : "(null)")} ");
#endif
            }
            catch { }
        }

        private void FlipToolBothMode()
        {
            try
            {
                m_ToolZoningMode.Update(ToolZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
#if DEBUG
                Dbg($"FlipToolBothMode → now={(ZoningMode)m_ToolZoningMode.value}");
#endif
            }
            catch { }
        }

        private void FlipRoadBothMode()
        {
            try
            {
                m_RoadZoningMode.Update(RoadZoningMode == ZoningMode.Both ? (int)ZoningMode.None : (int)ZoningMode.Both);
#if DEBUG
                Dbg($"FlipRoadBothMode → now={(ZoningMode)m_RoadZoningMode.value}");
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
                Dbg($"ChangeToolZoningMode({(ZoningMode)value}) → now={(ZoningMode)m_ToolZoningMode.value}");
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
                Dbg($"ChangeRoadZoningMode({(ZoningMode)value}) → now={(ZoningMode)m_RoadZoningMode.value}");
#endif
            }
            catch { }
        }

        // ----  Public Helpers for the Tool -----------------------------------------
        public void SetToolZoningMode(ZoningMode mode)
        {
            try
            {
                m_ToolZoningMode.Update((int)mode);
#if DEBUG
                Dbg($"SetToolZoningMode({mode}) → now={(ZoningMode)m_ToolZoningMode.value}");
#endif
            }
            catch { }
        }

        public void FlipToolBothOrNone()
        {
            try
            {
                var next = ToolZoningMode == ZoningMode.Both ? ZoningMode.None :
                           ToolZoningMode == ZoningMode.None ? ZoningMode.Both : ToolZoningMode;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"FlipToolBothOrNone → {next} (now={(ZoningMode)m_ToolZoningMode.value})");
#endif
            }
            catch { }
        }

        /// <summary>
        /// Toggle Left <-> Right. If called while mode is Both/None, set Left (seed the side-only toggle).
        /// </summary>
        public void InvertZoningSideOnly()
        {
            try
            {
                var mode = ToolZoningMode;
                ZoningMode next =
                    mode == ZoningMode.Left ? ZoningMode.Right :
                    mode == ZoningMode.Right ? ZoningMode.Left :
                    ZoningMode.Left;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"InvertZoningSideOnly → {next} (now={(ZoningMode)m_ToolZoningMode.value})");
#endif
            }
            catch { }
        }

        /// <summary>RMB preview toggle: Left<->Right if on a side; otherwise Both<->None.</summary>
        public void RmbPreviewToggle()
        {
            try
            {
#if DEBUG
                Dbg($"RmbPreviewToggle (pre) ToolZoningMode={ToolZoningMode}");
#endif
                if (ToolZoningMode == ZoningMode.Left || ToolZoningMode == ZoningMode.Right)
                    InvertZoningSideOnly();      // strict Left↔Right
                else
                    FlipToolBothOrNone();        // strict Both↔None
#if DEBUG
                Dbg($"RmbPreviewToggle (post) ToolZoningMode={ToolZoningMode}");
#endif
            }
            catch { }
        }

        // ----  Visibility Logic (Null-Safe) ----------------------------------
        // Show our section if:
        //  • our tool is the active tool, OR
        //  • the active prefab is a RoadPrefab (vanilla road-building UI context).
        private static bool ShouldShowFor(ToolBaseSystem? tool, PrefabBase? prefab)
        {
            try
            {
                if (tool is ZoningControllerToolSystem)
                    return true;

                if (prefab is RoadPrefab)
                    return true;

                // Fallback: if tool is null, try its prefab via null-guard (already done above),
                // but keep it strictly false otherwise to avoid surprises.
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
