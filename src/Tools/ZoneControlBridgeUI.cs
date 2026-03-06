// File: src/Tools/ZoneControlBridgeUI.cs
// Purpose:
//   - Expose UI bindings consumed by React
//     (ToolZoningMode, RoadZoningMode, IsZonableRoadPrefab, ContourEnabled, IsPhotoMode).
//   - Track tool/prefab changes for section visibility.
//   - Safety latch: when Photo Mode turns ON, disable EZ tool once if it is active.
// Notes: Trigger handlers live in ZoneControlBridgeUI.Actions.cs.

namespace EasyZoning.Tools
{
    using Colossal.UI.Binding;      // ValueBinding, TriggerBinding, GetterValueBinding
    using Game.Prefabs;             // PrefabBase, RoadPrefab
    using Game.Rendering;           // PhotoModeRenderSystem
    using Game.Tools;               // ToolSystem, ToolBaseSystem
    using Game.UI;                  // UISystemBase
    using Unity.Mathematics;        // int2

    public partial class ZoneControlBridgeUI : UISystemBase
    {
        // UI bindings consumed by React.
        // (React reads these via ModID + binding name strings.)
        private ValueBinding<bool> m_ContourEnabled = null!;
        private ValueBinding<int> m_ToolZoningMode = null!;
        private ValueBinding<int> m_RoadZoningMode = null!;
        private ValueBinding<bool> m_IsZonableRoadPrefab = null!;
      

        // Game systems used for tool state + Photo Mode guard.
        private ToolSystem m_MainToolSystem = null!;
        private ZoningControllerToolSystem m_ZoningTool = null!;
        private PhotoModeRenderSystem m_PhotoModeSystem = null!;

        // Tracks last Photo Mode state to detect OFF -> ON edge.
        private bool m_LastPhotoModeEnabled;

        // Convenience getters for current binding values.
        public ZoningMode ToolZoningMode => (ZoningMode) m_ToolZoningMode.value;
        public ZoningMode RoadZoningMode => (ZoningMode) m_RoadZoningMode.value;
        public bool ContourEnabled => m_ContourEnabled.value;

        // Converts a mode (Left/Right/Both/None) into depth values used by UI sliders.
        private static int2 DepthsFromMode(ZoningMode mode)
        {
            return new int2(
                (mode & ZoningMode.Left) != 0 ? 6 : 0,
                (mode & ZoningMode.Right) != 0 ? 6 : 0
            );
        }

        // Tool depth "view" (used by React controls).
        public int2 ToolDepths
        {
            get => DepthsFromMode(ToolZoningMode);
            set
            {
                // Build a mode enum from depth values.
                ZoningMode mode = ZoningMode.None;
                if (value.x > 0)
                    mode |= ZoningMode.Left;
                if (value.y > 0)
                    mode |= ZoningMode.Right;

                // Setter routes to handler in Actions file.
                SetToolZoningMode(mode);
            }
        }

        // Road placement depth "view" (used by React controls).
        public int2 RoadDepths
        {
            get => DepthsFromMode(RoadZoningMode);
            set
            {
                // Build a mode enum from depth values.
                ZoningMode mode = ZoningMode.None;
                if (value.x > 0)
                    mode |= ZoningMode.Left;
                if (value.y > 0)
                    mode |= ZoningMode.Right;

                // Setter routes to handler in Actions file.
                ChangeRoadZoningMode((int) mode);
            }
        }

#if DEBUG
        // Debug logging helper (writes to mod log only).
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

        // Debug-only: logs current tool mode + computed depths.
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

            // Bindings: provide state values for React to read/write.
            AddBinding(m_ToolZoningMode =
                new ValueBinding<int>(Mod.ModID, "ToolZoningMode", (int) ZoningMode.Both));
            AddBinding(m_RoadZoningMode =
                new ValueBinding<int>(Mod.ModID, "RoadZoningMode", (int) ZoningMode.Both));
            AddBinding(m_IsZonableRoadPrefab =
                new ValueBinding<bool>(Mod.ModID, "IsZonableRoadPrefab", false));
            AddBinding(m_ContourEnabled =
                new ValueBinding<bool>(Mod.ModID, "ContourEnabled", false));

