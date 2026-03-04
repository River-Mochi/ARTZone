// File: src/Tools/ZoneControlBridgeUI.cs
// Purpose:
//   - Expose UI bindings consumed by the React UI
//     (ToolZoningMode, RoadZoningMode, IsZonableRoadPrefab, ContourEnabled).
//   - Track tool/prefab changes for section visibility.
// Notes:
//   - Trigger handlers live in ZoneControlBridgeUI.Actions.cs.

namespace EasyZoning.Tools
{
    using Colossal.UI.Binding;      // ValueBinding, TriggerBinding
    using Game.Prefabs;             // PrefabBase, RoadPrefab
    using Game.Tools;               // ToolSystem, ToolBaseSystem
    using Game.UI;                  // UISystemBase
    using Unity.Mathematics;        // int2

    public partial class ZoneControlBridgeUI : UISystemBase
    {
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsZonableRoadPrefab = null!;
        private ValueBinding<bool> m_ContourEnabled = null!;

        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ZoningTool = null!;

        public ZoningMode ToolZoningMode => (ZoningMode) m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode) m_RoadZoningMode.value;
        public bool ContourEnabled => m_ContourEnabled.value;

        private static int2 DepthsFromMode(ZoningMode mode)
        {
            return new int2(
                (mode & ZoningMode.Left) != 0 ? 6 : 0,
                (mode & ZoningMode.Right) != 0 ? 6 : 0
            );
        }

        public int2 ToolDepths
        {
            get => DepthsFromMode(ToolZoningMode);
            set
            {
                ZoningMode mode = ZoningMode.None;
                if (value.x > 0)
                    mode |= ZoningMode.Left;
                if (value.y > 0)
                    mode |= ZoningMode.Right;

                SetToolZoningMode(mode);
            }
        }

        public int2 RoadDepths
        {
            get => DepthsFromMode(RoadZoningMode);
            set
            {
                ZoningMode mode = ZoningMode.None;
                if (value.x > 0)
                    mode |= ZoningMode.Left;
                if (value.y > 0)
                    mode |= ZoningMode.Right;

                ChangeRoadZoningMode((int) mode);
            }
        }

#if DEBUG
        private static void Dbg(string msg)
        {
            Colossal.Logging.ILog log = Mod.s_Log;
            if (log == null)
                return;

            try { log.Info("[EZ][UI] " + msg); } catch { }
        }

        private static string ModeToStr(ZoningMode z) =>
            z == ZoningMode.Both ? "Both"
            : z == ZoningMode.Left ? "Left"
            : z == ZoningMode.Right ? "Right"
            : "None";

        private void LogToolDepths(string tag)
        {
            ZoningMode mode = ToolZoningMode;
            int2 d = ToolDepths;
            Dbg($"{tag}: ToolZoningMode={ModeToStr(mode)} ToolDepths=({d.x},{d.y})");
        }
#else
        private static void Dbg(string _)
        {
        }
#endif

        protected override void OnCreate( )
        {
            base.OnCreate();

            AddBinding(m_ToolZoningMode =
                new ValueBinding<int>(Mod.ModID, "ToolZoningMode", (int) ZoningMode.Both));
            AddBinding(m_RoadZoningMode =
                new ValueBinding<int>(Mod.ModID, "RoadZoningMode", (int) ZoningMode.Both));
            AddBinding(m_IsZonableRoadPrefab =
                new ValueBinding<bool>(Mod.ModID, "IsZonableRoadPrefab", false));
            AddBinding(m_ContourEnabled =
                new ValueBinding<bool>(Mod.ModID, "ContourEnabled", false));

            // Triggers from UI (handlers are in ZoneControlBridgeUI.Actions.cs)
            AddBinding(new TriggerBinding<int>(Mod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(Mod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(Mod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(Mod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(Mod.ModID, "ToggleZoneControllerTool", ToggleTool));
            AddBinding(new TriggerBinding(Mod.ModID, "ToggleContourLines", ToggleContourLines));

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

            // Init visibility once.
            try
            {
                ToolBaseSystem activeTool = null!;
                PrefabBase activePrefab = null!;

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
                m_IsZonableRoadPrefab.Update(show);

#if DEBUG
                Dbg($"Init visibility → show={show}, tool={(activeTool != null ? activeTool.GetType().Name : "(null)")}, prefab={(activePrefab != null ? activePrefab.name : "(null)")}"); 
#endif
            }
            catch { }

#if DEBUG
            Dbg("ZoneControlBridgeUI created and bindings registered.");
#endif
        }

        protected override void OnDestroy( )
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

        private void OnToolChanged(ToolBaseSystem tool)
        {
            try
            {
                PrefabBase prefab = null!;
                try
                {
                    prefab = tool != null ? tool.GetPrefab() : null!;
                }
                catch { prefab = null!; }

                bool show = ShouldShowFor(tool, prefab);
                m_IsZonableRoadPrefab.Update(show);

#if DEBUG
                Dbg($"OnToolChanged: show={show} activeTool={(tool != null ? tool.GetType().Name : "(null)")} prefab={(prefab != null ? prefab.name : "(null)")}"); 
#endif
            }
            catch { }
        }

        private void OnPrefabChanged(PrefabBase prefab)
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
                m_IsZonableRoadPrefab.Update(show);

#if DEBUG
                Dbg($"OnPrefabChanged: show={show} prefab={(prefab != null ? prefab.name : "(null)")} tool={(tool != null ? tool.GetType().Name : "(null)")}"); 
#endif
            }
            catch { }
        }

        private static bool ShouldShowFor(ToolBaseSystem? tool, PrefabBase? prefab)
        {
            try
            {
                if (tool is ZoningControllerToolSystem)
                    return true;

                return IsZonableRoadPrefab(prefab);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsZonableRoadPrefab(PrefabBase? prefab)
        {
            if (prefab is not RoadPrefab road)
                return false;

            if (road.m_ZoneBlock == null)
                return false;

            if (road.m_HighwayRules)
                return false;

            return true;
        }
    }
}
