// File: src/Tools/ZoningControllerToolSystem.cs
// Purpose:
//   Runtime tool for updating zoning on EXISTING roads.
//   - LMB select/drag/apply
//   - RMB (Secondary Apply) cycles mode without applying
//   - Hover preview highlights only when a change would occur
//
// Notes:
//   - RMB uses secondaryApplyAction (CS2 tool action system).
//   - ESC uses Keyboard.current because vanilla Cancel is commonly bound to RMB.
//   - Updated component is only added when something actually changes (spam reduction).

namespace EasyZoning.Tools
{
    using EasyZoning.Components;     // ZoningPreviewComponent, ZoningDepthComponent
    using Game.Audio;                // ToolUXSoundSettingsData, AudioManager UI sounds
    using Game.Common;               // Updated marker (dirty flag)
    using Game.Net;                  // Layer
    using Game.Prefabs;              // PrefabBase
    using Game.Rendering;            // PhotoModeRenderSystem
    using Game.Tools;                // ToolBaseSystem, ToolSystem, RaycastHit, ToolOutputBarrier
    using Game.Zones;                // SubBlock (zone blocks buffer on road net entities)
    using System;                    // Exception (WarnOnce guard)
    using Unity.Collections;         // NativeArray, NativeList, Allocator
    using Unity.Entities;            // Entity, EntityQuery, ComponentLookup, BufferLookup, ECB
    using Unity.Jobs;                // JobHandle, IJob, IJobParallelFor
    using Unity.Mathematics;         // int2, math
    using UnityEngine.InputSystem;   // Keyboard (ESC cancel)

    public partial class ZoningControllerToolSystem : ToolBaseSystem
    {
        public override string toolID => "EasyZoning.ZoningTool";

        // Vanilla zoning depth baseline (cells). If road has no ZoningDepthComponent,
        // treat it as vanilla (6,6).
        private static readonly int2 kVanillaDepths = new int2(6, 6);

        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private ZoneControlBridgeUI m_UISystem = null!;
        private ToolHighlightSystem m_Highlight = null!;
        private PhotoModeRenderSystem m_PhotoModeSystem = null!;

        private BufferLookup<SubBlock> m_SubBlockLookup;
        private ComponentLookup<ZoningDepthComponent> m_ZoningDepthLookup;

        private EntityQuery m_ZoningPreviewQuery;
        private EntityQuery m_SoundbankQuery;

        private PrefabBase m_ToolPrefab = null!;

        // Selected/preview road entities for “drag to apply”.
        private NativeList<Entity> m_SelectedEntities;

        private enum Mode
        {
            None,
            Select,
            Apply,
            Cancel,
            Preview
        }

        private Mode m_Mode;
        private Entity m_PreviewEntity;

        // Preview stability: intersections can flicker between candidates.
        // Require the same hit for N frames before switching the highlighted preview target.
        private Entity m_PendingPreviewEntity;
        private int m_PendingPreviewFrames;
        private const int StableSwitchFrames = 2;

        // Current desired depths for the update tool (not the new-road tool).
        private int2 Depths => m_UISystem.ToolDepths;

#if DEBUG
        private static void Dbg(string msg)
        {
            try
            {
                Mod.s_Log?.Info("[EZ][Tool] " + msg);
            }
            catch { }
        }
#else
        private static void Dbg(string _)
        {
        }
#endif

        private bool IsPhotoModeEnabled( )
        {
            try
            {
                return m_PhotoModeSystem != null && m_PhotoModeSystem.Enabled;
            }
            catch
            {
                // Fail-closed for enabling decisions.
                return true;
            }
        }

