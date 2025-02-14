using Game;
using Game.Common;
using Game.Net;
using Game.Zones;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AdvancedRoadTools.Core;

public partial class SyncCreatedRoadsSystem : GameSystemBase
{
    private EntityQuery createdRoadsQuery;
    private ModificationBarrier4 _modificationBarrier;

    protected override void OnCreate()
    {
        base.OnCreate();

        createdRoadsQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Updated, Created, Road, SubBlock>()
            .Build(this);
        
        _modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4>();
    }

    protected override void OnUpdate()
    {
        if(createdRoadsQuery.IsEmpty) return;
        
        var ECB = _modificationBarrier.CreateCommandBuffer();

        var createdRoads = createdRoadsQuery.ToEntityArray(Allocator.TempJob);

        var syncCreatedRoadsJob = new SyncCreatedRoadsJob
        {
            CreatedRoads = createdRoads.AsReadOnly(),
            ECB = ECB.AsParallelWriter()
        }.Schedule(createdRoads.Length, 32, this.Dependency);
        this.Dependency = JobHandle.CombineDependencies(this.Dependency, syncCreatedRoadsJob);
        
        _modificationBarrier.AddJobHandleForProducer(this.Dependency);
    }

    public partial struct SyncCreatedRoadsJob : IJobParallelFor
    {
        public NativeArray<Entity>.ReadOnly CreatedRoads;
        public EntityCommandBuffer.ParallelWriter ECB;
        
        public void Execute(int index)
        {
            var createdRoadEntity = CreatedRoads[index];
        }
    }
}