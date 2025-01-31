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
    private ZoningControllerToolUISystem _zoningControllerToolUISystem;

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
        _zoningControllerToolUISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
    }


    protected override void OnUpdate()
    {
        var job = new UpdateRoadBlocksJob
        {
            BlockTypeHandle = GetComponentTypeHandle<Block>(false),
            AdvancedDataTypeHandle = GetComponentTypeHandle<AdvancedBlockData>(false)
        };

        this.Dependency = job.ScheduleParallel(updateRoadTilingQuery, Dependency);
    }
    
    public partial struct UpdateRoadBlocksJob : IJobChunk
    {
        public ComponentTypeHandle<Block> BlockTypeHandle;
        public ComponentTypeHandle<AdvancedBlockData> AdvancedDataTypeHandle;

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
                
                
                var left = (math.dot(1, block.m_Direction) < 0);
                
                var cellOffset = new float3(block.m_Direction.x, 0, block.m_Direction.y) * ZoneUtils.CELL_SIZE;
                cellOffset *= (6 - (left ?  advancedData.depthLeft : advancedData.depthRight))/2f;

                blocks[i] = new Block
                {
                    m_Position = advancedData.originalPosition + cellOffset ,
                    m_Direction = block.m_Direction,
                    m_Size = new int2(block.m_Size.x, left ?  advancedData.depthLeft : advancedData.depthRight)
                };
            }
        }
    }
}