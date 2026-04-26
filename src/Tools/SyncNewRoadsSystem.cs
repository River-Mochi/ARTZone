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
    using Game.Net;                 // Road, Upgraded
    using Game.Prefabs;             // PrefabBase, RoadPrefab
    using Game.Tools;               // NetToolSystem, Temp, ToolSystem
    using Game.Zones;               // SubBlock
    using Unity.Collections;        // NativeArray
    using Unity.Entities;           // EntityQuery, ComponentLookup, ECB
    using Unity.Jobs;               // IJobParallelFor, JobHandle
    using Unity.Mathematics;        // int2, math.any/all

    public partial class SyncNewRoadsSystem : GameSystemBase
    {
        private static readonly int2 kVanillaDepths = RoadZoneCompatibility.VanillaDepths;

        private EntityQuery m_NewCreatedRoadsQuery;
        private ModificationBarrier4 m_ModificationBarrier = null!;
        private ZoneControlBridgeUI m_UISystem = null!;
        private ToolSystem m_ToolSystem = null!;

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
            m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
        }

        protected override void OnUpdate( )
        {
            if (m_UISystem == null || m_ToolSystem == null)
                return;

            if (!IsZonableRoadBuildToolActive())
                return;

            // For newly created roads, follow the vanilla road-tool state (RoadZoningMode),
            // not the Easy Zoning update-tool state.
            int2 depths = m_UISystem.RoadDepths;

            if (m_NewCreatedRoadsQuery.IsEmpty)
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
                UpgradedLookup = GetComponentLookup<Upgraded>(isReadOnly: true),
                ZoningDepthLookup = GetComponentLookup<ZoningDepthComponent>(isReadOnly: true),
            }.Schedule(entities.Length, 32, Dependency);

            entities.Dispose(job);

            Dependency = JobHandle.CombineDependencies(Dependency, job);
            m_ModificationBarrier.AddJobHandleForProducer(Dependency);
        }

        private bool IsZonableRoadBuildToolActive( )
        {
            if (m_ToolSystem.activeTool is not NetToolSystem netTool)
                return false;

            return IsZonableRoadPrefab(netTool.GetPrefab());
        }

        private static bool IsZonableRoadPrefab(PrefabBase? prefab)
        {
            if (prefab is not RoadPrefab road)
                return false;

            if (road.m_ZoneBlock == null)
                return false;

            if (road.m_HighwayRules)
                return false;

            return true;
        }

        public struct AddOrSetZoningDepthToCreatedRoadsJob : IJobParallelFor
        {
            public NativeArray<Entity>.ReadOnly Entities;
            public EntityCommandBuffer.ParallelWriter ECB;

            [ReadOnly] public ComponentLookup<Temp> TempLookup;
            [ReadOnly] public ComponentLookup<Upgraded> UpgradedLookup;
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

                bool useVanillaDepths = math.all(Depths == kVanillaDepths);

                // Created entities can persist across frames; keep the temp state in sync
                // even when the user switches back to vanilla Both during placement.
                if (ZoningDepthLookup.TryGetComponent(entity, out ZoningDepthComponent existing))
                {
                    if (useVanillaDepths)
                    {
                        ECB.RemoveComponent<ZoningDepthComponent>(index, entity);
                    }
                    else if (!math.all(existing.Depths == Depths))
                    {
                        ECB.SetComponent(index, entity, new ZoningDepthComponent { Depths = Depths });
                    }
                }
                else if (!useVanillaDepths)
                {
                    ECB.AddComponent(index, entity, new ZoningDepthComponent { Depths = Depths });
                }

                bool hasUpgraded = UpgradedLookup.TryGetComponent(entity, out Upgraded upgraded);
                CompositionFlags nextFlags = RoadZoneCompatibility.ApplyDepthsToFlags(
                    hasUpgraded ? upgraded.m_Flags : default,
                    Depths);

                if (RoadZoneCompatibility.HasAnyFlags(nextFlags))
                {
                    Upgraded nextUpgraded = new Upgraded { m_Flags = nextFlags };
                    if (hasUpgraded)
                    {
                        if (upgraded.m_Flags != nextFlags)
                            ECB.SetComponent(index, entity, nextUpgraded);
                    }
                    else
                    {
                        ECB.AddComponent(index, entity, nextUpgraded);
                    }
                }
                else if (hasUpgraded)
                {
                    ECB.RemoveComponent<Upgraded>(index, entity);
                }
            }
        }
    }
}
