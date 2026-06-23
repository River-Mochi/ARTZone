// <copyright file="ToolHighlightSystem.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: src/Tools/ToolHighlightSystem.cs
// Purpose: Applies vanilla road highlight (blue outline) by toggling Highlighted on net entities.

namespace EasyZoning.Tools
{
    using Game;                     // GameSystemBase
    using Game.Common;              // Updated, BatchesUpdated
    using Game.Net;                 // Edge (for endpoints)
    using Game.Tools;               // ToolOutputBarrier
    using Unity.Collections;        // NativeList, NativeArray
    using Unity.Entities;           // ComponentLookup, ECB, Entity
    using Unity.Jobs;               // IJobParallelFor, JobHandle

    public partial class ToolHighlightSystem : GameSystemBase
    {
        private ToolOutputBarrier m_ToolOutputBarrier = null!;

        private ComponentLookup<Edge> m_EdgeLookup;
        private ComponentLookup<Highlighted> m_HighlightedLookup;

        private NativeList<Entity> m_ToHighlight;
        private NativeList<Entity> m_ToUnhighlight;

        protected override void OnCreate( )
        {
            base.OnCreate();

            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();

            m_EdgeLookup = GetComponentLookup<Edge>(isReadOnly: true);
            m_HighlightedLookup = GetComponentLookup<Highlighted>(isReadOnly: true);

            m_ToHighlight = new NativeList<Entity>(Allocator.Persistent);
            m_ToUnhighlight = new NativeList<Entity>(Allocator.Persistent);
        }

        protected override void OnDestroy( )
        {
            if (m_ToHighlight.IsCreated)
                m_ToHighlight.Dispose();
            if (m_ToUnhighlight.IsCreated)
                m_ToUnhighlight.Dispose();

            base.OnDestroy();
        }

        protected override void OnUpdate( )
        {
            EntityCommandBuffer ecb = m_ToolOutputBarrier.CreateCommandBuffer();

            m_EdgeLookup.Update(this);
            m_HighlightedLookup.Update(this);

            JobHandle deps = Dependency;

            if (!m_ToHighlight.IsEmpty)
            {
                JobHandle job = new HighlightJob
                {
                    Entities = m_ToHighlight.AsReadOnly(),
                    HighlightedLookup = m_HighlightedLookup,
                    EdgeLookup = m_EdgeLookup,
                    ECB = ecb.AsParallelWriter(),
                }.Schedule(m_ToHighlight.Length, 32, deps);

                deps = JobHandle.CombineDependencies(deps, job);
            }

            if (!m_ToUnhighlight.IsEmpty)
            {
                JobHandle job = new UnhighlightJob
                {
                    Entities = m_ToUnhighlight.AsReadOnly(),
                    HighlightedLookup = m_HighlightedLookup,
                    EdgeLookup = m_EdgeLookup,
                    ECB = ecb.AsParallelWriter(),
                }.Schedule(m_ToUnhighlight.Length, 32, deps);

                deps = JobHandle.CombineDependencies(deps, job);
            }

            Dependency = deps;
            m_ToolOutputBarrier.AddJobHandleForProducer(Dependency);

            m_ToHighlight.Clear();
            m_ToUnhighlight.Clear();
        }

        public void HighlightEntity(Entity entity, bool value)
        {
            if (entity == Entity.Null)
                return;

            if (value)
            {
                if (!m_ToHighlight.Contains(entity))
                    m_ToHighlight.Add(entity);
            }
            else
            {
                if (!m_ToUnhighlight.Contains(entity))
                    m_ToUnhighlight.Add(entity);
            }
        }

        private struct HighlightJob : IJobParallelFor
        {
            [ReadOnly] public ComponentLookup<Edge> EdgeLookup;
            [ReadOnly] public ComponentLookup<Highlighted> HighlightedLookup;
            public NativeArray<Entity>.ReadOnly Entities;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(int index)
            {
                Entity entity = Entities[index];
                if (entity == Entity.Null)
                    return;

                if (!HighlightedLookup.HasComponent(entity))
                {
                    ECB.AddComponent<Highlighted>(index, entity);
                    ECB.AddComponent<BatchesUpdated>(index, entity);
                }

                if (EdgeLookup.TryGetComponent(entity, out Edge edge))
                {
                    ECB.AddComponent<Updated>(index, edge.m_Start);
                    ECB.AddComponent<Updated>(index, edge.m_End);
                }
            }
        }

        private struct UnhighlightJob : IJobParallelFor
        {
            [ReadOnly] public ComponentLookup<Edge> EdgeLookup;
            [ReadOnly] public ComponentLookup<Highlighted> HighlightedLookup;
            public NativeArray<Entity>.ReadOnly Entities;
            public EntityCommandBuffer.ParallelWriter ECB;

            public void Execute(int index)
            {
                Entity entity = Entities[index];
                if (entity == Entity.Null)
                    return;

                if (HighlightedLookup.HasComponent(entity))
                {
                    ECB.RemoveComponent<Highlighted>(index, entity);
                    ECB.AddComponent<BatchesUpdated>(index, entity);
                }

                if (EdgeLookup.TryGetComponent(entity, out Edge edge))
                {
                    ECB.AddComponent<Updated>(index, edge.m_Start);
                    ECB.AddComponent<Updated>(index, edge.m_End);
                }
            }
        }
    }
}
