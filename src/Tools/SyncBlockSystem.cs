// src/Tools/SyncBlockSystem.cs
// Purpose: applies the preview/committed zoning depth to actual zone blocks
// respecting settings (RemoveZonedCells / RemoveOccupiedCells). Tool won’t function without it.

namespace AdvancedRoadTools
{
    using System;
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
        private ModificationBarrier4B m_ModificationBarrier = null!;

#if DEBUG
        private int m_LogTick;
        private int m_LastCount;
#endif

        protected override void OnCreate()
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

        protected override void OnUpdate()
        {
            if (m_UpdatedBlocksQuery.IsEmpty)
                return;

#if DEBUG
            // Throttle noisy logs: once per ~30 frames or when count changes
            int count = m_UpdatedBlocksQuery.CalculateEntityCount();
            m_LogTick++;
            if (count != m_LastCount || (m_LogTick % 30) == 0)
            {
                AdvancedRoadToolsMod.s_Log.Info($"[ART][SyncBlock] blocks={count} removeOcc={AdvancedRoadToolsMod.s_Settings?.RemoveOccupiedCells == true} removeZoned={AdvancedRoadToolsMod.s_Settings?.RemoveZonedCells == true}");
                m_LastCount = count;
            }
#endif

            var ecb = m_ModificationBarrier.CreateCommandBuffer();
            var updatedBlocks = m_UpdatedBlocksQuery.ToEntityArray(Allocator.TempJob);

            var syncBlockJob = new SyncBlockJob
            {
                ECB = ecb.AsParallelWriter(),
                Entities = updatedBlocks.AsReadOnly(),
                BlockLookup = GetComponentLookup<Block>(true),
                ValidAreaLookup = GetComponentLookup<ValidArea>(true),
                OwnerLookup = GetComponentLookup<Owner>(true),
                CellLookup = GetBufferLookup<Cell>(true),
                AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true),
                TempZoningLookup = GetComponentLookup<TempZoning>(true),
            }.Schedule(m_UpdatedBlocksQuery.CalculateEntityCount(), 32, this.Dependency);

            updatedBlocks.Dispose(syncBlockJob);
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, syncBlockJob);
            m_ModificationBarrier.AddJobHandleForProducer(this.Dependency);
        }

        public struct SyncBlockJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity>.ReadOnly Entities;

            [ReadOnly] public ComponentLookup<Block> BlockLookup;
            [ReadOnly] public ComponentLookup<ValidArea> ValidAreaLookup;
            [ReadOnly] public BufferLookup<Cell> CellLookup;
            [ReadOnly] public ComponentLookup<Owner> OwnerLookup;
            [ReadOnly] public ComponentLookup<AdvancedRoad> AdvancedRoadLookup;
            [ReadOnly] public ComponentLookup<TempZoning> TempZoningLookup;

            public void Execute(int index)
            {
                Entity blockEntity = Entities[index];

                // These indexers will throw if the component is missing; the query guarantees they exist.
                Block block = BlockLookup[blockEntity];
                ValidArea validArea = ValidAreaLookup[blockEntity];

                if (!OwnerLookup.TryGetComponent(blockEntity, out Owner owner))
                    throw new NullReferenceException($"Block {blockEntity} has no owner assigned.");

                Entity roadEntity = owner.m_Owner;

                bool left = (math.dot(1, block.m_Direction) < 0);

                int depth;
                if (TempZoningLookup.TryGetComponent(roadEntity, out TempZoning tempZoning))
                    depth = left ? tempZoning.Depths.x : tempZoning.Depths.y;
                else if (AdvancedRoadLookup.TryGetComponent(roadEntity, out AdvancedRoad data))
                    depth = left ? data.Depths.x : data.Depths.y;
                else
                    return;

                if (AdvancedRoadToolsMod.s_Settings != null)
                {
                    if (AdvancedRoadToolsMod.s_Settings.RemoveOccupiedCells &&
                        IsAnyCellOccupied(CellLookup[blockEntity], block, validArea))
                        return;

                    if (AdvancedRoadToolsMod.s_Settings.RemoveZonedCells &&
                        IsAnyCellZoned(CellLookup[blockEntity], block, validArea))
                        return;
                }

                block.m_Size.y = depth;
                ECB.SetComponent(index, blockEntity, block);

                validArea.m_Area.w = depth;
                ECB.SetComponent(index, blockEntity, validArea);
            }

            // Remove 'in' on non-readonly structs to silence RCS1242.
            private bool IsAnyCellOccupied(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                if (validArea.m_Area.y * validArea.m_Area.w == 0)
                    return false;

                for (int z = validArea.m_Area.z; z < validArea.m_Area.w; z++)
                {
                    for (int x = validArea.m_Area.x; x < validArea.m_Area.y; x++)
                    {
                        int idx = z * block.m_Size.x + x;
                        Cell cell = cells[idx];
                        if ((cell.m_State & CellFlags.Occupied) != 0)
                            return true;
                    }
                }
                return false;
            }

            private bool IsAnyCellZoned(DynamicBuffer<Cell> cells, Block block, ValidArea validArea)
            {
                if (validArea.m_Area.y * validArea.m_Area.w == 0)
                    return false;

                for (int z = validArea.m_Area.z; z < validArea.m_Area.w; z++)
                {
                    for (int x = validArea.m_Area.x; x < validArea.m_Area.y; x++)
                    {
                        int idx = z * block.m_Size.x + x;
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
