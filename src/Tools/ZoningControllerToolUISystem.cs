// File: src/Tools/ZoningControllerToolUISystem.cs
// Purpose:
//  • Expose UI bindings the React UI reads/writes
//    (ToolZoningMode, RoadZoningMode, IsZonableRoadPrefab, IsPhotoMode, ContourEnabled).
//  • Track tool/prefab changes for section visibility.
// Notes:
//  • Action/trigger handlers live in ZoningControllerToolUISystem.Actions.cs.

namespace EasyZoning.Tools
{
    using Colossal.UI.Binding;
    using Game.Prefabs;
    using Game.Rendering;
    using Game.Tools;
    using Game.UI;
    using Unity.Mathematics;

    public partial class ZoningControllerToolUISystem : UISystemBase
    {
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsZonableRoadPrefab = null!;  // true when zonable road prefab is active (UI section visibility)
        private ValueBinding<bool> m_ContourEnabled = null!;       // contour toggle in update panel

        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ZoningTool = null!;
        private PhotoModeRenderSystem m_PhotoModeSystem = null!;

        public ZoningMode ToolZoningMode => (ZoningMode) m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode) m_RoadZoningMode.value;
        public bool ContourEnabled => m_ContourEnabled.value;

        /// <summary>
        /// Convert a ZoningMode into engine depths.
        /// Convention for this mod: Depths.x = LEFT side, Depths.y = RIGHT side.
        /// </summary>
        private static int2 DepthsFromMode(ZoningMode mode)
        {
            return new int2(
                (mode & ZoningMode.Left) != 0 ? 6 : 0,  // x = left
                (mode & ZoningMode.Right) != 0 ? 6 : 0  // y = right
            );
        }

        /// <summary>
        /// Current Easy Zoning tool depths (update-existing-roads mode).
        /// </summary>
        public int2 ToolDepths
        {
            get => DepthsFromMode(ToolZoningMode);
            set
            {
                var mode = ZoningMode.None;
                if (value.x > 0)
                    mode |= ZoningMode.Left;
                if (value.y > 0)
                    mode |= ZoningMode.Right;

                SetToolZoningMode(mode);
            }
        }

        /// <summary>
        /// Current vanilla road-tool depths (new-roads mode).
        /// </summary>
        public int2 RoadDepths
        {
            get => DepthsFromMode(RoadZoningMode);
            set
            {
                var mode = ZoningMode.None;
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
            var log = Mod.s_Log;
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
            var mode = ToolZoningMode;
            var d = ToolDepths;
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

            // Photo mode system → drives IsPhotoMode binding used by UI to hide panel/buttons.
            m_PhotoModeSystem = World.GetOrCreateSystemManaged<PhotoModeRenderSystem>();
            AddUpdateBinding(new GetterValueBinding<bool>(
                Mod.ModID,
                "IsPhotoMode",
                ( ) => m_PhotoModeSystem != null && m_PhotoModeSystem.Enabled));

            // Triggers from UI (handlers are in the .Actions.cs partial)
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
            Dbg("UISystem created and bindings registered.");
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

            // Roads without ZoneBlock cannot zone.
            if (road.m_ZoneBlock == null)
                return false;

            // Highways do not support zoning.
            if (road.m_HighwayRules)
                return false;

            return true;
        }
    }
}
