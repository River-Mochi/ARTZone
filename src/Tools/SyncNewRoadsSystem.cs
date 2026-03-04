// File: src/Tools/SyncNewRoadsSystem.cs
// Purpose: Adds or updates ZoningDepthComponent on NEW created roads using current UI depths.
// Notes:
// - Without this, freshly drawn roads won’t inherit the chosen zoning change depths.
// - Uses Add-or-Set to avoid duplicate AddComponent if Created roads persist across frames.

namespace EasyZoning.Tools
{
    using EasyZoning.Components;    // ZoningDepthComponent
    using Game;                     // GameSystemBase
    using Game.Common;              // Created, Updated
    using Game.Net;                 // Road
    using Game.Tools;               // Temp
    using Game.Zones;               // SubBlock
    using Unity.Collections;        // NativeArray
    using Unity.Entities;           // EntityQuery, ComponentLookup, ECB
    using Unity.Jobs;               // IJobParallelFor, JobHandle
    using Unity.Mathematics;        // int2, math.any/all

    public partial class SyncNewRoadsSystem : GameSystemBase
    {
        private static readonly int2 kVanillaDepths = new int2(6, 6);

        private EntityQuery m_NewCreatedRoadsQuery;
        private ModificationBarrier4 m_ModificationBarrier = null!;
        private ZoneControlBridgeUI m_UISystem = null!;

#if DEBUG
        private static void Dbg(string msg)
        {
            Colossal.Logging.ILog log = Mod.s_Log;
            if (log == null)
                return;
            try
            {
                log.Info("[EZ][SyncCreated] " + msg);
            }
            catch { }
        }
#else
        private static void Dbg(string msg)
        {
        }
#endif

        protected override void OnCreate( )
        {
            base.OnCreate();

            m_NewCreatedRoadsQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Road, Temp, SubBlock, Updated>()
                .WithAll<Created>()
                .Build(this);

            m_ModificationBarrier = World.GetOrCreateSystemManaged<ModificationBarrier4>();
            m_UISystem = World.GetOrCreateSystemManaged<ZoneControlBridgeUI>();
        }

        protected override void OnUpdate( )
        {
            if (m_UISystem == null)
                return;

            // For newly created roads, follow the vanilla road-tool state (RoadZoningMode),
            // not the Easy Zoning update-tool state.
            int2 depths = m_UISystem.RoadDepths;

            // Skip when nothing to do or depths are vanilla default (6,6).
            if (m_NewCreatedRoadsQuery.IsEmpty || !math.any(depths != kVanillaDepths))
                return;

            EntityCommandBuffer ecb = m_ModificationBarrier.CreateCommandBuffer();
            NativeArray<Entity> entities = m_NewCreatedRoadsQuery.ToEntityArray(Allocator.TempJob);

#if DEBUG
            Dbg($"newRoads={entities.Length} depths=({depths.x},{depths.y})");
#endif

            JobHandle job = new AddOrSetZoningDepthToCreatedRoadsJob
            {
                Entities = entities.AsReadOnly(),
                ECB = ecb.AsParallelWriter(),
                Depths = depths,
                TempLookup = GetComponentLookup<Temp>(isReadOnly: true),
                ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(isReadOnly: true),
            }.Schedule(entities.Length, 32, Dependency);

            entities.Dispose(job);

            Dependency = JobHandle.CombineDependencies(Dependency, job);
            m_ModificationBarrier.AddJobHandleForProducer(Dependency);
        }

        public struct AddOrSetZoningDepthToCreatedRoadsJob : IJobParallelFor
        {
            public NativeArray<Entity>.ReadOnly Entities;
            public EntityCommandBuffer.ParallelWriter ECB;

            [ReadOnly] public ComponentLookup<Temp> TempLookup;
            [ReadOnly] public ComponentLookup<ZoningDepthComponent> ZoningDepthLookup;

            public int2 Depths;

            public void Execute(int index)
            {
                Entity entity = Entities[index];

                if (!TempLookup.HasComponent(entity))
                    return; // Temp removed mid-frame

                Temp temp = TempLookup[entity];

                if ((temp.m_Flags & TempFlags.Create) != TempFlags.Create)
                    return;

                // Created entities can persist across frames; avoid duplicate AddComponent.
                if (ZoningDepthLookup.TryGetComponent(entity, out ZoningDepthComponent existing))
                {
                    if (!math.all(existing.Depths == Depths))
                    {
                        ECB.SetComponent(index, entity, new ZoningDepthComponent { Depths = Depths });
                    }

                    return;
                }

                ECB.AddComponent(index, entity, new ZoningDepthComponent { Depths = Depths });
            }
        }
    }
}
