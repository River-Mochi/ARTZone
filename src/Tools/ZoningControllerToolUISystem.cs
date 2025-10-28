// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose UI bindings the React UI reads/writes (ToolZoningMode, RoadZoningMode, IsRoadPrefab).
//  • Handle triggers (Change/Flip/Toggle) with null guards.
//  • Show the “Zoning Side” section when our tool is active OR a RoadPrefab is active.
// FULL DROP-IN

namespace EasyZoning.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsRoadPrefab = null!; // “should show section”

        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ZoningTool = null!;

        public ZoningMode ToolZoningMode => (ZoningMode)m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode)m_RoadZoningMode.value;

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

#if DEBUG
        private static void Dbg(string msg)
        {
            var log = EasyZoningMod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info("[EZ][UI] " + msg);
            }
            catch { }
        }
        private static string ModeToStr(ZoningMode z) =>
            z == ZoningMode.Both ? "Both" : z == ZoningMode.Left ? "Left" : z == ZoningMode.Right ? "Right" : "None";
#else
        private static void Dbg(string msg)
        {
        }
#endif

        protected override void OnCreate()
        {
            base.OnCreate();

            AddBinding(m_ToolZoningMode = new ValueBinding<int>(EasyZoningMod.ModID, "ToolZoningMode", (int)ZoningMode.Both));
            AddBinding(m_RoadZoningMode = new ValueBinding<int>(EasyZoningMod.ModID, "RoadZoningMode", (int)ZoningMode.Both));
            AddBinding(m_IsRoadPrefab = new ValueBinding<bool>(EasyZoningMod.ModID, "IsRoadPrefab", false)); // “should show section”

            AddBinding(new TriggerBinding<int>(EasyZoningMod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(EasyZoningMod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(EasyZoningMod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(EasyZoningMod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(EasyZoningMod.ModID, "ToggleZoneControllerTool", ToggleTool));

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
            catch { }

            try
            {
                m_ZoningTool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            }
            catch { }

            try
            {
                ToolBaseSystem activeTool = null!;
                Game.Prefabs.PrefabBase activePrefab = null!;
                if (m_MainToolSystem != null)
                {
                    activeTool = m_MainToolSystem.activeTool;
                    try
                    {
                        activePrefab = activeTool != null ? activeTool.GetPrefab() : null!;
                    }
                    catch { activePrefab = null!; }
                }
                bool show = ShouldShowFor(activeTool, activePrefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg($"Init visibility → show={show}, tool={(activeTool != null ? activeTool.GetType().Name : "(null)")}, prefab={(activePrefab != null ? activePrefab.name : "(null)")}");
#endif
            }
            catch { }

#if DEBUG
            Dbg("UISystem created and bindings registered.");
#endif
        }

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

        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        private void OnToolChanged(ToolBaseSystem tool)
        {
            try
            {
                Game.Prefabs.PrefabBase prefab = null!;
                try
                {
                    prefab = tool != null ? tool.GetPrefab() : null!;
                }
                catch { prefab = null!; }
                bool show = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg($"OnToolChanged: show={show} activeTool={(tool != null ? tool.GetType().Name : "(null)")} prefab={(prefab != null ? prefab.name : "(null)")}");
#endif
            }
            catch { }
        }

        private void OnPrefabChanged(Game.Prefabs.PrefabBase prefab)
        {
            try
            {
                ToolBaseSystem tool = null!;
                try
                {
                    tool = (m_MainToolSystem != null) ? m_MainToolSystem.activeTool : null!;
                }
                catch { tool = null!; }
                bool show = ShouldShowFor(tool, prefab);
                m_IsRoadPrefab.Update(show);
#if DEBUG
                Dbg($"OnPrefabChanged: show={show} prefab={(prefab != null ? prefab.name : "(null)")} tool={(tool != null ? tool.GetType().Name : "(null)")}");
#endif
            }
            catch { }
        }

        private void ToggleTool()
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

        private void FlipToolBothMode()
        {
            try
            {
                var next = (ToolZoningMode == ZoningMode.Both) ? ZoningMode.None : ZoningMode.Both;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"FlipToolBothMode → Tool={ModeToStr(next)}");
#endif
            }
            catch { }
        }

        private void FlipRoadBothMode()
        {
            try
            {
                var next = (RoadZoningMode == ZoningMode.Both) ? ZoningMode.None : ZoningMode.Both;
                m_RoadZoningMode.Update((int)next);
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
                Dbg($"ChangeToolZoningMode → Tool={ModeToStr((ZoningMode)value)}");
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
                Dbg($"ChangeRoadZoningMode → Road={ModeToStr((ZoningMode)value)}");
#endif
            }
            catch { }
        }

        public void SetToolZoningMode(ZoningMode mode)
        {
            try
            {
                m_ToolZoningMode.Update((int)mode);
#if DEBUG
                Dbg($"SetToolZoningMode → Tool={ModeToStr(mode)}");
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
                Dbg($"FlipToolBothOrNone → Tool={ModeToStr(next)}");
#endif
            }
            catch { }
        }

        public void InvertZoningSideOnly()
        {
            try
            {
                var mode = ToolZoningMode;
                var next =
                    mode == ZoningMode.Left ? ZoningMode.Right :
                    mode == ZoningMode.Right ? ZoningMode.Left :
                                               ZoningMode.Left;
                m_ToolZoningMode.Update((int)next);
#if DEBUG
                Dbg($"InvertZoningSideOnly → Tool={ModeToStr(next)}");
#endif
            }
            catch { }
        }

        public void RmbPreviewToggle()
        {
            try
            {
                if (ToolZoningMode == ZoningMode.Left || ToolZoningMode == ZoningMode.Right)
                    InvertZoningSideOnly();
                else
                    FlipToolBothOrNone();
            }
            catch { }
        }

        private static bool ShouldShowFor(ToolBaseSystem? tool, Game.Prefabs.PrefabBase? prefab)
        {
            try
            {
                if (tool is ZoningControllerToolSystem)
                    return true;
                if (prefab is RoadPrefab)
                    return true;
                return false;
            }
            catch { return false; }
        }
    }
}
