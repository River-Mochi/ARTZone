using AdvancedRoadTools.Components;
using AdvancedRoadTools.Tools;
using Colossal.Entities;
using Game;
using Game.Common;
using Game.Net;
using Game.Tools;
using Game.Zones;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace AdvancedRoadTools;

public partial class SyncUpgradedRoadsSystem : GameSystemBase
{
    private EntityQuery upgradedRoadsQuery;
    private ModificationBarrier4 modificationBarrier;
    private ZoningControllerToolUISystem uiSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        upgradedRoadsQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Road, SubBlock, Updated, Temp>()
            .Build(this);

        modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4>();
        uiSystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = modificationBarrier.CreateCommandBuffer();

        var depths = uiSystem.RoadDepths;

        if (!upgradedRoadsQuery.IsEmpty)
        {
            var entities = upgradedRoadsQuery.ToEntityArray(Allocator.TempJob);
            var temps = upgradedRoadsQuery.ToComponentDataArray<Temp>(Allocator.TempJob);
            var job = new TempZonedUpgrade
            {
                Entities = entities.AsReadOnly(),
                Temps = temps.AsReadOnly(),
                ECB = ecb.AsParallelWriter(),
                Depths = depths,
            }.Schedule(entities.Length, 32, this.Dependency);
            entities.Dispose(job);
            temps.Dispose(job);
            this.Dependency = JobHandle.CombineDependencies(this.Dependency, job);
        }

        modificationBarrier.AddJobHandleForProducer(this.Dependency);
    }

    public struct TempZonedUpgrade : IJobParallelFor
    {
        public NativeArray<Entity>.ReadOnly Entities;
        public NativeArray<Temp>.ReadOnly Temps;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int2 Depths;
        
        public void Execute(int index)
        {
            var entity = Entities[index];
            var temp = Temps[index];
            
            if((temp.m_Flags & TempFlags.Modify) != TempFlags.Modify) return;
            
            return;
        }
    }
}