        protected override void OnCreate( )
        {
            base.OnCreate();

            // Barriers/systems used every frame while tool is active.
            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_UISystem = World.GetOrCreateSystemManaged<ZoneControlBridgeUI>();
            m_Highlight = World.GetOrCreateSystemManaged<ToolHighlightSystem>();
            m_PhotoModeSystem = World.GetOrCreateSystemManaged<PhotoModeRenderSystem>();

            // Query used to clean up preview components when hover leaves.
            m_ZoningPreviewQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ZoningPreviewComponent>()
                .Build(this);

            // Sound settings (vanilla UI tool sounds).
            m_SoundbankQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ToolUXSoundSettingsData>()
                .Build(this);

            // Lookups updated per-frame in OnUpdate.
            m_SubBlockLookup = GetBufferLookup<SubBlock>(isReadOnly: true);
            m_ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(isReadOnly: true);

            m_SelectedEntities = new NativeList<Entity>(Allocator.Persistent);
        }

        protected override void OnDestroy( )
        {
            if (m_SelectedEntities.IsCreated)
                m_SelectedEntities.Dispose();

            base.OnDestroy();
        }

        protected override void OnStartRunning( )
        {
            base.OnStartRunning();

            // Tool actions:
            // - Apply = LMB (select/drag/apply)
            // - Secondary Apply = RMB (cycle mode)
            //
            // Cancel action is NOT enabled here because vanilla Cancel is commonly RMB
            // and we must keep RMB dedicated to “cycle mode”.
            applyAction.shouldBeEnabled = true;
            secondaryApplyAction.shouldBeEnabled = true;
            cancelAction.shouldBeEnabled = false;

            // Limit raycast/interaction to roads + zoning.
            requireZones = true;
            requireNet = Layer.Road;
            allowUnderground = false;

            // Contour snap option is controlled by UI toggle.
            bool contourOn = m_UISystem != null && m_UISystem.ContourEnabled;
            selectedSnap = contourOn
                ? (Snap.All | Snap.ContourLines)
                : (Snap.All & ~Snap.ContourLines);

#if DEBUG
            Dbg("OnStartRunning: tool ACTIVE");
#endif
        }

        protected override void OnStopRunning( )
        {
            applyAction.shouldBeEnabled = false;
            secondaryApplyAction.shouldBeEnabled = false;
            cancelAction.shouldBeEnabled = false;

            requireZones = false;
            requireNet = Layer.None;
            allowUnderground = false;

            // Clear all highlight state and selections.
            for (int i = 0; i < m_SelectedEntities.Length; i++)
                m_Highlight.HighlightEntity(m_SelectedEntities[i], false);

            m_SelectedEntities.Clear();

            if (m_PreviewEntity != Entity.Null)
                m_Highlight.HighlightEntity(m_PreviewEntity, false);

            m_PreviewEntity = Entity.Null;
            m_PendingPreviewEntity = Entity.Null;
            m_PendingPreviewFrames = 0;

            base.OnStopRunning();

#if DEBUG
            Dbg("OnStopRunning: tool INACTIVE");
#endif
        }