            // PhotoMode: expose a read-only binding for UI checks.
            // Also used internally for the safety latch.
            m_PhotoModeSystem = World.GetOrCreateSystemManaged<PhotoModeRenderSystem>();
            AddUpdateBinding(new GetterValueBinding<bool>(
                Mod.ModID,
                "IsPhotoMode",
                ( ) => m_PhotoModeSystem != null && m_PhotoModeSystem.Enabled));

            // Trigger bindings: React calls these; handlers are in Actions.cs.
            AddBinding(new TriggerBinding<int>(Mod.ModID, "ChangeRoadZoningMode", ChangeRoadZoningMode));
            AddBinding(new TriggerBinding<int>(Mod.ModID, "ChangeToolZoningMode", ChangeToolZoningMode));
            AddBinding(new TriggerBinding(Mod.ModID, "FlipToolBothMode", FlipToolBothMode));
            AddBinding(new TriggerBinding(Mod.ModID, "FlipRoadBothMode", FlipRoadBothMode));
            AddBinding(new TriggerBinding(Mod.ModID, "ToggleZoneControllerTool", ToggleTool));
            AddBinding(new TriggerBinding(Mod.ModID, "ToggleContourLines", ToggleContourLines));

            // ToolSystem events drive section visibility updates.
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

            // Cache EZ tool instance (used for enable/disable calls).
            try
            {
                m_ZoningTool = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
            }
            catch { }

            // Initialize PhotoMode edge tracking state.
            try
            {
                m_LastPhotoModeEnabled = m_PhotoModeSystem != null && m_PhotoModeSystem.Enabled;
            }
            catch
            {
                m_LastPhotoModeEnabled = false;
            }

            // Initialize section visibility once at creation time.
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

        protected override void OnUpdate( )
        {
            base.OnUpdate();

            // Read PhotoMode state each frame (needed for OFF -> ON detection).
            bool photoModeEnabled;
            try
            {
                photoModeEnabled = m_PhotoModeSystem != null && m_PhotoModeSystem.Enabled;
            }
            catch
            {
                photoModeEnabled = false;
            }

            // PhotoMode transition OFF -> ON:
            // Disable EZ tool once if it is active, to avoid tool/input conflicts.
            if (photoModeEnabled && !m_LastPhotoModeEnabled)
            {
                try
                {
                    ToolBaseSystem active = (m_MainToolSystem != null) ? m_MainToolSystem.activeTool : null!;
                    if (active is ZoningControllerToolSystem && m_ZoningTool != null)
                    {
                        m_ZoningTool.SetToolEnabled(false);
#if DEBUG
                        Dbg("Photo Mode entered -> EZ tool disabled.");
#endif
                    }
                }
                catch
                {
                }
            }

            // Update edge-tracking state.
            m_LastPhotoModeEnabled = photoModeEnabled;
        }

        protected override void OnDestroy( )
        {
            // Unhook events to avoid holding stale delegates across reloads.
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

        // Tool changed: recompute whether EZ Tool Options section should be visible.
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

        // Prefab changed: recompute whether EZ Tool section should be visible.
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

        // Returns whether the EZ Tool section should be visible.
        private static bool ShouldShowFor(ToolBaseSystem? tool, PrefabBase? prefab)
        {
            try
            {
                // Show when EZ tool is active.
                if (tool is ZoningControllerToolSystem)
                    return true;

                // Otherwise show when a zonable road is selected in the Net tool.
                return IsZonableRoadPrefab(prefab);
            }
            catch
            {
                return false;
            }
        }

        // Determines whether the selected prefab supports zoning blocks and is not a highway.
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
