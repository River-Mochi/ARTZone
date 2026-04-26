// File: src/Tools/SyncBlockSystem.cs
// Purpose: Applies the preview/committed zoning depth to zone blocks
// respecting settings (RemoveZonedCells / RemoveOccupiedCells). Tool will not function without it.

namespace EasyZoning.Tools
{
    using EasyZoning.Components;    // ZoningDepthComponent, ZoningPreviewComponent, ZoningRestoreComponent
    using Game;
    using Game.Common;              // Owner, Updated (dirty marker pattern)
    using Game.Net;                 // Curve, Upgraded
    using Game.Tools;               // ToolSystem, UpgradeToolSystem
    using Game.Zones;               // Block, ValidArea, Cell, ZoneType
    using System;                   // InvalidOperationException (DEBUG guard)
    using Unity.Collections;        // NativeArray
    using Unity.Entities;           // DynamicBuffer, EntityQuery, ComponentLookup, BufferLookup, ECB
    using Unity.Jobs;               // IJobParallelFor, JobHandle
    using Unity.Mathematics;        // int2, math.clamp

    public partial class SyncBlockSystem : GameSystemBase
    {
        private EntityQuery m_UpdatedBlocksQuery;
        private ModificationBarrier4B m_ModificationBarrier = null!;
        private ToolSystem m_ToolSystem = null!;

#if DEBUG
        private int m_LogTick;
        private int m_LastCount;
#endif

        protected override void OnCreate( )
        {
            base.OnCreate();

            m_UpdatedBlocksQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<Block, ValidArea>()
                .WithAll<Owner, Updated>()
                .Build(this);

            m_ModificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4B>();
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();

#if DEBUG
            m_LogTick = 0;
            m_LastCount = -1;
#endif
        }

        protected override void OnUpdate( )
        {
            if (m_UpdatedBlocksQuery.IsEmpty)
            {
                return;
            }

            // Read settings once per update.
            bool removeOccupied = Mod.Settings != null && Mod.Settings.RemoveOccupiedCells;
            bool removeZoned = Mod.Settings != null && Mod.Settings.RemoveZonedCells;

#if DEBUG
            int count = m_UpdatedBlocksQuery.CalculateEntityCount();
            m_LogTick++;
            if (count != m_LastCount || (m_LogTick % 60) == 0)
            {
                Mod.s_Log.Info(
                    $"[EZ][SyncBlock] scan Updated blocks={count} (game-driven dirty flags are normal after load)\n " +
                    $"settings: removeOcc={removeOccupied} removeZoned={removeZoned}");
                m_LastCount = count;
            }
#endif

            EntityCommandBuffer ecb = m_ModificationBarrier.CreateCommandBuffer();
            NativeArray<Entity> updatedBlocks = m_UpdatedBlocksQuery.ToEntityArray(Allocator.TempJob);

            JobHandle syncBlockJob = new SyncBlockJob
            {
                ECB = ecb.AsParallelWriter(),
                Entities = updatedBlocks.AsReadOnly(),
                BlockLookup = GetComponentLookup<Block>(isReadOnly: true),
                ValidAreaLookup = GetComponentLookup<ValidArea>(isReadOnly: true),
                OwnerLookup = GetComponentLookup<Owner>(isReadOnly: true),
                CurveLookup = GetComponentLookup<Curve>(isReadOnly: true),
                CellLookup = GetBufferLookup<Cell>(isReadOnly: true),
                SubBlockLookup = GetBufferLookup<SubBlock>(isReadOnly: true),
                TempLookup = GetComponentLookup<Temp>(isReadOnly: true),
                UpgradedLookup = GetComponentLookup<Upgraded>(isReadOnly: true),
                ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(isReadOnly: true),
                ZoningPreviewLookup = GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true),
                ZoningRestoreLookup = GetComponentLookup<ZoningRestoreComponent>(isReadOnly: true),
                SuppressTempRoads = m_ToolSystem != null && m_ToolSystem.activeTool is UpgradeToolSystem,
                RemoveOccupiedCells = removeOccupied,
                RemoveZonedCells = removeZoned,
            }.Schedule(updatedBlocks.Length, 32, Dependency);

            updatedBlocks.Dispose(syncBlockJob);
            Dependency = JobHandle.CombineDependencies(Dependency, syncBlockJob);
            m_ModificationBarrier.AddJobHandleForProducer(Dependency);
        }

        public struct SyncBlockJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity>.ReadOnly Entities;

            [ReadOnly] public ComponentLookup<Block> BlockLookup;
            [ReadOnly] public ComponentLookup<ValidArea> ValidAreaLookup;
            [ReadOnly] public BufferLookup<Cell> CellLookup;
            [ReadOnly] public ComponentLookup<Owner> OwnerLookup;
            [ReadOnly] public ComponentLookup<Curve> CurveLookup;
            [ReadOnly] public BufferLookup<SubBlock> SubBlockLookup;
            [ReadOnly] public ComponentLookup<Temp> TempLookup;
            [ReadOnly] public ComponentLookup<Upgraded> UpgradedLookup;
            [ReadOnly] public ComponentLookup<ZoningDepthComponent> ZoningDepthLookup;
            [ReadOnly] public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;
            [ReadOnly] public ComponentLookup<ZoningRestoreComponent> ZoningRestoreLookup;