        public override void GetAvailableSnapMask(out Snap onMask, out Snap offMask)
        {
            base.GetAvailableSnapMask(out onMask, out offMask);

            // Keep contour snap state consistent with the UI toggle.
            bool contourOn = m_UISystem != null && m_UISystem.ContourEnabled;

            if (contourOn)
            {
                onMask |= Snap.ContourLines;
                offMask &= ~Snap.ContourLines;
            }
            else
            {
                onMask &= ~Snap.ContourLines;
                offMask |= Snap.ContourLines;
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = Dependency;

            // Update lookups used by filtering and apply logic.
            m_SubBlockLookup.Update(this);
            m_ZoningDepthLookup.Update(this);

            // Hit-test + filter: only “true” when hit is a road and it would actually change.
            bool hasRoad = TryGetRoadUnderCursor(out Entity hitEntity, out RaycastHit _);

            // Load vanilla soundbank (if present).
            bool haveSoundbank = m_SoundbankQuery.CalculateEntityCount() > 0;
            ToolUXSoundSettingsData soundbank = default;
            if (haveSoundbank)
                soundbank = m_SoundbankQuery.GetSingleton<ToolUXSoundSettingsData>();

            // RMB cycle: use CS2 tool system (Secondary Apply).
            // This is Phase-2 migration away from Mouse.current polling.
            bool cyclePressed = false;
            try
            {
                cyclePressed = secondaryApplyAction.WasPressedThisFrame();
            }
            catch (Exception ex)
            {
#if DEBUG
                Mod.WarnOnce(
                    "SecondaryApplyAction.ReadFailed",
                    ( ) => $"[EZ] secondaryApplyAction read failed: {ex.GetType().Name}: {ex.Message}");
#else
                _ = ex; // silence "unused variable" in Release builds
#endif
            }

            if (cyclePressed)
            {
                m_UISystem.CycleMode();
                if (haveSoundbank)
                    AudioManager.instance.PlayUISound(soundbank.m_SnapSound);
            }

            // NOTE: Escape is read via Keyboard.current because the tool's Cancel action is often bound to RMB in vanilla.
            // Keeping Escape explicit prevents RMB-cycle from accidental cancel trigger and stays dedicated to 4-cycle.
            bool escapePressed = false;
            try
            {
                Keyboard kb = Keyboard.current;
                if (kb != null && kb.escapeKey.wasPressedThisFrame)
                    escapePressed = true;
            }
            catch { }

            // Determine tool state for this frame.
            if (escapePressed && (m_SelectedEntities.Length > 0 || m_PreviewEntity != Entity.Null))
                m_Mode = Mode.Cancel;
            else if (applyAction.WasPressedThisFrame() || applyAction.IsPressed())
                m_Mode = Mode.Select;
            else if (applyAction.WasReleasedThisFrame() && hasRoad)
                m_Mode = Mode.Apply;
            else if (applyAction.WasReleasedThisFrame() && !hasRoad)
                m_Mode = Mode.Cancel;
            else
                m_Mode = Mode.Preview;

            EntityCommandBuffer ecb = m_ToolOutputBarrier.CreateCommandBuffer();

            switch (m_Mode)
            {
                case Mode.Preview:
                    UpdatePreviewSelection(hasRoad, hitEntity);
                    break;

                case Mode.Select when hasRoad:
                    // Dragging selects multiple segments.
                    if (!m_SelectedEntities.Contains(hitEntity))
                    {
                        m_SelectedEntities.Add(hitEntity);
                        m_Highlight.HighlightEntity(hitEntity, true);

                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_SelectEntitySound);
                    }
                    break;

                case Mode.Cancel:
                    {
                        // Cancel clears any selection + preview highlight.
                        for (int i = 0; i < m_SelectedEntities.Length; i++)
                            m_Highlight.HighlightEntity(m_SelectedEntities[i], false);

                        m_SelectedEntities.Clear();

                        if (m_PreviewEntity != Entity.Null)
                            m_Highlight.HighlightEntity(m_PreviewEntity, false);

                        m_PreviewEntity = Entity.Null;
                        m_PendingPreviewEntity = Entity.Null;
                        m_PendingPreviewFrames = 0;

                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_NetCancelSound);

                        break;
                    }

                case Mode.Apply:
                    {
                        // Apply commits ToolDepths to the selected road entities.
                        ComponentLookup<ZoningPreviewComponent> previewLookup =
                            GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true);
                        ComponentLookup<ZoningDepthComponent> depthLookup =
                            GetComponentLookup<ZoningDepthComponent>(isReadOnly: true);
                        ComponentLookup<Updated> updatedLookup =
                            GetComponentLookup<Updated>(isReadOnly: true);

                        JobHandle setJob = new SetZoningDepthJob
                        {
                            Entities = m_SelectedEntities.AsArray().AsReadOnly(),
                            ZoningPreviewLookup = previewLookup,
                            DepthLookup = depthLookup,
                            UpdatedLookup = updatedLookup,
                            ToolDepths = Depths,
                            ECB = ecb
                        }.Schedule(inputDeps);

                        inputDeps = JobHandle.CombineDependencies(inputDeps, setJob);

                        // Clear selection highlight immediately (visual feedback).
                        for (int i = 0; i < m_SelectedEntities.Length; i++)
                            m_Highlight.HighlightEntity(m_SelectedEntities[i], false);

                        m_SelectedEntities.Clear();
                        m_PreviewEntity = Entity.Null;
                        m_PendingPreviewEntity = Entity.Null;
                        m_PendingPreviewFrames = 0;

                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_NetBuildSound);

