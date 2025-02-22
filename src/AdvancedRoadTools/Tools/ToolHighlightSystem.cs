using System;
using Game;
using Game.Common;
using Game.Net;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace AdvancedRoadTools.Tools;

public partial class ToolHighlightSystem : GameSystemBase
{
    private ComponentLookup<Edge> edgeLookup;
    private ComponentLookup<Highlighted> highlightedLookup;
    private EntityCommandBuffer ecb;
    private ToolOutputBarrier toolOutputBarrier;

    private NativeList<Entity> toToggleEntities;
    public NativeArray<Entity>.ReadOnly ToToggleEntities => toToggleEntities.AsReadOnly();

    protected override void OnCreate()
    {
        toolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
        
        edgeLookup = GetComponentLookup<Edge>(true);
        highlightedLookup = GetComponentLookup<Highlighted>(true);

        toToggleEntities = new NativeList<Entity>(Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        if(toToggleEntities.IsEmpty) return;
        
        ecb = toolOutputBarrier.CreateCommandBuffer();
        edgeLookup.Update(this);
        highlightedLookup.Update(this);
        
        log.Debug($"({System.Reflection.MethodBase.GetCurrentMethod()?.Name}) Toggling {toToggleEntities.Length} entities");

        var setHighlightedJob = new SetHighlightJob
        {
            EntitiesToToggle = toToggleEntities.AsReadOnly(),
            HighlightedLookup = highlightedLookup,
            ECB = ecb.AsParallelWriter(),
            EdgeLookup = edgeLookup
        }.Schedule(toToggleEntities.Length, 32, Dependency);
        
        
        Dependency = JobHandle.CombineDependencies(Dependency, setHighlightedJob);
        toolOutputBarrier.AddJobHandleForProducer(Dependency);
        Dependency.Complete();
        
        toToggleEntities.Clear();
    }

    public void HighlightEntity(Entity entity, bool value)
    {
        if (highlightedLookup.HasComponent(entity) == value) return;
        
        toToggleEntities.Add(entity);
    }

    private struct SetHighlightJob : IJobParallelFor
    {
        public NativeArray<Entity>.ReadOnly EntitiesToToggle;
        [ReadOnly] public ComponentLookup<Highlighted> HighlightedLookup;
        [ReadOnly] public ComponentLookup<Edge> EdgeLookup;
        public EntityCommandBuffer.ParallelWriter ECB;

        public void Execute(int index)
        {
            var entity = EntitiesToToggle[index];
            if (entity == Entity.Null) log.Error($"Trying to set highlights on a null entity!");

            switch (HighlightedLookup.HasComponent(entity))
            {
                case true:
                    ECB.RemoveComponent<Highlighted>(index, entity);
                    ECB.RemoveComponent<BatchesUpdated>(index, entity);
                    log.Debug($"Removed highlight from {entity}");
                    break;
                case false:
                    ECB.AddComponent<Highlighted>(index, entity);
                    ECB.AddComponent<BatchesUpdated>(index, entity);
                    log.Debug($"Added highlight to {entity}");
                    break;
            }

            if (EdgeLookup.TryGetComponent(entity, out var edge))
            {
                ECB.AddComponent<Updated>(index, edge.m_Start);
                ECB.AddComponent<Updated>(index, edge.m_End);
                log.Debug($"Updated edges of {entity}");
            }
        }
    }
}