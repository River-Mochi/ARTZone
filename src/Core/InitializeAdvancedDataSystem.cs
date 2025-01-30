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
using Block = Game.Zones.Block;

namespace AdvancedRoadTools.Core;

public partial class InitializeAdvancedDataSystem : GameSystemBase
{
    public EntityQuery uninitializedRoadsQuery;
    private ModificationBarrier4B _modification4B;

    protected override void OnCreate()
    {
        uninitializedRoadsQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Block>()
            .WithNone<AdvancedBlockData>()
            .Build(this);


        this.RequireAnyForUpdate(this.uninitializedRoadsQuery);
        _modification4B = World.GetOrCreateSystemManaged<ModificationBarrier4B>();
    }


    protected override void OnUpdate()
    {
        var ecb = _modification4B.CreateCommandBuffer();

        this.Dependency = new InitializeAdvancedBlock()
            {
                ecb = ecb,
                EntityTypeHandle = GetEntityTypeHandle(),
                BlockTypeHandle = GetComponentTypeHandle<Block>(true),
                LastSystemVersion = LastSystemVersion
            }
            .Schedule(uninitializedRoadsQuery, Dependency);

        this._modification4B.AddJobHandleForProducer(this.Dependency);
    }


    public partial struct InitializeAdvancedBlock : IJobChunk
    {
        public EntityTypeHandle EntityTypeHandle;
        [ReadOnly] public ComponentTypeHandle<Block> BlockTypeHandle;
        public uint LastSystemVersion;

        public EntityCommandBuffer ecb;

        //TODO: add default values
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(EntityTypeHandle);
            var blocks = chunk.GetNativeArray(ref BlockTypeHandle);
            var enumerator = new ChunkEntityEnumerator(useEnabledMask, chunkEnabledMask, chunk.Count);

            AdvancedRoadToolsMod.log.Info($"Adding advanced roads to {chunk.Count} entities.");
            while (enumerator.NextEntityIndex(out var i))
            {
                ecb.AddComponent<AdvancedBlockData>(entities[i], new()
                {
                    originalPosition = blocks[i].m_Position,
                    offset = default,
                    TillingMode = Setting.TillingModes.Both
                });
            }
        }
    }
}