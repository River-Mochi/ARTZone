// File: src/Tools/SyncBlockSystem.cs
// Purpose: Applies the preview/committed zoning depth to zone blocks
// respecting settings (RemoveZonedCells / RemoveOccupiedCells). Tool will not function without it.

namespace EasyZoning.Tools
{
    using EasyZoning.Components;
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
        private ModificationBarrier4B m_ModificationBarrier = null!;

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
                    $"[EZ][SyncBlock] blocks={count} removeOcc={removeOccupied} removeZoned={removeZoned}");
                m_LastCount = count;
            }
#endif

            var ecb = m_ModificationBarrier.CreateCommandBuffer();
            var updatedBlocks = m_UpdatedBlocksQuery.ToEntityArray(Allocator.TempJob);

            var syncBlockJob = new SyncBlockJob
            {
                ECB = ecb.AsParallelWriter(),
                Entities = updatedBlocks.AsReadOnly(),
                BlockLookup = GetComponentLookup<Block>(isReadOnly: true),
                ValidAreaLookup = GetComponentLookup<ValidArea>(isReadOnly: true),
                OwnerLookup = GetComponentLookup<Owner>(isReadOnly: true),
                CellLookup = GetBufferLookup<Cell>(isReadOnly: true),
                ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(isReadOnly: true),
                ZoningPreviewLookup = GetComponentLookup<ZoningPreviewComponent>(isReadOnly: true),
                RemoveOccupiedCells = removeOccupied,
                RemoveZonedCells = removeZoned,
            }.Schedule(updatedBlocks.Length, 32, this.Dependency);

            updatedBlocks.Dispose(syncBlockJob);
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, syncBlockJob);
            m_ModificationBarrier.AddJobHandleForProducer(this.Dependency);
        }




        public struct SyncBlockJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity>.ReadOnly Entities;

            [ReadOnly]
            public ComponentLookup<Block> BlockLookup;

            [ReadOnly]
            public ComponentLookup<ValidArea> ValidAreaLookup;

            [ReadOnly]
            public BufferLookup<Cell> CellLookup;

            [ReadOnly]
            public ComponentLookup<Owner> OwnerLookup;

            [ReadOnly]
            public ComponentLookup<ZoningDepthComponent> ZoningDepthLookup;

            [ReadOnly]
            public ComponentLookup<ZoningPreviewComponent> ZoningPreviewLookup;

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
    throw new NullReferenceException($"Block {blockEntity} has no owner assigned.");
#else
                    return;
#endif
                }

                Entity roadEntity = owner.m_Owner;

                bool left = IsLeftSide(CellLookup[blockEntity], block, validArea);

                int depth;
                if (ZoningPreviewLookup.TryGetComponent(roadEntity, out ZoningPreviewComponent zoningPreview))
                {
                    // Mod convention: Depths.x = LEFT, Depths.y = RIGHT.
                    depth = left ? zoningPreview.Depths.x : zoningPreview.Depths.y;
                }
                else if (ZoningDepthLookup.TryGetComponent(roadEntity, out ZoningDepthComponent data))
                {
                    // Mod convention: Depths.x = LEFT, Depths.y = RIGHT.
                    depth = left ? data.Depths.x : data.Depths.y;
                }
                else
                {
                    return;
                }

                if (RemoveOccupiedCells && IsAnyCellOccupied(CellLookup[blockEntity], block, validArea))
                {
                    return;
                }

                if (RemoveZonedCells && IsAnyCellZoned(CellLookup[blockEntity], block, validArea))
                {
                    return;
                }

                block.m_Size.y = depth;
                ECB.SetComponent(index, blockEntity, block);

                validArea.m_Area.w = depth;
                ECB.SetComponent(index, blockEntity, validArea);
            }

            private static bool IsLeftSide(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                // ART behavior: use block direction sign as the left/right discriminator.
                // (float2(1,1) matches ART's implicit math.dot(1, dir) usage.)
                return math.dot(new float2(1f, 1f), block.m_Direction) < 0f;
            }


            private static bool IsAnyCellOccupied(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                if (validArea.m_Area.y * validArea.m_Area.w == 0)
                {
                    return false;
                }

                for (int z = validArea.m_Area.z; z < validArea.m_Area.w; z++)
                {
                    for (int x = validArea.m_Area.x; x < validArea.m_Area.y; x++)
                    {
                        int idx = z * block.m_Size.x + x;
                        Cell cell = cells[idx];
                        if ((cell.m_State & CellFlags.Occupied) != 0)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private static bool IsAnyCellZoned(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                if (validArea.m_Area.y * validArea.m_Area.w == 0)
                {
                    return false;
                }

                for (int z = validArea.m_Area.z; z < validArea.m_Area.w; z++)
                {
                    for (int x = validArea.m_Area.x; x < validArea.m_Area.y; x++)
                    {
                        int idx = z * block.m_Size.x + x;
                        Cell cell = cells[idx];
                        if (cell.m_Zone.m_Index != ZoneType.None.m_Index)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }
    }
}
