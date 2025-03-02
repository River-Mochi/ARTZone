using AdvancedRoadTools.Components;
using AdvancedRoadTools.Tools;
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

public partial class SyncCreatedRoadsSystem : GameSystemBase
{
    private EntityQuery createdRoadsQuery;
    private ModificationBarrier4 _modificationBarrier;
    private ZoningControllerToolUISystem _UISystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        createdRoadsQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Updated, Temp, Road, SubBlock>()
            .WithNone<Applied>()
            .Build(this);

        _modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4>();
        _UISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
    }

    protected override void OnUpdate()
    {
        if (createdRoadsQuery.IsEmpty) return;

        var ECB = _modificationBarrier.CreateCommandBuffer();

        var createdRoads = createdRoadsQuery.ToEntityArray(Allocator.TempJob);

        var syncCreatedRoadsJob = new SyncCreatedRoadsJob
        {
            CreatedRoads = createdRoads.AsReadOnly(),
            ECB = ECB.AsParallelWriter(),
            Depths = new int2(_UISystem.DepthLeft, _UISystem.DepthRight),
            TempLookup = GetComponentLookup<Temp>(true),
            AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true)
        }.Schedule(createdRoads.Length, 32, this.Dependency);
        this.Dependency = JobHandle.CombineDependencies(this.Dependency, syncCreatedRoadsJob);

        _modificationBarrier.AddJobHandleForProducer(this.Dependency);
    }

    public partial struct SyncCreatedRoadsJob : IJobParallelFor
    {
        public NativeArray<Entity>.ReadOnly CreatedRoads;
        public EntityCommandBuffer.ParallelWriter ECB;
        public int2 Depths;

        [ReadOnly] public ComponentLookup<Temp> TempLookup;
        [ReadOnly] public ComponentLookup<AdvancedRoad> AdvancedRoadLookup;

        public void Execute(int index)
        {
            var createdRoadEntity = CreatedRoads[index];

            if (TempLookup.TryGetComponent(createdRoadEntity, out var temp) && temp.m_Original != Entity.Null)
            {
                if (AdvancedRoadLookup.TryGetComponent(temp.m_Original, out var advancedRoad))
                {
                    Depths = advancedRoad.Depths;
                }
                else
                {
                    Depths = new int2(6);
                }
            }

            if (math.all(Depths == new int2(6))) return;

            ECB.AddComponent(index, createdRoadEntity, new AdvancedRoad
            {
                Depths = Depths
            });
        }
    }
}