            public bool SuppressTempRoads;
            public bool RemoveOccupiedCells;
            public bool RemoveZonedCells;

            public void Execute(int index)
            {
                Entity blockEntity = Entities[index];

                Block block = BlockLookup[blockEntity];
                ValidArea validArea = ValidAreaLookup[blockEntity];

                if (!OwnerLookup.TryGetComponent(blockEntity, out Owner owner))
                {
#if DEBUG
                    throw new InvalidOperationException($"[EZ] Block {blockEntity} missing Owner (query expected Owner).");
#else
                    return;
#endif
                }

                Entity roadEntity = owner.m_Owner;

                if (SuppressTempRoads &&
                    TempLookup.HasComponent(roadEntity) &&
                    !ZoningPreviewLookup.HasComponent(roadEntity))
                {
                    return;
                }

                bool left = CurveLookup.TryGetComponent(roadEntity, out Curve curve)
                    ? RoadZoneCompatibility.IsBlockOnLeft(block, curve)
                    : IsLeftSideFallback(block);

                if (!TryGetEffectiveRoadDepths(roadEntity, out int2 depths))
                {
                    return;
                }

                int depth = left ? depths.x : depths.y;

                DynamicBuffer<Cell> cells = CellLookup[blockEntity];

                if (RemoveOccupiedCells && IsAnyCellOccupied(cells, block, validArea))
                {
                    return;
                }

                if (RemoveZonedCells && IsAnyCellZoned(cells, block, validArea))
                {
                    return;
                }

                block.m_Size.y = depth;
                ECB.SetComponent(index, blockEntity, block);

                validArea.m_Area.w = depth;
                ECB.SetComponent(index, blockEntity, validArea);

                if (ZoningRestoreLookup.HasComponent(roadEntity))
                    ECB.RemoveComponent<ZoningRestoreComponent>(index, roadEntity);
            }

            private bool TryGetEffectiveRoadDepths(Entity roadEntity, out int2 depths)
            {
                if (ZoningPreviewLookup.TryGetComponent(roadEntity, out ZoningPreviewComponent zoningPreview))
                {
                    depths = zoningPreview.Depths;
                    return true;
                }

                if (ZoningRestoreLookup.TryGetComponent(roadEntity, out ZoningRestoreComponent zoningRestore))
                {
                    depths = zoningRestore.Depths;
                    return true;
                }

                if (UpgradedLookup.TryGetComponent(roadEntity, out Upgraded upgraded) &&
                    RoadZoneCompatibility.TryGetDepthsFromFlags(upgraded.m_Flags, out int2 flaggedDepths))
                {
                    depths = flaggedDepths;
                    return true;
                }

                if (TryGetDepthsFromBlockLayout(roadEntity, out int2 blockLayoutDepths))
                {
                    depths = blockLayoutDepths;
                    return true;
                }

                if (ZoningDepthLookup.TryGetComponent(roadEntity, out ZoningDepthComponent data))
                {
                    depths = data.Depths;
                    return true;
                }

                depths = default;
                return false;
            }

            private bool TryGetDepthsFromBlockLayout(Entity roadEntity, out int2 depths)
            {
                depths = RoadZoneCompatibility.VanillaDepths;

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
                    Entity subBlockEntity = subBlocks[i].m_SubBlock;
                    if (!BlockLookup.TryGetComponent(subBlockEntity, out Block subBlock) ||
                        !ValidAreaLookup.TryGetComponent(subBlockEntity, out ValidArea subArea))
                    {
                        continue;
                    }

                    bool enabled = subBlock.m_Size.y > 0 && subArea.m_Area.w > 0;
                    bool isLeft = RoadZoneCompatibility.IsBlockOnLeft(subBlock, curve);

                    if (isLeft)
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

            private static bool IsLeftSideFallback(Block block)
            {
                // ART behavior: use block direction sign as the left/right discriminator.
                // (float2(1,1) matches ART's implicit math.dot(1, dir) usage.)
                return math.dot(new float2(1f, 1f), block.m_Direction) < 0f;
            }

            private static bool IsAnyCellOccupied(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                int x0 = validArea.m_Area.x;
                int x1 = validArea.m_Area.y;
                int z0 = validArea.m_Area.z;
                int z1 = validArea.m_Area.w;

                // Empty or invalid ranges.
                if (x1 <= x0 || z1 <= z0)
                    return false;

                // Clamp to block bounds.
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
                        if ((cell.m_State & CellFlags.Occupied) != 0)
                            return true;
                    }
                }

                return false;
            }

            private static bool IsAnyCellZoned(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                int x0 = validArea.m_Area.x;
                int x1 = validArea.m_Area.y;
                int z0 = validArea.m_Area.z;
                int z1 = validArea.m_Area.w;

                // Empty or invalid ranges.
                if (x1 <= x0 || z1 <= z0)
                    return false;

                // Clamp to block bounds.
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
                        if (cell.m_Zone.m_Index != ZoneType.None.m_Index)
                            return true;
                    }
                }

                return false;
            }

        }
    }
}
