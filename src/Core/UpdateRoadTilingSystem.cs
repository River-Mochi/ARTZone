using System.Diagnostics;
using System.Linq;
using AdvancedRoadTools.Core;
using Game.Zones;
using Colossal;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Game;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.UI.Widgets;
using Unity.Mathematics;
using Block = Game.Zones.Block;

namespace AdvancedRoadTools.Core;

public partial class UpdateRoadTilingSystem : GameSystemBase
{
    public EntityQuery updateRoadTilingQuery;

    protected override void OnCreate()
    {
        updateRoadTilingQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAllRW<Block, AdvancedBlockData>()
            .Build(this);
        
        updateRoadTilingQuery.SetChangedVersionFilter(
        [
            typeof(AdvancedBlockData),
            typeof(Block)
        ]);
        
        this.RequireAnyForUpdate(this.updateRoadTilingQuery);
    }


    protected override void OnUpdate()
    {
        var job = new UpdateRoadBlocksJob
        {
            BlockTypeHandle = GetComponentTypeHandle<Block>(false),
            AdvancedDataTypeHandle = GetComponentTypeHandle<AdvancedBlockData>(true),
            offset = new float3(AdvancedRoadToolsMod.m_Setting.OffsetX, AdvancedRoadToolsMod.m_Setting.OffsetY, AdvancedRoadToolsMod.m_Setting.OffsetZ)
        };

        this.Dependency = job.ScheduleParallel(updateRoadTilingQuery, Dependency);
    }
    
    public partial struct UpdateRoadBlocksJob : IJobChunk
    {
        public ComponentTypeHandle<Block> BlockTypeHandle;
        [ReadOnly]public ComponentTypeHandle<AdvancedBlockData> AdvancedDataTypeHandle;
        public float3 offset;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var blocks = chunk.GetNativeArray(ref BlockTypeHandle);
            var advancedDatas = chunk.GetNativeArray(ref AdvancedDataTypeHandle);

            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);
            
            AdvancedRoadToolsMod.log.Info($"Updating Road Blocks of {chunk.Count} entities.");
            
            while(enumerator.NextEntityIndex(out var i))
            {
                var block = blocks[i];
                var advancedData = advancedDatas[i];
                
                DebugBlock(block);

                var cellOffset = new float3(block.m_Direction.x, 0, block.m_Direction.y) * ZoneUtils.CELL_SIZE;
                cellOffset *= (6 - AdvancedRoadToolsMod.m_Setting.SizeY)/2f;
                //cellOffset *= math.sign(math.dot(block.m_Direction, 1));

                blocks[i] = new Block
                {
                    m_Position = advancedData.originalPosition + cellOffset ,
                    m_Direction = block.m_Direction,
                    m_Size = new int2(AdvancedRoadToolsMod.m_Setting.SizeXActive ? AdvancedRoadToolsMod.m_Setting.SizeX : block.m_Size.x,
                        AdvancedRoadToolsMod.m_Setting.SizeYActive ? AdvancedRoadToolsMod.m_Setting.SizeY : block.m_Size.y)
                        
                };
            }
        }

        private void DebugBlock(Block block)
        {
            AdvancedRoadToolsMod.log.Info($"Block data dump" +
                                          $"\nPos: {block.m_Position}\tDir: {block.m_Direction}" +
                                          $"\nSize: {block.m_Size}\t Dot: {math.dot(block.m_Direction, 1)}");
        }
    }
}