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
    using Building = Game.Buildings.Building;
    using EasyZoning.Components;     // ZoningPreviewComponent, ZoningDepthComponent, ZoningRestoreComponent
    using Game.Audio;                // ToolUXSoundSettingsData, AudioManager UI sounds
    using Game.Common;               // Updated marker (dirty flag)
    using Game.Net;                  // Curve, Layer, Upgraded
    using Game.Prefabs;              // BuildingData, PrefabBase, PrefabRef, SpawnableBuildingData
    using Game.Rendering;            // PhotoModeRenderSystem
    using Game.Tools;                // ToolBaseSystem, ToolSystem, RaycastHit, ToolOutputBarrier
    using Game.Zones;                // Block, Cell, SubBlock, ValidArea, ZoneType
    using ObjectTransform = Game.Objects.Transform;
    using System;                    // Exception (WarnOnce guard)
    using Unity.Collections;         // NativeArray, NativeList, Allocator
    using Unity.Collections.LowLevel.Unsafe; // Allows preview highlight writes by road-side job
    using Unity.Entities;            // Entity, EntityQuery, ComponentLookup, BufferLookup, ECB
    using Unity.Jobs;                // JobHandle, IJob, IJobParallelFor
    using Unity.Mathematics;         // int2, math
    using UnityEngine.InputSystem;   // Keyboard (ESC cancel)

    public partial class ZoningControllerToolSystem : ToolBaseSystem
    {
        public override string toolID => "EasyZoning.ZoningTool";

        // Vanilla zoning depth baseline (cells). If road has no ZoningDepthComponent,
        // treat it as vanilla (6,6).
        private static readonly int2 kVanillaDepths = RoadZoneCompatibility.VanillaDepths;

        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private ZoneControlBridgeUI m_UISystem = null!;
        private ToolHighlightSystem m_Highlight = null!;
        private PhotoModeRenderSystem m_PhotoModeSystem = null!;

        private BufferLookup<SubBlock> m_SubBlockLookup;
        private BufferLookup<Cell> m_CellLookup;
        private ComponentLookup<Block> m_BlockLookup;
        private ComponentLookup<Curve> m_CurveLookup;
        private ComponentLookup<Upgraded> m_UpgradedLookup;
        private ComponentLookup<ValidArea> m_ValidAreaLookup;
        private ComponentLookup<ZoningDepthComponent> m_ZoningDepthLookup;
        private ComponentLookup<ZoningPreviewComponent> m_ZoningPreviewLookup;
        private ComponentLookup<ZoningRestoreComponent> m_ZoningRestoreLookup;
        private ComponentLookup<ObjectTransform> m_TransformLookup;
        private ComponentLookup<PrefabRef> m_PrefabRefLookup;
        private ComponentLookup<BuildingData> m_BuildingDataLookup;
        private ComponentLookup<SpawnableBuildingData> m_SpawnableBuildingLookup;
        private ComponentLookup<SignatureBuildingData> m_SignatureBuildingLookup;
        private ComponentLookup<ZoneData> m_ZoneDataLookup;

        private EntityQuery m_GrowableBuildingQuery;
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
        private Entity m_VanillaRemovalPreviewEntity;
        private Entity m_VanillaRemovalPreviewRoad;

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

            // Real growable buildings, used by the "Prevent buildings" option.
            // CellFlags.Occupied is too broad because CS2 can also set it for
            // height/overlap blocked painted cells that are not actual buildings.
            m_GrowableBuildingQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Building, ObjectTransform, PrefabRef>()
                .WithNone<Temp, Deleted, Destroyed>()
                .Build(this);

            // Lookups updated per-frame in OnUpdate.
            m_SubBlockLookup = GetBufferLookup<SubBlock>(isReadOnly: true);
            m_CellLookup = GetBufferLookup<Cell>(isReadOnly: true);
            m_BlockLookup = GetComponentLookup<Block>(isReadOnly: true);
            m_CurveLookup = GetComponentLookup<Curve>(isReadOnly: true);
            m_UpgradedLookup = GetComponentLookup<Upgraded>(isReadOnly: true);
            m_ValidAreaLookup = GetComponentLookup<ValidArea>(isReadOnly: true);
            m_ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(isReadOnly: true);
            m_ZoningPreviewLookup = GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true);
            m_ZoningRestoreLookup = GetComponentLookup<ZoningRestoreComponent>(isReadOnly: true);
            m_TransformLookup = GetComponentLookup<ObjectTransform>(isReadOnly: true);
            m_PrefabRefLookup = GetComponentLookup<PrefabRef>(isReadOnly: true);
            m_BuildingDataLookup = GetComponentLookup<BuildingData>(isReadOnly: true);
            m_SpawnableBuildingLookup = GetComponentLookup<SpawnableBuildingData>(isReadOnly: true);
            m_SignatureBuildingLookup = GetComponentLookup<SignatureBuildingData>(isReadOnly: true);
            m_ZoneDataLookup = GetComponentLookup<ZoneData>(isReadOnly: true);

            m_SelectedEntities = new NativeList<Entity>(Allocator.Persistent);
            m_VanillaRemovalPreviewEntity = Entity.Null;
            m_VanillaRemovalPreviewRoad = Entity.Null;
        }

        protected override void OnDestroy( )
        {
            ClearVanillaRemovalPreviewTemp();

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

            ClearTransientPreviewState();

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
            m_CellLookup.Update(this);
            m_BlockLookup.Update(this);
            m_CurveLookup.Update(this);
            m_UpgradedLookup.Update(this);
            m_ValidAreaLookup.Update(this);
            m_ZoningDepthLookup.Update(this);
            m_ZoningPreviewLookup.Update(this);
            m_ZoningRestoreLookup.Update(this);
            m_TransformLookup.Update(this);
            m_PrefabRefLookup.Update(this);
            m_BuildingDataLookup.Update(this);
            m_SpawnableBuildingLookup.Update(this);
            m_SignatureBuildingLookup.Update(this);
            m_ZoneDataLookup.Update(this);

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
            }

            // Hit-test + filter: only “true” when hit is a road and it would actually change.
            // This must run after RMB cycling so preview responds to the newly selected mode
            // immediately instead of evaluating the hover against stale tool depths.
            bool hasRoad = TryGetRoadUnderCursor(out Entity hitEntity, out RaycastHit _);

            // Load vanilla soundbank (if present).
            bool haveSoundbank = m_SoundbankQuery.CalculateEntityCount() > 0;
            ToolUXSoundSettingsData soundbank = default;
            if (haveSoundbank)
                soundbank = m_SoundbankQuery.GetSingleton<ToolUXSoundSettingsData>();

            bool protectOccupiedCells = Mod.Settings?.RemoveOccupiedCells ?? true;
            bool protectZonedCells = Mod.Settings?.RemoveZonedCells ?? true;

            if (cyclePressed)
            {
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

            bool shouldApply = false;
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
                        // Clear selection highlight immediately (visual feedback).
                        for (int i = 0; i < m_SelectedEntities.Length; i++)
                            m_Highlight.HighlightEntity(m_SelectedEntities[i], false);

                        if (m_PreviewEntity != Entity.Null)
                            m_Highlight.HighlightEntity(m_PreviewEntity, false);

                        shouldApply = true;

                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_NetBuildSound);

                        break;
                    }
            }

            NativeArray<Entity> selectedEntities = new NativeArray<Entity>(m_SelectedEntities.Length, Allocator.TempJob);
            if (m_SelectedEntities.Length > 0)
                NativeArray<Entity>.Copy(m_SelectedEntities.AsArray(), selectedEntities, m_SelectedEntities.Length);

            NativeArray<Entity>.ReadOnly selectedReadOnly = selectedEntities.AsReadOnly();
            NativeArray<int2> selectedDepths = BuildDesiredDepths(selectedEntities, protectOccupiedCells, protectZonedCells);
            NativeArray<int2>.ReadOnly selectedDepthsReadOnly = selectedDepths.AsReadOnly();
            NativeArray<Entity> syncSelectedEntities = shouldApply
                ? new NativeArray<Entity>(0, Allocator.TempJob)
                : selectedEntities;
            NativeArray<Entity>.ReadOnly syncSelectedReadOnly = syncSelectedEntities.AsReadOnly();

            bool wantsVanillaRemovalPreview =
                m_Mode == Mode.Preview &&
                !shouldApply &&
                selectedEntities.Length == 1 &&
                WouldPreviewRemoveCommittedSide(selectedEntities[0], selectedDepths[0]);

            if (wantsVanillaRemovalPreview || m_VanillaRemovalPreviewEntity != Entity.Null)
            {
                inputDeps.Complete();
                inputDeps = default;

                if (wantsVanillaRemovalPreview)
                    SyncVanillaRemovalPreviewTemp(selectedEntities[0], selectedDepths[0]);
                else
                    ClearVanillaRemovalPreviewTemp();
            }

            if (shouldApply)
            {
                ComponentLookup<ZoningPreviewComponent> previewLookup =
                    GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true);
                ComponentLookup<ZoningDepthComponent> depthLookup =
                    GetComponentLookup<ZoningDepthComponent>(isReadOnly: true);
                ComponentLookup<ZoningRestoreComponent> restoreLookup =
                    GetComponentLookup<ZoningRestoreComponent>(isReadOnly: true);
                ComponentLookup<Updated> updatedLookup =
                    GetComponentLookup<Updated>(isReadOnly: true);

                JobHandle setJob = new SetZoningDepthJob
                {
                    Entities = selectedReadOnly,
                    SubBlockLookup = GetBufferLookup<SubBlock>(isReadOnly: true),
                    CellLookup = GetBufferLookup<Cell>(isReadOnly: true),
                    BlockLookup = GetComponentLookup<Block>(isReadOnly: true),
                    CurveLookup = GetComponentLookup<Curve>(isReadOnly: true),
                    ValidAreaLookup = GetComponentLookup<ValidArea>(isReadOnly: true),
                    ZoningPreviewLookup = previewLookup,
                    ZoningRestoreLookup = restoreLookup,
                    DepthLookup = depthLookup,
                    UpgradedLookup = GetComponentLookup<Upgraded>(isReadOnly: true),
                    UpdatedLookup = updatedLookup,
                    ToolDepths = Depths,
                    DesiredDepths = selectedDepthsReadOnly,
                    ProtectOccupiedCells = false,
                    ProtectZonedCells = protectZonedCells,
                    ECB = ecb
                }.Schedule(inputDeps);

                inputDeps = JobHandle.CombineDependencies(inputDeps, setJob);
            }

            // Preview selection sync:
            // - Add/Update ZoningPreviewComponent for currently selected entities
            // - Remove ZoningPreviewComponent from entities no longer selected
            //
            // Important: hover preview only writes the "expanded" live road state so
            // newly enabled sides can render. Remove-side preview is routed through a
            // separate temp vanilla highlight entity.
            ComponentLookup<ZoningPreviewComponent> previewReadLookup =
                GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true);

            JobHandle syncTempJob = new SyncTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                CellLookup = GetBufferLookup<Cell>(isReadOnly: true),
                BlockLookup = GetComponentLookup<Block>(isReadOnly: true),
                CurveLookup = GetComponentLookup<Curve>(isReadOnly: true),
                ValidAreaLookup = GetComponentLookup<ValidArea>(isReadOnly: true),
                ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(isReadOnly: true),
                ZoningPreviewLookup = previewReadLookup,
                ZoningRestoreLookup = GetComponentLookup<ZoningRestoreComponent>(isReadOnly: true),
                UpgradedLookup = GetComponentLookup<Upgraded>(isReadOnly: true),
                SubBlockLookup = GetBufferLookup<SubBlock>(isReadOnly: true),
                UpdatedLookup = GetComponentLookup<Updated>(isReadOnly: true),
                SelectedEntities = syncSelectedReadOnly,
                ToolDepths = Depths,
                DesiredDepths = selectedDepthsReadOnly,
                ProtectOccupiedCells = false,
                ProtectZonedCells = protectZonedCells
            }.Schedule(syncSelectedEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, syncTempJob);

            NativeArray<Entity> zoningPreviewEntities =
                m_ZoningPreviewQuery.ToEntityArray(Allocator.TempJob);

            JobHandle cleanupTempJob = new CleanupTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                ZoningPreviewLookup = GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true),
                ZoningRestoreLookup = GetComponentLookup<ZoningRestoreComponent>(isReadOnly: true),
                SubBlockLookup = GetBufferLookup<SubBlock>(isReadOnly: true),
                UpgradedLookup = GetComponentLookup<Upgraded>(isReadOnly: true),
                UpdatedLookup = GetComponentLookup<Updated>(isReadOnly: true),
                SelectedEntities = selectedReadOnly,
                Entities = zoningPreviewEntities.AsReadOnly()
            }.Schedule(zoningPreviewEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, cleanupTempJob);
            zoningPreviewEntities.Dispose(inputDeps);
            selectedEntities.Dispose(inputDeps);
            selectedDepths.Dispose(inputDeps);
            if (shouldApply)
                syncSelectedEntities.Dispose(inputDeps);

            if (shouldApply)
            {
                m_SelectedEntities.Clear();
                m_PreviewEntity = Entity.Null;
                m_PendingPreviewEntity = Entity.Null;
                m_PendingPreviewFrames = 0;
            }

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
            int2 current = GetCommittedRoadDepths(entity);
            int2 desired = ConstrainDepthsForProtectedCells(
                entity,
                current,
                Depths,
                Mod.Settings?.RemoveOccupiedCells ?? true,
                Mod.Settings?.RemoveZonedCells ?? true);
            return math.any(desired != current);
        }

        private NativeArray<int2> BuildDesiredDepths(
            NativeArray<Entity> entities,
            bool protectOccupiedCells,
            bool protectZonedCells)
        {
            NativeArray<int2> desiredDepths = new NativeArray<int2>(entities.Length, Allocator.TempJob);
            for (int i = 0; i < entities.Length; i++)
            {
                Entity entity = entities[i];
                int2 current = GetCommittedRoadDepths(entity);
                desiredDepths[i] = ConstrainDepthsForProtectedCells(
                    entity,
                    current,
                    Depths,
                    protectOccupiedCells,
                    protectZonedCells);
            }

            return desiredDepths;
        }

        private int2 ConstrainDepthsForProtectedCells(
            Entity roadEntity,
            int2 current,
            int2 desired,
            bool protectOccupiedCells,
            bool protectZonedCells)
        {
            if (!protectOccupiedCells && !protectZonedCells)
                return desired;

            if (desired.x < current.x &&
                HasProtectedCellsOnSide(roadEntity, leftSide: true, protectOccupiedCells, protectZonedCells))
            {
                desired.x = current.x;
            }

            if (desired.y < current.y &&
                HasProtectedCellsOnSide(roadEntity, leftSide: false, protectOccupiedCells, protectZonedCells))
            {
                desired.y = current.y;
            }

            return desired;
        }

        private bool HasProtectedCellsOnSide(
            Entity roadEntity,
            bool leftSide,
            bool protectOccupiedCells,
            bool protectZonedCells)
        {
            if (protectOccupiedCells && HasGrowableBuildingOnSide(roadEntity, leftSide))
                return true;

            if (!protectZonedCells)
                return false;

            if (roadEntity == Entity.Null ||
                !m_CurveLookup.TryGetComponent(roadEntity, out Curve curve) ||
                !m_SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
            {
                return false;
            }

            for (int i = 0; i < subBlocks.Length; i++)
            {
                Entity blockEntity = subBlocks[i].m_SubBlock;
                if (!m_BlockLookup.TryGetComponent(blockEntity, out Block block) ||
                    !m_ValidAreaLookup.TryGetComponent(blockEntity, out ValidArea validArea) ||
                    !m_CellLookup.TryGetBuffer(blockEntity, out DynamicBuffer<Cell> cells))
                {
                    continue;
                }

                if (RoadZoneCompatibility.IsBlockOnLeft(block, curve) != leftSide)
                    continue;

                if (HasProtectedCells(cells, block, validArea, protectOccupiedCells: false, protectZonedCells: true))
                    return true;
            }

            return false;
        }

        private bool HasGrowableBuildingOnSide(Entity roadEntity, bool leftSide)
        {
            if (roadEntity == Entity.Null ||
                !m_CurveLookup.TryGetComponent(roadEntity, out Curve curve) ||
                !m_SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
            {
                return false;
            }

            NativeArray<Entity> buildings = m_GrowableBuildingQuery.ToEntityArray(Allocator.Temp);
            try
            {
                for (int i = 0; i < subBlocks.Length; i++)
                {
                    Entity blockEntity = subBlocks[i].m_SubBlock;
                    if (!m_BlockLookup.TryGetComponent(blockEntity, out Block block) ||
                        !m_ValidAreaLookup.TryGetComponent(blockEntity, out ValidArea validArea) ||
                        !m_CellLookup.TryGetBuffer(blockEntity, out DynamicBuffer<Cell> cells) ||
                        RoadZoneCompatibility.IsBlockOnLeft(block, curve) != leftSide)
                    {
                        continue;
                    }

                    for (int j = 0; j < buildings.Length; j++)
                    {
                        if (GrowableBuildingLotUsesBlockCells(buildings[j], block, validArea, cells))
                            return true;
                    }
                }
            }
            finally
            {
                buildings.Dispose();
            }

            return false;
        }

        private bool GrowableBuildingLotUsesBlockCells(
            Entity buildingEntity,
            Block block,
            ValidArea validArea,
            DynamicBuffer<Cell> cells)
        {
            if (!m_TransformLookup.TryGetComponent(buildingEntity, out ObjectTransform transform) ||
                !m_PrefabRefLookup.TryGetComponent(buildingEntity, out PrefabRef prefabRef) ||
                m_SignatureBuildingLookup.HasComponent(prefabRef.m_Prefab) ||
                !m_SpawnableBuildingLookup.TryGetComponent(prefabRef.m_Prefab, out SpawnableBuildingData spawnableBuildingData) ||
                !m_ZoneDataLookup.TryGetComponent(spawnableBuildingData.m_ZonePrefab, out ZoneData zoneData) ||
                !m_BuildingDataLookup.TryGetComponent(prefabRef.m_Prefab, out BuildingData buildingData))
            {
                return false;
            }

            if (zoneData.m_ZoneType.Equals(ZoneType.None))
                return false;

            int2 lotSize = buildingData.m_LotSize;
            if (lotSize.x <= 0 || lotSize.y <= 0)
                return false;

            float2 right = math.rotate(transform.m_Rotation, new float3(8f, 0f, 0f)).xz;
            float2 forward = math.rotate(transform.m_Rotation, new float3(0f, 0f, 8f)).xz;
            float2 rightOffset = right * ((float)lotSize.x * 0.5f - 0.5f);
            float2 forwardOffset = forward * ((float)lotSize.y * 0.5f - 0.5f);
            float2 rowStart = transform.m_Position.xz + forwardOffset + rightOffset;

            int2 min = validArea.m_Area.xz;
            int2 max = validArea.m_Area.yw;

            for (int z = 0; z < lotSize.y; z++)
            {
                float2 position = rowStart;
                for (int x = 0; x < lotSize.x; x++)
                {
                    int2 cellIndex = ZoneUtils.GetCellIndex(block, position);
                    if (math.all((cellIndex >= min) & (cellIndex < max)))
                    {
                        int index = cellIndex.y * block.m_Size.x + cellIndex.x;
                        if ((uint) index < (uint) cells.Length)
                        {
                            Cell cell = cells[index];
                            if ((cell.m_State & CellFlags.Visible) != CellFlags.None &&
                                cell.m_Zone.Equals(zoneData.m_ZoneType))
                            {
                                return true;
                            }
                        }
                    }

                    position -= right;
                }

                rowStart -= forward;
            }

            return false;
        }

        private int2 GetCommittedRoadDepths(Entity roadEntity)
        {
            if (roadEntity == Entity.Null)
                return kVanillaDepths;

            if (m_ZoningPreviewLookup.TryGetComponent(roadEntity, out ZoningPreviewComponent preview))
                return preview.CommittedDepths;

            if (m_ZoningRestoreLookup.TryGetComponent(roadEntity, out ZoningRestoreComponent restore))
                return restore.Depths;

            if (m_UpgradedLookup.TryGetComponent(roadEntity, out Upgraded upgraded) &&
                RoadZoneCompatibility.TryGetDepthsFromFlags(upgraded.m_Flags, out int2 flaggedDepths))
            {
                return flaggedDepths;
            }

            if (TryGetDepthsFromBlockLayout(roadEntity, out int2 blockDepths))
                return blockDepths;

            if (m_ZoningDepthLookup.TryGetComponent(roadEntity, out ZoningDepthComponent depth))
                return depth.Depths;

            return kVanillaDepths;
        }

        private bool TryGetDepthsFromBlockLayout(Entity roadEntity, out int2 depths)
        {
            depths = kVanillaDepths;

            if (!m_CurveLookup.TryGetComponent(roadEntity, out Curve curve) ||
                !m_SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
            {
                return false;
            }

            bool sawLeft = false;
            bool sawRight = false;
            bool leftEnabled = false;
            bool leftDisabled = false;
            bool rightEnabled = false;
            bool rightDisabled = false;

            for (int i = 0; i < subBlocks.Length; i++)
            {
                Entity blockEntity = subBlocks[i].m_SubBlock;
                if (!m_BlockLookup.TryGetComponent(blockEntity, out Block block) ||
                    !m_ValidAreaLookup.TryGetComponent(blockEntity, out ValidArea validArea))
                {
                    continue;
                }

                bool enabled = block.m_Size.y > 0 && validArea.m_Area.w > 0;
                bool left = RoadZoneCompatibility.IsBlockOnLeft(block, curve);

                if (left)
                {
                    sawLeft = true;
                    leftEnabled |= enabled;
                    leftDisabled |= !enabled;
                }
                else
                {
                    sawRight = true;
                    rightEnabled |= enabled;
                    rightDisabled |= !enabled;
                }
            }

            if (!sawLeft || !sawRight ||
                (leftEnabled && leftDisabled) ||
                (rightEnabled && rightDisabled))
            {
                return false;
            }

            depths = RoadZoneCompatibility.DepthsFromDisabledSides(leftDisabled, rightDisabled);
            return true;
        }

        private static bool HasProtectedCells(
            DynamicBuffer<Cell> cells,
            Block block,
            ValidArea validArea,
            bool protectOccupiedCells,
            bool protectZonedCells)
        {
            int x0 = validArea.m_Area.x;
            int x1 = validArea.m_Area.y;
            int z0 = validArea.m_Area.z;
            int z1 = validArea.m_Area.w;

            if (x1 <= x0 || z1 <= z0)
                return false;

            x0 = math.clamp(x0, 0, block.m_Size.x);
            x1 = math.clamp(x1, 0, block.m_Size.x);
            z0 = math.clamp(z0, 0, block.m_Size.y);
            z1 = math.clamp(z1, 0, block.m_Size.y);

            if (x1 <= x0 || z1 <= z0)
                return false;

            int stride = block.m_Size.x;
            for (int z = z0; z < z1; z++)
            {
                int row = z * stride;
                for (int x = x0; x < x1; x++)
                {
                    int idx = row + x;
                    if ((uint) idx >= (uint) cells.Length)
                        continue;

                    Cell cell = cells[idx];
                    if (protectOccupiedCells && IsBuildingOccupiedCell(cell))
                        return true;

                    if (protectZonedCells && cell.m_Zone.m_Index != ZoneType.None.m_Index)
                        return true;
                }
            }

            return false;
        }

        private static bool IsBuildingOccupiedCell(Cell cell)
        {
            // CS2 also marks cells as Occupied when height/overlap rules make a painted
            // zone square unusable. Those are not buildings, so do not let the building
            // protection toggle block players from clearing those painted cells.
            return (cell.m_State & CellFlags.Occupied) != 0 && cell.m_Height == short.MaxValue;
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


        // Keep preview metadata in sync for selected entities.
        // Preview only enables added sides on the live road so white cells can render.
        // Side removals are previewed separately through a temp vanilla highlight entity.
        public struct SyncTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            [ReadOnly] public BufferLookup<Cell> CellLookup;
            [ReadOnly] public ComponentLookup<Block> BlockLookup;
            [ReadOnly] public ComponentLookup<Curve> CurveLookup;
            [ReadOnly] public ComponentLookup<ValidArea> ValidAreaLookup;
            [ReadOnly] public ComponentLookup<ZoningDepthComponent> ZoningDepthLookup;
            [ReadOnly] public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            [ReadOnly] public ComponentLookup<ZoningRestoreComponent> ZoningRestoreLookup;
            [ReadOnly] public ComponentLookup<Upgraded> UpgradedLookup;
            [ReadOnly] public BufferLookup<SubBlock> SubBlockLookup;
            [ReadOnly] public ComponentLookup<Updated> UpdatedLookup;

            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public int2 ToolDepths;
            public NativeArray<int2>.ReadOnly DesiredDepths;
            public bool ProtectOccupiedCells;
            public bool ProtectZonedCells;

            public void Execute(int index)
            {
                Entity e = SelectedEntities[index];
                bool hasPreview = ZoningPreviewLookup.TryGetComponent(e, out ZoningPreviewComponent data);
                int2 current = hasPreview ? data.CommittedDepths : GetCommittedRoadDepths(e);
                int2 requested = index < DesiredDepths.Length ? DesiredDepths[index] : ToolDepths;
                int2 preview = ConstrainDepthsForProtectedCells(e, current, requested);
                bool changed = false;

                if (hasPreview)
                {
                    if (!math.all(data.Depths == preview))
                    {
                        data = new ZoningPreviewComponent
                        {
                            Depths = preview,
                            CommittedDepths = data.CommittedDepths,
                            CommittedFlags = data.CommittedFlags,
                            HasCommittedUpgraded = data.HasCommittedUpgraded
                        };
                        ECB.SetComponent(index, e, data);
                        changed = true;
                    }

                    changed |= ApplyPreviewRoadUpgradedState(index, e, data);
                }
                else
                {
                    ZoningPreviewComponent previewData = CreatePreviewData(e, preview);
                    ECB.AddComponent(index, e, previewData);
                    changed = true;

                    changed |= ApplyPreviewRoadUpgradedState(index, e, previewData);
                }

                if (ZoningRestoreLookup.HasComponent(e))
                {
                    ECB.RemoveComponent<ZoningRestoreComponent>(index, e);
                    changed = true;
                }

                if (changed)
                {
                    MarkRoadAndSubBlocksUpdated(index, e);
                }
            }

            private ZoningPreviewComponent CreatePreviewData(Entity roadEntity, int2 previewDepths)
            {
                bool hasCommittedUpgraded = UpgradedLookup.TryGetComponent(roadEntity, out Upgraded upgraded);
                return new ZoningPreviewComponent
                {
                    Depths = previewDepths,
                    CommittedDepths = GetCommittedRoadDepths(roadEntity),
                    CommittedFlags = hasCommittedUpgraded ? upgraded.m_Flags : default,
                    HasCommittedUpgraded = hasCommittedUpgraded
                };
            }

            private int2 ConstrainDepthsForProtectedCells(Entity roadEntity, int2 current, int2 desired)
            {
                if (!ProtectOccupiedCells && !ProtectZonedCells)
                    return desired;

                if (desired.x < current.x &&
                    HasProtectedCellsOnSide(roadEntity, leftSide: true))
                {
                    desired.x = current.x;
                }

                if (desired.y < current.y &&
                    HasProtectedCellsOnSide(roadEntity, leftSide: false))
                {
                    desired.y = current.y;
                }

                return desired;
            }

            private bool HasProtectedCellsOnSide(Entity roadEntity, bool leftSide)
            {
                if (!CurveLookup.TryGetComponent(roadEntity, out Curve curve) ||
                    !SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
                {
                    return false;
                }

                for (int i = 0; i < subBlocks.Length; i++)
                {
                    Entity blockEntity = subBlocks[i].m_SubBlock;
                    if (!BlockLookup.TryGetComponent(blockEntity, out Block block) ||
                        !ValidAreaLookup.TryGetComponent(blockEntity, out ValidArea validArea) ||
                        !CellLookup.TryGetBuffer(blockEntity, out DynamicBuffer<Cell> cells))
                    {
                        continue;
                    }

                    if (RoadZoneCompatibility.IsBlockOnLeft(block, curve) != leftSide)
                        continue;

                    if (HasProtectedCells(cells, block, validArea, ProtectOccupiedCells, ProtectZonedCells))
                        return true;
                }

                return false;
            }

            private int2 GetCommittedRoadDepths(Entity roadEntity)
            {
                if (ZoningRestoreLookup.TryGetComponent(roadEntity, out ZoningRestoreComponent restore))
                    return restore.Depths;

                if (UpgradedLookup.TryGetComponent(roadEntity, out Upgraded upgraded) &&
                    RoadZoneCompatibility.TryGetDepthsFromFlags(upgraded.m_Flags, out int2 flaggedDepths))
                {
                    return flaggedDepths;
                }

                if (TryGetDepthsFromBlockLayout(roadEntity, out int2 blockDepths))
                    return blockDepths;

                if (ZoningDepthLookup.TryGetComponent(roadEntity, out ZoningDepthComponent depth))
                    return depth.Depths;

                return kVanillaDepths;
            }

            private bool TryGetDepthsFromBlockLayout(Entity roadEntity, out int2 depths)
            {
                depths = kVanillaDepths;

                if (!CurveLookup.TryGetComponent(roadEntity, out Curve curve) ||
                    !SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
                {
                    return false;
                }

                bool sawLeft = false;
                bool sawRight = false;
                bool leftEnabled = false;
                bool leftDisabled = false;
                bool rightEnabled = false;
                bool rightDisabled = false;

                for (int i = 0; i < subBlocks.Length; i++)
                {
                    Entity blockEntity = subBlocks[i].m_SubBlock;
                    if (!BlockLookup.TryGetComponent(blockEntity, out Block block) ||
                        !ValidAreaLookup.TryGetComponent(blockEntity, out ValidArea validArea))
                    {
                        continue;
                    }

                    bool enabled = block.m_Size.y > 0 && validArea.m_Area.w > 0;
                    bool left = RoadZoneCompatibility.IsBlockOnLeft(block, curve);

                    if (left)
                    {
                        sawLeft = true;
                        leftEnabled |= enabled;
                        leftDisabled |= !enabled;
                    }
                    else
                    {
                        sawRight = true;
                        rightEnabled |= enabled;
                        rightDisabled |= !enabled;
                    }
                }

                if (!sawLeft || !sawRight ||
                    (leftEnabled && leftDisabled) ||
                    (rightEnabled && rightDisabled))
                {
                    return false;
                }

                depths = RoadZoneCompatibility.DepthsFromDisabledSides(leftDisabled, rightDisabled);
                return true;
            }

            private bool ApplyPreviewRoadUpgradedState(int index, Entity roadEntity, ZoningPreviewComponent preview)
            {
                bool hasUpgraded = UpgradedLookup.TryGetComponent(roadEntity, out Upgraded upgraded);
                CompositionFlags baseFlags = preview.HasCommittedUpgraded ? preview.CommittedFlags : default;
                int2 expandedDepths = math.max(preview.Depths, preview.CommittedDepths);
                CompositionFlags previewFlags = RoadZoneCompatibility.ApplyDepthsToFlags(baseFlags, expandedDepths);

                if (RoadZoneCompatibility.HasAnyFlags(previewFlags) || preview.HasCommittedUpgraded)
                {
                    Upgraded nextUpgraded = new Upgraded { m_Flags = previewFlags };
                    if (hasUpgraded)
                    {
                        if (upgraded.m_Flags == previewFlags)
                            return false;

                        ECB.SetComponent(index, roadEntity, nextUpgraded);
                        return true;
                    }

                    ECB.AddComponent(index, roadEntity, nextUpgraded);
                    return true;
                }

                if (hasUpgraded)
                {
                    ECB.RemoveComponent<Upgraded>(index, roadEntity);
                    return true;
                }

                return false;
            }

            private void MarkRoadAndSubBlocksUpdated(int index, Entity roadEntity)
            {
                if (!UpdatedLookup.HasComponent(roadEntity))
                    ECB.AddComponent<Updated>(index, roadEntity);

                if (!SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
                    return;

                for (int i = 0; i < subBlocks.Length; i++)
                {
                    Entity subBlock = subBlocks[i].m_SubBlock;
                    if (subBlock != Entity.Null && !UpdatedLookup.HasComponent(subBlock))
                    {
                        ECB.AddComponent<Updated>(index, subBlock);
                    }
                }
            }

        }

        // Remove preview components from entities not currently selected.
        public struct CleanupTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;

            [ReadOnly] public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            [ReadOnly] public ComponentLookup<ZoningRestoreComponent> ZoningRestoreLookup;
            [ReadOnly] public BufferLookup<SubBlock> SubBlockLookup;
            [ReadOnly] public ComponentLookup<Upgraded> UpgradedLookup;
            [ReadOnly] public ComponentLookup<Updated> UpdatedLookup;

            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public NativeArray<Entity>.ReadOnly Entities;

            public void Execute(int index)
            {
                Entity e = Entities[index];
                if (SelectedEntities.Contains(e))
                    return;

                if (!ZoningPreviewLookup.TryGetComponent(e, out ZoningPreviewComponent preview))
                    return;

                bool changed = RestoreCommittedUpgradedState(index, e, preview);
                bool needsRestore = math.any(preview.Depths > preview.CommittedDepths);
                if (needsRestore)
                {
                    ZoningRestoreComponent restore = new ZoningRestoreComponent { Depths = preview.CommittedDepths };
                    if (ZoningRestoreLookup.HasComponent(e))
                        ECB.SetComponent(index, e, restore);
                    else
                        ECB.AddComponent(index, e, restore);

                    changed = true;
                }

                if (changed)
                    MarkRoadAndSubBlocksUpdated(index, e);

                ECB.RemoveComponent<ZoningPreviewComponent>(index, e);
            }

            private bool RestoreCommittedUpgradedState(int index, Entity roadEntity, ZoningPreviewComponent preview)
            {
                bool hasUpgraded = UpgradedLookup.TryGetComponent(roadEntity, out Upgraded upgraded);
                if (preview.HasCommittedUpgraded)
                {
                    Upgraded committedUpgraded = new Upgraded { m_Flags = preview.CommittedFlags };
                    if (hasUpgraded)
                    {
                        if (upgraded.m_Flags == preview.CommittedFlags)
                            return false;

                        ECB.SetComponent(index, roadEntity, committedUpgraded);
                        return true;
                    }

                    ECB.AddComponent(index, roadEntity, committedUpgraded);
                    return true;
                }

                if (hasUpgraded)
                {
                    ECB.RemoveComponent<Upgraded>(index, roadEntity);
                    return true;
                }

                return false;
            }

            private void MarkRoadAndSubBlocksUpdated(int jobIndex, Entity roadEntity)
            {
                if (!UpdatedLookup.HasComponent(roadEntity))
                {
                    ECB.AddComponent<Updated>(jobIndex, roadEntity);
                }

                if (!SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
                    return;

                for (int i = 0; i < subBlocks.Length; i++)
                {
                    Entity blockEntity = subBlocks[i].m_SubBlock;
                    if (blockEntity != Entity.Null && !UpdatedLookup.HasComponent(blockEntity))
                    {
                        ECB.AddComponent<Updated>(jobIndex, blockEntity);
                    }
                }
            }
        }

        // Apply commits the chosen depths to roads.
        // Updated is only added if it isn't already present.
        public struct SetZoningDepthJob : IJob
        {
            public NativeArray<Entity>.ReadOnly Entities;

            [ReadOnly] public BufferLookup<SubBlock> SubBlockLookup;
            [ReadOnly] public BufferLookup<Cell> CellLookup;
            [ReadOnly] public ComponentLookup<Block> BlockLookup;
            [ReadOnly] public ComponentLookup<Curve> CurveLookup;
            [ReadOnly] public ComponentLookup<ValidArea> ValidAreaLookup;
            [ReadOnly] public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            [ReadOnly] public ComponentLookup<ZoningRestoreComponent> ZoningRestoreLookup;
            [ReadOnly] public ComponentLookup<ZoningDepthComponent> DepthLookup;
            [ReadOnly] public ComponentLookup<Upgraded> UpgradedLookup;
            [ReadOnly] public ComponentLookup<Updated> UpdatedLookup;

            public int2 ToolDepths;
            public NativeArray<int2>.ReadOnly DesiredDepths;
            public bool ProtectOccupiedCells;
            public bool ProtectZonedCells;
            public EntityCommandBuffer ECB;

            public void Execute( )
            {
                for (int i = 0; i < Entities.Length; i++)
                {
                    Entity e = Entities[i];
                    bool hasPreview = ZoningPreviewLookup.TryGetComponent(e, out ZoningPreviewComponent preview);
                    int2 current = hasPreview
                        ? preview.CommittedDepths
                        : GetCommittedRoadDepths(e);
                    int2 requested = i < DesiredDepths.Length ? DesiredDepths[i] : ToolDepths;
                    int2 desired = ConstrainDepthsForProtectedCells(e, current, requested);

                    if (hasPreview)
                        ECB.RemoveComponent<ZoningPreviewComponent>(e);

                    bool hasRestore = ZoningRestoreLookup.HasComponent(e);

                    bool useVanillaDepths = math.all(desired == kVanillaDepths);
                    if (DepthLookup.HasComponent(e))
                    {
                        if (useVanillaDepths)
                            ECB.RemoveComponent<ZoningDepthComponent>(e);
                        else
                            ECB.SetComponent(e, new ZoningDepthComponent { Depths = desired });
                    }
                    else if (!useVanillaDepths)
                    {
                        ECB.AddComponent(e, new ZoningDepthComponent { Depths = desired });
                    }

                    bool hasUpgraded = UpgradedLookup.TryGetComponent(e, out Upgraded upgraded);
                    CompositionFlags baseFlags = hasPreview && preview.HasCommittedUpgraded
                        ? preview.CommittedFlags
                        : hasUpgraded
                            ? upgraded.m_Flags
                            : default;
                    CompositionFlags nextFlags = RoadZoneCompatibility.ApplyDepthsToFlags(
                        baseFlags,
                        desired);

                    if (RoadZoneCompatibility.HasAnyFlags(nextFlags))
                    {
                        Upgraded nextUpgraded = new Upgraded { m_Flags = nextFlags };
                        if (hasUpgraded)
                        {
                            if (upgraded.m_Flags != nextFlags)
                                ECB.SetComponent(e, nextUpgraded);
                        }
                        else
                        {
                            ECB.AddComponent(e, nextUpgraded);
                        }
                    }
                    else if (hasUpgraded)
                    {
                        ECB.RemoveComponent<Upgraded>(e);
                    }

                    // Both is represented by vanilla/default state, so the stored EZ
                    // depth component and ZonesDisabled flags are removed. Keep a
                    // one-frame depth sync target so SyncBlockSystem restores blocks
                    // from old 0-depth/None layout back to normal 6/6.
                    if (useVanillaDepths && !math.all(current == desired))
                    {
                        ZoningRestoreComponent syncDepths = new ZoningRestoreComponent { Depths = desired };
                        if (hasRestore)
                            ECB.SetComponent(e, syncDepths);
                        else
                            ECB.AddComponent(e, syncDepths);
                    }
                    else if (hasRestore)
                    {
                        ECB.RemoveComponent<ZoningRestoreComponent>(e);
                    }

                    if (!UpdatedLookup.HasComponent(e))
                        ECB.AddComponent<Updated>(e);

                    if (SubBlockLookup.TryGetBuffer(e, out DynamicBuffer<SubBlock> subBlocks))
                    {
                        for (int j = 0; j < subBlocks.Length; j++)
                        {
                            Entity subBlock = subBlocks[j].m_SubBlock;
                            if (subBlock != Entity.Null && !UpdatedLookup.HasComponent(subBlock))
                            {
                                ECB.AddComponent<Updated>(subBlock);
                            }
                        }
                    }
                }
            }

            private int2 GetCommittedRoadDepths(Entity roadEntity)
            {
                if (ZoningRestoreLookup.TryGetComponent(roadEntity, out ZoningRestoreComponent restore))
                    return restore.Depths;

                if (UpgradedLookup.TryGetComponent(roadEntity, out Upgraded upgraded) &&
                    RoadZoneCompatibility.TryGetDepthsFromFlags(upgraded.m_Flags, out int2 flaggedDepths))
                {
                    return flaggedDepths;
                }

                if (TryGetDepthsFromBlockLayout(roadEntity, out int2 blockDepths))
                    return blockDepths;

                if (DepthLookup.TryGetComponent(roadEntity, out ZoningDepthComponent depth))
                    return depth.Depths;

                return kVanillaDepths;
            }

            private int2 ConstrainDepthsForProtectedCells(Entity roadEntity, int2 current, int2 desired)
            {
                if (!ProtectOccupiedCells && !ProtectZonedCells)
                    return desired;

                if (desired.x < current.x &&
                    HasProtectedCellsOnSide(roadEntity, leftSide: true))
                {
                    desired.x = current.x;
                }

                if (desired.y < current.y &&
                    HasProtectedCellsOnSide(roadEntity, leftSide: false))
                {
                    desired.y = current.y;
                }

                return desired;
            }

            private bool HasProtectedCellsOnSide(Entity roadEntity, bool leftSide)
            {
                if (!CurveLookup.TryGetComponent(roadEntity, out Curve curve) ||
                    !SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
                {
                    return false;
                }

                for (int i = 0; i < subBlocks.Length; i++)
                {
                    Entity blockEntity = subBlocks[i].m_SubBlock;
                    if (!BlockLookup.TryGetComponent(blockEntity, out Block block) ||
                        !ValidAreaLookup.TryGetComponent(blockEntity, out ValidArea validArea) ||
                        !CellLookup.TryGetBuffer(blockEntity, out DynamicBuffer<Cell> cells))
                    {
                        continue;
                    }

                    if (RoadZoneCompatibility.IsBlockOnLeft(block, curve) != leftSide)
                        continue;

                    if (HasProtectedCells(cells, block, validArea, ProtectOccupiedCells, ProtectZonedCells))
                        return true;
                }

                return false;
            }

            private bool TryGetDepthsFromBlockLayout(Entity roadEntity, out int2 depths)
            {
                depths = kVanillaDepths;

                if (!CurveLookup.TryGetComponent(roadEntity, out Curve curve) ||
                    !SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
                {
                    return false;
                }

                bool sawLeft = false;
                bool sawRight = false;
                bool leftEnabled = false;
                bool leftDisabled = false;
                bool rightEnabled = false;
                bool rightDisabled = false;

                for (int i = 0; i < subBlocks.Length; i++)
                {
                    Entity blockEntity = subBlocks[i].m_SubBlock;
                    if (!BlockLookup.TryGetComponent(blockEntity, out Block block) ||
                        !ValidAreaLookup.TryGetComponent(blockEntity, out ValidArea validArea))
                    {
                        continue;
                    }

                    bool enabled = block.m_Size.y > 0 && validArea.m_Area.w > 0;
                    bool left = RoadZoneCompatibility.IsBlockOnLeft(block, curve);

                    if (left)
                    {
                        sawLeft = true;
                        leftEnabled |= enabled;
                        leftDisabled |= !enabled;
                    }
                    else
                    {
                        sawRight = true;
                        rightEnabled |= enabled;
                        rightDisabled |= !enabled;
                    }
                }

                if (!sawLeft || !sawRight ||
                    (leftEnabled && leftDisabled) ||
                    (rightEnabled && rightDisabled))
                {
                    return false;
                }

                depths = RoadZoneCompatibility.DepthsFromDisabledSides(leftDisabled, rightDisabled);
                return true;
            }
        }

        private bool WouldPreviewRemoveCommittedSide(Entity roadEntity, int2 desiredDepths)
        {
            int2 current = GetCommittedRoadDepths(roadEntity);
            return desiredDepths.x < current.x || desiredDepths.y < current.y;
        }

        private void SyncVanillaRemovalPreviewTemp(Entity roadEntity, int2 previewDepths)
        {
            if (roadEntity == Entity.Null || !WouldPreviewRemoveCommittedSide(roadEntity, previewDepths))
            {
                ClearVanillaRemovalPreviewTemp();
                return;
            }

            if (m_VanillaRemovalPreviewRoad != Entity.Null && m_VanillaRemovalPreviewRoad != roadEntity)
            {
                ClearRoadHighlightImmediate(m_VanillaRemovalPreviewRoad);
            }

            if (m_VanillaRemovalPreviewEntity == Entity.Null || !EntityManager.Exists(m_VanillaRemovalPreviewEntity))
            {
                m_VanillaRemovalPreviewEntity = EntityManager.CreateEntity();
            }

            bool hasCommittedUpgraded = m_UpgradedLookup.TryGetComponent(roadEntity, out Upgraded committedUpgraded);
            CompositionFlags baseFlags = hasCommittedUpgraded ? committedUpgraded.m_Flags : default;
            CompositionFlags previewFlags = RoadZoneCompatibility.ApplyDepthsToFlags(baseFlags, previewDepths);

            DynamicBuffer<SubBlock> previewSubBlocks = EntityManager.HasBuffer<SubBlock>(m_VanillaRemovalPreviewEntity)
                ? EntityManager.GetBuffer<SubBlock>(m_VanillaRemovalPreviewEntity)
                : EntityManager.AddBuffer<SubBlock>(m_VanillaRemovalPreviewEntity);

            if (m_SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> sourceSubBlocks))
            {
                previewSubBlocks.CopyFrom(sourceSubBlocks);
            }
            else
            {
                previewSubBlocks.Clear();
            }

            Temp previewTemp = new Temp(roadEntity, TempFlags.Modify | TempFlags.Upgrade);
            if (EntityManager.HasComponent<Temp>(m_VanillaRemovalPreviewEntity))
                EntityManager.SetComponentData(m_VanillaRemovalPreviewEntity, previewTemp);
            else
                EntityManager.AddComponentData(m_VanillaRemovalPreviewEntity, previewTemp);

            Upgraded previewUpgraded = new Upgraded { m_Flags = previewFlags };
            if (EntityManager.HasComponent<Upgraded>(m_VanillaRemovalPreviewEntity))
                EntityManager.SetComponentData(m_VanillaRemovalPreviewEntity, previewUpgraded);
            else
                EntityManager.AddComponentData(m_VanillaRemovalPreviewEntity, previewUpgraded);

            if (EntityManager.HasComponent<Deleted>(m_VanillaRemovalPreviewEntity))
                EntityManager.RemoveComponent<Deleted>(m_VanillaRemovalPreviewEntity);

            if (!EntityManager.HasComponent<ZoneGridHighlighted>(m_VanillaRemovalPreviewEntity))
                EntityManager.AddComponent<ZoneGridHighlighted>(m_VanillaRemovalPreviewEntity);

            if (!EntityManager.HasComponent<Updated>(m_VanillaRemovalPreviewEntity))
                EntityManager.AddComponent<Updated>(m_VanillaRemovalPreviewEntity);

            m_VanillaRemovalPreviewRoad = roadEntity;
        }

        private void ClearVanillaRemovalPreviewTemp()
        {
            if (m_VanillaRemovalPreviewRoad != Entity.Null)
            {
                ClearRoadHighlightImmediate(m_VanillaRemovalPreviewRoad);
            }

            if (m_VanillaRemovalPreviewEntity != Entity.Null && EntityManager.Exists(m_VanillaRemovalPreviewEntity))
            {
                EntityManager.DestroyEntity(m_VanillaRemovalPreviewEntity);
            }

            m_VanillaRemovalPreviewEntity = Entity.Null;
            m_VanillaRemovalPreviewRoad = Entity.Null;
        }

        private void ClearRoadHighlightImmediate(Entity roadEntity)
        {
            if (roadEntity == Entity.Null ||
                !m_SubBlockLookup.TryGetBuffer(roadEntity, out DynamicBuffer<SubBlock> subBlocks))
            {
                return;
            }

            for (int i = 0; i < subBlocks.Length; i++)
            {
                Entity subBlock = subBlocks[i].m_SubBlock;
                if (subBlock == Entity.Null || !m_CellLookup.TryGetBuffer(subBlock, out DynamicBuffer<Cell> cells))
                    continue;

                bool changed = false;
                for (int j = 0; j < cells.Length; j++)
                {
                    Cell cell = cells[j];
                    CellFlags nextState = cell.m_State & ~CellFlags.Highlight;
                    if (nextState == cell.m_State)
                        continue;

                    cell.m_State = nextState;
                    cells[j] = cell;
                    changed = true;
                }

                if (changed && !EntityManager.HasComponent<Updated>(subBlock))
                    EntityManager.AddComponent<Updated>(subBlock);
            }
        }

        private void ClearTransientPreviewState()
        {
            ClearVanillaRemovalPreviewTemp();

            NativeArray<Entity> previewEntities = m_ZoningPreviewQuery.ToEntityArray(Allocator.Temp);
            try
            {
                for (int i = 0; i < previewEntities.Length; i++)
                {
                    Entity roadEntity = previewEntities[i];
                    if (!EntityManager.HasComponent<ZoningPreviewComponent>(roadEntity))
                        continue;

                    ZoningPreviewComponent preview = EntityManager.GetComponentData<ZoningPreviewComponent>(roadEntity);
                    bool changed = RestoreCommittedUpgradedStateImmediate(roadEntity, preview);
                    if (!math.all(preview.Depths == preview.CommittedDepths))
                    {
                        ZoningRestoreComponent restore = new ZoningRestoreComponent { Depths = preview.CommittedDepths };
                        if (EntityManager.HasComponent<ZoningRestoreComponent>(roadEntity))
                            EntityManager.SetComponentData(roadEntity, restore);
                        else
                            EntityManager.AddComponentData(roadEntity, restore);

                        changed = true;
                    }

                    if (changed)
                        MarkRoadAndSubBlocksUpdatedImmediate(roadEntity);

                    EntityManager.RemoveComponent<ZoningPreviewComponent>(roadEntity);
                }
            }
            finally
            {
                previewEntities.Dispose();
            }
        }

        private bool RestoreCommittedUpgradedStateImmediate(Entity roadEntity, ZoningPreviewComponent preview)
        {
            bool hasUpgraded = EntityManager.HasComponent<Upgraded>(roadEntity);
            if (preview.HasCommittedUpgraded)
            {
                Upgraded committedUpgraded = new Upgraded { m_Flags = preview.CommittedFlags };
                if (hasUpgraded)
                {
                    Upgraded current = EntityManager.GetComponentData<Upgraded>(roadEntity);
                    if (current.m_Flags == preview.CommittedFlags)
                        return false;

                    EntityManager.SetComponentData(roadEntity, committedUpgraded);
                    return true;
                }

                EntityManager.AddComponentData(roadEntity, committedUpgraded);
                return true;
            }

            if (hasUpgraded)
            {
                EntityManager.RemoveComponent<Upgraded>(roadEntity);
                return true;
            }

            return false;
        }

        private void MarkRoadAndSubBlocksUpdatedImmediate(Entity roadEntity)
        {
            if (!EntityManager.HasComponent<Updated>(roadEntity))
                EntityManager.AddComponent<Updated>(roadEntity);

            if (!EntityManager.HasBuffer<SubBlock>(roadEntity))
                return;

            DynamicBuffer<SubBlock> subBlocks = EntityManager.GetBuffer<SubBlock>(roadEntity);
            for (int i = 0; i < subBlocks.Length; i++)
            {
                Entity subBlock = subBlocks[i].m_SubBlock;
                if (subBlock != Entity.Null && !EntityManager.HasComponent<Updated>(subBlock))
                    EntityManager.AddComponent<Updated>(subBlock);
            }
        }
    }
}
