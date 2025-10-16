// SyncCreatedRoadsSystem.cs
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

namespace AdvancedRoadTools
{
    public partial class SyncCreatedRoadsSystem : GameSystemBase
    {
        private EntityQuery NewCreatedRoadsQuery;
        private ModificationBarrier4 _modificationBarrier = null!;   // set in OnCreate
        private ZoningControllerToolUISystem _UISystem = null!;      // set in OnCreate

        protected override void OnCreate()
        {
            base.OnCreate();

            NewCreatedRoadsQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Road, Temp, SubBlock, Updated>()
                .WithAll<Created>()
                .Build(this);

            _modificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4>();
            _UISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ECB = _modificationBarrier.CreateCommandBuffer();
            int2 depths = _UISystem.RoadDepths;

            if (!NewCreatedRoadsQuery.IsEmpty && math.any(depths != new int2(6)))
            {
                NativeArray<Entity> entities = NewCreatedRoadsQuery.ToEntityArray(Allocator.TempJob);
                JobHandle job = new AddAdvancedRoadToCreatedRoadsJob
                {
                    Entities = entities.AsReadOnly(),
                    ECB = ECB.AsParallelWriter(),
                    Depths = depths,
                    TempLookup = GetComponentLookup<Temp>(true)
                }.Schedule(entities.Length, 32, this.Dependency);
                entities.Dispose(job);
                this.Dependency = JobHandle.CombineDependencies(this.Dependency, job);
            }

            _modificationBarrier.AddJobHandleForProducer(this.Dependency);
        }

        public struct AddAdvancedRoadToCreatedRoadsJob : IJobParallelFor
        {
            public NativeArray<Entity>.ReadOnly Entities;
            public EntityCommandBuffer.ParallelWriter ECB;
            [ReadOnly] public ComponentLookup<Temp> TempLookup;
            public int2 Depths;

            public void Execute(int index)
            {
                Entity entity = Entities[index];
                Temp temp = TempLookup[entity];

                if ((temp.m_Flags & TempFlags.Create) == TempFlags.Create)
                {
                    ECB.AddComponent(index, entity, new AdvancedRoad { Depths = Depths });
                }
            }
        }
    }
}
