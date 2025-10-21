// src/Tools/SyncCreatedRoadsSystem.cs
// Purpose: adds AdvancedRoad component to newly created roads using the current RoadDepths from UI.
// Without this, freshly drawn roads won’t inherit the chosen zoning side depths.
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
        private EntityQuery m_NewCreatedRoadsQuery;
        private ModificationBarrier4 m_ModificationBarrier = null!;   // set in OnCreate
        private ZoningControllerToolUISystem m_UISystem = null!;      // set in OnCreate

        protected override void OnCreate()
        {
            base.OnCreate();

            m_NewCreatedRoadsQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Road, Temp, SubBlock, Updated>()
                .WithAll<Created>()
                .Build(this);

            m_ModificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4>();
            m_UISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer ECB = m_ModificationBarrier.CreateCommandBuffer();
            int2 depths = m_UISystem.RoadDepths;

            if (!m_NewCreatedRoadsQuery.IsEmpty && math.any(depths != new int2(6)))
            {
                NativeArray<Entity> entities = m_NewCreatedRoadsQuery.ToEntityArray(Allocator.TempJob);
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

            m_ModificationBarrier.AddJobHandleForProducer(this.Dependency);
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
