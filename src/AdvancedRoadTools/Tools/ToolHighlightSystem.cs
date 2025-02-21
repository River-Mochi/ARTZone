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
    }

    protected override void OnUpdate()
    {
        if(toToggleEntities.IsEmpty) return;
        
        ecb = toolOutputBarrier.CreateCommandBuffer();
        edgeLookup.Update(this);
        highlightedLookup.Update(this);
        
        log.Debug($"({System.Reflection.MethodBase.GetCurrentMethod()?.Name}) Toggling {toToggleEntities} entities");

        var setHighlightedJob = new SetHighlightJob
        {
            EntitiesToToggle = toToggleEntities.AsReadOnly(),
            HighlightedLookup = highlightedLookup,
            ECB = ecb,
            EdgeLookup = edgeLookup
        }.Schedule(toToggleEntities.Length, 32, Dependency);
        
        
        Dependency = JobHandle.CombineDependencies(Dependency, setHighlightedJob);
        Dependency.Complete();

        toToggleEntities.Dispose();
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
        public EntityCommandBuffer ECB;

        public void Execute(int index)
        {
            var entity = EntitiesToToggle[index];
            if (entity == Entity.Null) throw new NullReferenceException($"Entity {entity} is null!");

            var isHighlighted = HighlightedLookup.HasComponent(entity);

            switch (isHighlighted)
            {
                case true:
                    ECB.RemoveComponent<Highlighted>(entity);
                    ECB.RemoveComponent<BatchesUpdated>(entity);
                    break;
                case false:
                    ECB.AddComponent<Highlighted>(entity);
                    ECB.AddComponent<BatchesUpdated>(entity);
                    break;
            }

            if (EdgeLookup.TryGetComponent(entity, out var edge))
            {
                ECB.AddComponent<Updated>(edge.m_Start);
                ECB.AddComponent<Updated>(edge.m_End);
            }
        }
    }
}