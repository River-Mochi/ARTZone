// File: src/Tools/SyncBlockSystem.cs
// Purpose: applies the preview/committed zoning depth to actual zone blocks
// respecting settings (RemoveZonedCells / RemoveOccupiedCells). Tool won’t function without it

namespace AdvancedRoadTools
{
    using AdvancedRoadTools.Components;
    using Game;
    using Game.Common;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    public partial class SyncBlockSystem : GameSystemBase
    {
        private EntityQuery m_UpdatedBlocksQuery;
        private ModificationBarrier4B m_ModificationBarrier = null!; // assigned in OnCreate

        protected override void OnCreate()
        {
            base.OnCreate();

            m_UpdatedBlocksQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<Block, ValidArea>()  // we will write both
                .WithAll<Owner, Updated>()      // only blocks touched this frame
                .Build(this);

            m_ModificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4B>();
        }

        protected override void OnUpdate()
        {
            if (m_UpdatedBlocksQuery.IsEmpty)
                return;

            // Snapshot settings so the job never touches static settings (thread-safe & NRE-safe)
            bool removeOccupied = AdvancedRoadToolsMod.s_Settings?.RemoveOccupiedCells == true;
            bool removeZoned = AdvancedRoadToolsMod.s_Settings?.RemoveZonedCells == true;

            var ecb = m_ModificationBarrier.CreateCommandBuffer();
            NativeArray<Entity> updatedBlocks = m_UpdatedBlocksQuery.ToEntityArray(Allocator.TempJob);

#if DEBUG
            AdvancedRoadToolsMod.s_Log.Info($"[ART][SyncBlock] blocks={updatedBlocks.Length} removeOcc={removeOccupied} removeZoned={removeZoned}");
#endif

            JobHandle syncBlockJob = new SyncBlockJob
            {
                ECB = ecb.AsParallelWriter(),
                Entities = updatedBlocks.AsReadOnly(),
                BlockLookup = GetComponentLookup<Block>(true),
                ValidAreaLookup = GetComponentLookup<ValidArea>(true),
                OwnerLookup = GetComponentLookup<Owner>(true),
                CellLookup = GetBufferLookup<Cell>(true),
                AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true),
                TempZoningLookup = GetComponentLookup<TempZoning>(true),
                RemoveOccupiedCells = removeOccupied,
                RemoveZonedCells = removeZoned
            }.Schedule(m_UpdatedBlocksQuery.CalculateEntityCount(), 32, Dependency);

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
            [ReadOnly] public ComponentLookup<Owner> OwnerLookup;
            [ReadOnly] public BufferLookup<Cell> CellLookup;
            [ReadOnly] public ComponentLookup<AdvancedRoad> AdvancedRoadLookup;
            [ReadOnly] public ComponentLookup<TempZoning> TempZoningLookup;

            public bool RemoveOccupiedCells;
            public bool RemoveZonedCells;

            public void Execute(int index)
            {
                Entity blockEntity = Entities[index];

                // Required components (by query); still access guarded to be defensive under late deletes.
                if (!BlockLookup.HasComponent(blockEntity) || !ValidAreaLookup.HasComponent(blockEntity))
                    return;

                Block block = BlockLookup[blockEntity];
                ValidArea validArea = ValidAreaLookup[blockEntity];

                // Owner is declared in the query, but be defensive anyway.
                if (!OwnerLookup.TryGetComponent(blockEntity, out Owner owner))
                    return;

                Entity roadEntity = owner.m_Owner;

                // Determine left/right from block direction (x < 0 => left).
                bool left = block.m_Direction.x < 0;

                // Pick a depth from either TempZoning (preview) or persisted AdvancedRoad.
                int depth;
                if (TempZoningLookup.TryGetComponent(roadEntity, out TempZoning tempZoning))
                {
                    depth = left ? tempZoning.Depths.x : tempZoning.Depths.y;
                }
                else if (AdvancedRoadLookup.TryGetComponent(roadEntity, out AdvancedRoad data))
                {
                    depth = left ? data.Depths.x : data.Depths.y;
                }
                else
                {
                    // Road has neither temp nor persisted settings → nothing to do.
                    return;
                }

                // Cells buffer must exist on the block; guard access.
                if (!CellLookup.HasBuffer(blockEntity))
                    return;

                DynamicBuffer<Cell> cells = CellLookup[blockEntity];

                // Respect settings: bail out if any cell in the slice is occupied/zoned.
                if (RemoveOccupiedCells && IsAnyCellOccupied(cells, block, validArea))
                    return;

                if (RemoveZonedCells && IsAnyCellZoned(cells, block, validArea))
                    return;

                // Apply new depth to both Block and ValidArea (w == depth).
                block.m_Size.y = depth;
                ECB.SetComponent(index, blockEntity, block);

                validArea.m_Area.w = depth;
                ECB.SetComponent(index, blockEntity, validArea);
            }

            private bool IsAnyCellOccupied(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                GetClampedRect(block, validArea, out int x0, out int x1, out int z0, out int z1);
                if (x0 >= x1 || z0 >= z1)
                    return false;

                int width = block.m_Size.x;
                for (int z = z0; z < z1; z++)
                {
                    int rowStart = z * width;
                    for (int x = x0; x < x1; x++)
                    {
                        int idx = rowStart + x;
                        if ((cells[idx].m_State & CellFlags.Occupied) != 0)
                            return true;
                    }
                }
                return false;
            }

            private bool IsAnyCellZoned(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                GetClampedRect(block, validArea, out int x0, out int x1, out int z0, out int z1);
                if (x0 >= x1 || z0 >= z1)
                    return false;

                int width = block.m_Size.x;
                for (int z = z0; z < z1; z++)
                {
                    int rowStart = z * width;
                    for (int x = x0; x < x1; x++)
                    {
                        int idx = rowStart + x;
                        if (cells[idx].m_Zone.m_Index != ZoneType.None.m_Index)
                            return true;
                    }
                }
                return false;
            }

            private static void GetClampedRect(Block block, ValidArea validArea,
                                               out int x0, out int x1, out int z0, out int z1)
            {
                int width = math.max(0, block.m_Size.x);
                int depth = math.max(0, block.m_Size.y);

                x0 = math.clamp(validArea.m_Area.x, 0, width);
                x1 = math.clamp(validArea.m_Area.y, 0, width);
                z0 = math.clamp(validArea.m_Area.z, 0, depth);
                z1 = math.clamp(validArea.m_Area.w, 0, depth);
            }
        }
    }
}
