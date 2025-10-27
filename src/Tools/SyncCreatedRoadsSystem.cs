// File: src/Tools/SyncCreatedRoadsSystem.cs
// Purpose: adds ZoningDepth component to NEW created roads using the current RoadDepths from UI.
// Without this, freshly drawn roads won’t inherit the chosen zoning side depths.

namespace ARTZone.Tools
{
    using ARTZone.Components;
    using Game;
    using Game.Common;
    using Game.Net;
    using Game.Tools;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    public partial class SyncCreatedRoadsSystem : GameSystemBase
    {
        private EntityQuery m_NewCreatedRoadsQuery;
        private ModificationBarrier4 m_ModificationBarrier = null!;   // set in OnCreate
        private ZoningControllerToolUISystem m_UISystem = null!;      // set in OnCreate

#if DEBUG
        private static void Dbg(string msg)
        {
            var log = ARTZoneMod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info("[ART][SyncCreated] " + msg);
            }
            catch { /* never crash on logging */ }
        }
#else
        private static void Dbg(string msg) { }
#endif

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
            // UISystem is created in OnCreate, but be defensive.
            if (m_UISystem == null)
                return;

            int2 depths = m_UISystem.RoadDepths;

            // Only act when there are brand new roads AND the chosen depth is not the vanilla default (6,6).
            if (m_NewCreatedRoadsQuery.IsEmpty || !math.any(depths != new int2(6)))
                return;

            var ecb = m_ModificationBarrier.CreateCommandBuffer();
            NativeArray<Entity> entities = m_NewCreatedRoadsQuery.ToEntityArray(Allocator.TempJob);

#if DEBUG
            Dbg($"newRoads={entities.Length} depths=({depths.x},{depths.y})");
#endif

            JobHandle job = new AddZoningDepthToCreatedRoadsJob
            {
                Entities = entities.AsReadOnly(),
                ECB = ecb.AsParallelWriter(),
                Depths = depths,
                TempLookup = GetComponentLookup<Temp>(true)
            }.Schedule(entities.Length, 32, Dependency);

            entities.Dispose(job);

            Dependency = JobHandle.CombineDependencies(Dependency, job);
            m_ModificationBarrier.AddJobHandleForProducer(Dependency);
        }

        public struct AddZoningDepthToCreatedRoadsJob : IJobParallelFor
        {
            public NativeArray<Entity>.ReadOnly Entities;
            public EntityCommandBuffer.ParallelWriter ECB;

            [ReadOnly] public ComponentLookup<Temp> TempLookup;
            public int2 Depths;

            public void Execute(int index)
            {
                Entity entity = Entities[index];

                // If Temp was removed mid-frame, skip quietly.
                if (!TempLookup.HasComponent(entity))
                    return;

                Temp temp = TempLookup[entity];

                if ((temp.m_Flags & TempFlags.Create) == TempFlags.Create)
                {
                    ECB.AddComponent(index, entity, new ZoningDepthComponent { Depths = Depths });
                }
            }
        }
    }
}
