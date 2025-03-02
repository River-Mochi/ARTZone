using System;
using AdvancedRoadTools.Components;
using AdvancedRoadTools.Logging;
using AdvancedRoadTools.Tools;
using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.Zones;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace AdvancedRoadTools;

public partial class CreateParallelRoadsSystem : GameSystemBase
{
    private EntityQuery tempParallelQuery;

    private ModificationBarrier4 _modificationBarrier;


    protected override void OnCreate()
    {
        base.OnCreate();

        tempParallelQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Road, Curve, TempParallel, PrefabRef, Updated>()
            .Build(this);

        _modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4>();
    }

    protected override void OnUpdate()
    {
        if (tempParallelQuery.IsEmpty) return;

        var ecb = _modificationBarrier.CreateCommandBuffer();

        var tempParallel = tempParallelQuery.ToEntityArray(Allocator.TempJob);

        var syncBlockJob = new HandleTempParallelJob
        {
            ECB = ecb.AsParallelWriter(),
            Entities = tempParallel.AsReadOnly(),
            TempParallelLookup = GetComponentLookup<TempParallel>(true),
            PrefabLookup = GetComponentLookup<PrefabRef>(true),
            CurveLookup = GetComponentLookup<Curve>(true),

        }.Schedule(tempParallelQuery.CalculateEntityCount(), 32, this.Dependency);
        this.Dependency = JobHandle.CombineDependencies(this.Dependency, syncBlockJob);
        
        Dependency.Complete();

        tempParallel.Dispose();

        _modificationBarrier.AddJobHandleForProducer(this.Dependency);
    }

    public struct HandleTempParallelJob : IJobParallelFor
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public NativeArray<Entity>.ReadOnly Entities;
        [ReadOnly] public ComponentLookup<TempParallel> TempParallelLookup;
        [ReadOnly] public ComponentLookup<PrefabRef> PrefabLookup;
        [ReadOnly] public ComponentLookup<Curve> CurveLookup;
        [ReadOnly] public RandomSeed RandomSeed;

        public void Execute(int index)
        {
            var entity = Entities[index];
            var temp = TempParallelLookup[entity];
            var curve = CurveLookup[entity];
            var prefab = PrefabLookup[temp.RootEntity];
            
            var newRoad = ECB.CreateEntity(index);

            var controlPoints = new NativeList<ControlPoint>(Allocator.TempJob);
            controlPoints.Add(new ControlPoint
            {
                m_Position = curve.m_Bezier.a + math.right() * 10,
            });
            controlPoints.Add(new ControlPoint
            {
                m_Position = curve.m_Bezier.d + math.right() * 10,
            });
            
            Random random = RandomSeed.GetRandom(0);


            CreationDefinition definition = new();
            definition.m_Prefab = prefab.m_Prefab;
            definition.m_RandomSeed = random.NextInt();
            
            ECB.AddComponent(index, newRoad, definition);

            NetCourse netCourse = new();

            netCourse.m_Curve = NetUtils.StraightCurve(controlPoints[0].m_Position, controlPoints[1].m_Position);
            
            ECB.AddComponent(index, newRoad, netCourse);
            ECB.AddComponent<Updated>(index, newRoad);
        }
    }
}