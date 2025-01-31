using AdvancedRoadTools.Core.Logging;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AdvancedRoadTools.Core;

[BurstCompile]
public partial class ZoningControllerToolSystem : ToolBaseSystem
{    
    public EntityQuery edgesQuery;
    private ModificationBarrier4B _modification4B;
    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
        edgesQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Edge>()
            .WithNone<Highlighted>()
            .Build(this);
        
        this.RequireAnyForUpdate(this.edgesQuery);
        _modification4B = World.GetOrCreateSystemManaged<ModificationBarrier4B>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!GetRaycastResult(out Entity entity, out var hit)) return;
        
        var ecb = _modification4B.CreateCommandBuffer();
            
        this.Dependency = new EdgeHighlightJob
        {
            Entity = entity,
            EdgeLookup = GetComponentLookup<Edge>(true),
            ecb = ecb
        }.Schedule(this.Dependency);
            
        this._modification4B.AddJobHandleForProducer(this.Dependency);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }

    public override PrefabBase GetPrefab()
    {
        return null;
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        Log.Info($"[{this.GetType().Name}] TrySetPrefab called]");
        return false;
    }

    public override void InitializeRaycast()
    {
        base.InitializeRaycast();

        this.m_ToolRaycastSystem.typeMask = TypeMask.Lanes | TypeMask.Net;
        this.m_ToolRaycastSystem.netLayerMask = Layer.Road;
        this.m_ToolRaycastSystem.areaTypeMask = Game.Areas.AreaTypeMask.Surfaces;
    }

    public override string toolID => "Zoning Controller";

    public partial struct EdgeHighlightJob : IJob
    {
        public Entity Entity;
        public ComponentLookup<Edge> EdgeLookup;
        public EntityCommandBuffer ecb;

        
        public void Execute()
        {
            ecb.AddComponent<Highlighted>(EdgeLookup[Entity].m_End); 
        }
    }
    
    
}