                        break;
                    }
            }

            // Preview overlay sync:
            // - Add/Update ZoningPreviewComponent for currently selected entities
            // - Remove ZoningPreviewComponent from entities no longer selected
            //
            // Updated is only added when the preview data actually changes (spam reduction).
            ComponentLookup<ZoningPreviewComponent> previewReadLookup =
                GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true);
            ComponentLookup<Updated> updatedReadLookup2 =
                GetComponentLookup<Updated>(isReadOnly: true);

            JobHandle syncTempJob = new SyncTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                ZoningPreviewLookup = previewReadLookup,
                UpdatedLookup = updatedReadLookup2,
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                ToolDepths = Depths
            }.Schedule(m_SelectedEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, syncTempJob);

            NativeArray<Entity> zoningPreviewEntities =
                m_ZoningPreviewQuery.ToEntityArray(Allocator.TempJob);

            ComponentLookup<Updated> updatedReadLookup3 =
                GetComponentLookup<Updated>(isReadOnly: true);

            JobHandle cleanupTempJob = new CleanupTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                UpdatedLookup = updatedReadLookup3,
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                Entities = zoningPreviewEntities.AsReadOnly()
            }.Schedule(zoningPreviewEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, cleanupTempJob);
            zoningPreviewEntities.Dispose(inputDeps);

            m_ToolOutputBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }

        private void UpdatePreviewSelection(bool hasRoad, Entity hitEntity)
        {
            // No hit → clear selection and preview so the tool “lets go” naturally.
            if (!hasRoad)
            {
                for (int i = 0; i < m_SelectedEntities.Length; i++)
                    m_Highlight.HighlightEntity(m_SelectedEntities[i], false);

                m_SelectedEntities.Clear();

                if (m_PreviewEntity != Entity.Null)
                    m_Highlight.HighlightEntity(m_PreviewEntity, false);

                m_PreviewEntity = Entity.Null;
                m_PendingPreviewEntity = Entity.Null;
                m_PendingPreviewFrames = 0;
                return;
            }

            // Same as current preview → stable.
            if (hitEntity == m_PreviewEntity)
            {
                m_PendingPreviewEntity = Entity.Null;
                m_PendingPreviewFrames = 0;
                return;
            }

            // Stability gating to avoid intersection flicker.
            if (hitEntity == m_PendingPreviewEntity)
            {
                m_PendingPreviewFrames++;
            }
            else
            {
                m_PendingPreviewEntity = hitEntity;
                m_PendingPreviewFrames = 1;
            }

            if (m_PendingPreviewFrames < StableSwitchFrames)
                return;

            // Switch preview target.
            for (int i = 0; i < m_SelectedEntities.Length; i++)
                m_Highlight.HighlightEntity(m_SelectedEntities[i], false);

            m_SelectedEntities.Clear();
            m_PreviewEntity = Entity.Null;

            m_Highlight.HighlightEntity(hitEntity, true);
            m_SelectedEntities.Add(hitEntity);
            m_PreviewEntity = hitEntity;

            m_PendingPreviewEntity = Entity.Null;
            m_PendingPreviewFrames = 0;
        }

        // Filtered raycast:
        // - Must be a road segment (has SubBlock buffer)
        // - Must represent a change (WouldChange) so the tool only highlights actionable segments
        private bool TryGetRoadUnderCursor(out Entity entity, out RaycastHit hit)
        {
            if (!base.GetRaycastResult(out entity, out hit))
                return false;

            if (!m_SubBlockLookup.TryGetBuffer(entity, out _))
            {
                entity = Entity.Null;
                return false;
            }

            if (!WouldChange(entity))
            {
                entity = Entity.Null;
                return false;
            }

            return true;
        }

        // Decide if applying the current tool depth would change this road.
        private bool WouldChange(Entity entity)
        {
            int2 desired = Depths;

            int2 current;
            if (m_ZoningDepthLookup.TryGetComponent(entity, out ZoningDepthComponent depth))
                current = depth.Depths;
            else
                current = kVanillaDepths;

            return math.any(desired != current);
        }

        public override PrefabBase GetPrefab( ) => m_ToolPrefab;

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            if (prefab == null || prefab.name != toolID)
                return false;

            m_ToolPrefab = prefab;
            return true;
        }

        public override void InitializeRaycast( )
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.typeMask = TypeMask.Net;
            m_ToolRaycastSystem.netLayerMask = Layer.Road;
        }

        // Called by GTL UI button or Hot Keybind.
        // Enabling is blocked during Photo Mode; disabling is always allowed.
        public void SetToolEnabled(bool isEnabled)
        {
            if (m_ToolSystem == null)
                return;

            if (isEnabled)
            {
                if (IsPhotoModeEnabled())
                {
#if DEBUG
            Dbg("SetToolEnabled(true) blocked (PhotoMode).");
#endif
                    return;
                }

                if (m_ToolSystem.activeTool != this)
                    m_ToolSystem.activeTool = this;
            }
            else
            {
                if (m_ToolSystem.activeTool == this)
                    m_ToolSystem.activeTool = World.GetOrCreateSystemManaged<DefaultToolSystem>();
            }
        }


        // Keep preview component in sync for selected entities.
        // Updated is only added when preview changes or is newly added.
        public struct SyncTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            [ReadOnly] public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            [ReadOnly] public ComponentLookup<Updated> UpdatedLookup;

            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public int2 ToolDepths;

            public void Execute(int index)
            {
                Entity e = SelectedEntities[index];
                int2 preview = ToolDepths;

                if (ZoningPreviewLookup.TryGetComponent(e, out ZoningPreviewComponent data))
                {
                    if (!math.all(data.Depths == preview))
                    {
                        ECB.SetComponent(index, e, new ZoningPreviewComponent { Depths = preview });

                        if (!UpdatedLookup.HasComponent(e))
                            ECB.AddComponent<Updated>(index, e);
                    }
                }
                else
                {
                    ECB.AddComponent(index, e, new ZoningPreviewComponent { Depths = preview });

                    if (!UpdatedLookup.HasComponent(e))
                        ECB.AddComponent<Updated>(index, e);
                }
            }
        }

        // Remove preview components from entities not currently selected.
        public struct CleanupTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            [ReadOnly] public ComponentLookup<Updated> UpdatedLookup;

            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public NativeArray<Entity>.ReadOnly Entities;

            public void Execute(int index)
            {
                Entity e = Entities[index];
                if (SelectedEntities.Contains(e))
                    return;

                ECB.RemoveComponent<ZoningPreviewComponent>(index, e);

                if (!UpdatedLookup.HasComponent(e))
                    ECB.AddComponent<Updated>(index, e);
            }
        }

        // Apply commits the chosen depths to roads.
        // Updated is only added if it isn't already present.
        public struct SetZoningDepthJob : IJob
        {
            public NativeArray<Entity>.ReadOnly Entities;

            [ReadOnly] public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            [ReadOnly] public ComponentLookup<ZoningDepthComponent> DepthLookup;
            [ReadOnly] public ComponentLookup<Updated> UpdatedLookup;

            public int2 ToolDepths;
            public EntityCommandBuffer ECB;

            public void Execute( )
            {
                foreach (Entity e in Entities)
                {
                    if (ZoningPreviewLookup.HasComponent(e))
                        ECB.RemoveComponent<ZoningPreviewComponent>(e);

                    if (DepthLookup.HasComponent(e))
                        ECB.SetComponent(e, new ZoningDepthComponent { Depths = ToolDepths });
                    else
                        ECB.AddComponent(e, new ZoningDepthComponent { Depths = ToolDepths });

                    if (!UpdatedLookup.HasComponent(e))
                        ECB.AddComponent<Updated>(e);
                }
            }
        }
    }
}
