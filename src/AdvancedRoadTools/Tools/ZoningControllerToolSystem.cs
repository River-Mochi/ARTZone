// File: src/AdvancedRoadTools/Tools/ZoningControllerToolSystem.cs
// Purpose: Tool logic; poll ProxyAction.enabled + WasPressedThisFrame() for invert. Null-safe guards to avoid NREs.

#nullable enable

namespace AdvancedRoadTools.Tools
{
    using System;
    using AdvancedRoadTools.Components;
    using Game.Audio;
    using Game.Common;
    using Game.Net;
    using Game.Prefabs;
    using Game.Tools;
    using Game.Zones;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;

    public partial class ZoningControllerToolSystem : ToolBaseSystem
    {
        public const string ToolID = "Zone Controller Tool";
        public override string toolID => ToolID;

        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private ZoningControllerToolUISystem m_ZoningControllerToolUISystem = null!;
        private ToolHighlightSystem m_ToolHighlightSystem = null!;

        private ComponentLookup<AdvancedRoad> m_AdvancedRoadLookup;
        private BufferLookup<SubBlock> m_SubBlockLookup;

        private EntityQuery m_TempZoningQuery;
        private EntityQuery m_SoundbankQuery;

        private PrefabBase m_ToolPrefab = null!;
        private NativeList<Entity> m_SelectedEntities;

        private enum Mode
        {
            None, Select, Apply, Cancel, Preview
        }
        private Mode m_Mode;
        private Entity m_PreviewEntity;

        private int2 Depths =>
            m_ZoningControllerToolUISystem != null ? m_ZoningControllerToolUISystem.ToolDepths : new int2(6);

        protected override void OnCreate()
        {
            AdvancedRoadToolsMod.s_Log.Info("[ART] ZoningControllerToolSystem.OnCreate");
            base.OnCreate();

            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_ZoningControllerToolUISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_ToolHighlightSystem = World.GetOrCreateSystemManaged<ToolHighlightSystem>();

            m_TempZoningQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<TempZoning>().Build(this);
            m_SoundbankQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<ToolUXSoundSettingsData>().Build(this);

            m_AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true);
            m_SubBlockLookup = GetBufferLookup<SubBlock>(true);

            m_SelectedEntities = new NativeList<Entity>(Allocator.Persistent);

            // Register the tool with an explicit UI icon via ToolDefinition.UI().
            // ToolDefinition.UI()’s default now uses the fixed COUI path:
            // "coui://AdvancedRoadTools/UI/images/Tool_Icon/ToolsIcon.png"
            ToolDefinition def = new ToolDefinition(
                typeof(ZoningControllerToolSystem),
                toolID,
                59,
                new ToolDefinition.UI()
            );
            ToolsHelper.RegisterTool(def);
        }

        protected override void OnDestroy()
        {
            if (m_SelectedEntities.IsCreated)
                m_SelectedEntities.Dispose();
            base.OnDestroy();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            requireZones = true;
            requireNet = Layer.Road;
            allowUnderground = true;
        }

        protected override void OnStopRunning()
        {
            base.OnStopRunning();
            requireZones = false;
            requireNet = Layer.None;
            allowUnderground = false;
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (m_ToolSystem == null || m_ToolOutputBarrier == null)
                return inputDeps;

            m_AdvancedRoadLookup.Update(this);
            m_SubBlockLookup.Update(this);

            inputDeps = Dependency;

            // Safe raycast
            bool hasHit;
            Entity hitEntity;
            RaycastHit hit;
            try
            {
                hasHit = GetRaycastResult(out hitEntity, out hit);
            }
            catch
            {
                hasHit = false;
                hitEntity = Entity.Null;
            }

            bool haveSoundbank = m_SoundbankQuery.CalculateEntityCount() > 0;
            ToolUXSoundSettingsData soundbank = default;
            if (haveSoundbank)
                soundbank = m_SoundbankQuery.GetSingleton<ToolUXSoundSettingsData>();

            // Primary tool input → mode
            if (cancelAction.WasPressedThisFrame())
                m_Mode = Mode.Cancel;
            else if (applyAction.WasPressedThisFrame() || applyAction.IsPressed())
                m_Mode = Mode.Select;
            else if (applyAction.WasReleasedThisFrame() && hasHit)
                m_Mode = Mode.Apply;
            else if (applyAction.WasReleasedThisFrame() && !hasHit)
                m_Mode = Mode.Cancel;
            else
                m_Mode = Mode.Preview;

            EntityCommandBuffer ecb = m_ToolOutputBarrier.CreateCommandBuffer();

            switch (m_Mode)
            {
                case Mode.Preview:
                    if (m_PreviewEntity != hitEntity)
                    {
                        if (m_PreviewEntity != Entity.Null)
                            m_ToolHighlightSystem?.HighlightEntity(m_PreviewEntity, false);

                        m_SelectedEntities.Clear();
                        m_PreviewEntity = Entity.Null;

                        if (hasHit)
                        {
                            m_ToolHighlightSystem?.HighlightEntity(hitEntity, true);
                            m_SelectedEntities.Add(hitEntity);
                            m_PreviewEntity = hitEntity;
                        }
                    }
                    break;

                case Mode.Select when hasHit:
                    if (!m_SelectedEntities.Contains(hitEntity))
                    {
                        m_SelectedEntities.Add(hitEntity);
                        m_ToolHighlightSystem?.HighlightEntity(hitEntity, true);
                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_SelectEntitySound);
                    }
                    break;

                case Mode.Apply:
                    {
                        JobHandle setJob = new SetAdvancedRoadJob
                        {
                            TempZoningLookup = GetComponentLookup<TempZoning>(true),
                            Entities = m_SelectedEntities.AsArray().AsReadOnly(),
                            ECB = ecb
                        }.Schedule(inputDeps);

                        inputDeps = JobHandle.CombineDependencies(inputDeps, setJob);

                        for (int i = 0; i < m_SelectedEntities.Length; i++)
                            m_ToolHighlightSystem?.HighlightEntity(m_SelectedEntities[i], false);
                        m_SelectedEntities.Clear();

                        if (haveSoundbank)
                            AudioManager.instance.PlayUISound(soundbank.m_NetBuildSound);
                        break;
                    }

                case Mode.Cancel:
                    for (int i = 0; i < m_SelectedEntities.Length; i++)
                        m_ToolHighlightSystem?.HighlightEntity(m_SelectedEntities[i], false);
                    m_SelectedEntities.Clear();
                    if (haveSoundbank)
                        AudioManager.instance.PlayUISound(soundbank.m_NetCancelSound);
                    break;
            }

            // Invert (poll Colossal API directly)
            Game.Input.ProxyAction? invert = AdvancedRoadToolsMod.m_InvertZoningAction;
            if (invert != null && invert.enabled && invert.WasPressedThisFrame())
            {
                m_ZoningControllerToolUISystem?.InvertZoningMode();
            }

            // Temp zoning sync/cleanup
            ComponentLookup<TempZoning> tempLookup = GetComponentLookup<TempZoning>(true);

            JobHandle syncTempJob = new SyncTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                TempZoningLookup = tempLookup,
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                Depths = Depths
            }.Schedule(m_SelectedEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, syncTempJob);

            NativeArray<Entity> tempZoningEntities = m_TempZoningQuery.ToEntityArray(Allocator.TempJob);

            JobHandle cleanupTempJob = new CleanupTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                Entities = tempZoningEntities.AsReadOnly()
            }.Schedule(tempZoningEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, cleanupTempJob);
            tempZoningEntities.Dispose(inputDeps);

            m_ToolOutputBarrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }

        private new bool GetRaycastResult(out Entity entity, out RaycastHit hit)
        {
            if (!base.GetRaycastResult(out entity, out hit))
                return false;

            bool hasAdvancedRoad = m_AdvancedRoadLookup.TryGetComponent(entity, out AdvancedRoad data);
            bool hasSubBlock = m_SubBlockLookup.TryGetBuffer(entity, out _);

            if (!hasSubBlock)
            {
                entity = Entity.Null;
                return false;
            }

            if (hasAdvancedRoad)
            {
                if (math.any(Depths != data.Depths))
                    return true;
            }
            else
            {
                if (math.any(Depths != new int2(6)))
                    return true;
            }

            entity = Entity.Null;
            return false;
        }

        public override PrefabBase GetPrefab() => m_ToolPrefab;

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            if (prefab == null || prefab.name != toolID)
                return false;

            AdvancedRoadToolsMod.s_Log.Info("[ART] Tool selected: " + toolID);
            m_ToolPrefab = prefab;
            return true;
        }

        public override void InitializeRaycast()
        {
            base.InitializeRaycast();
            m_ToolRaycastSystem.typeMask = TypeMask.Net;
            m_ToolRaycastSystem.netLayerMask = Layer.Road;
        }

        public void SetToolEnabled(bool isEnabled)
        {
            if (m_ToolSystem == null)
                return;

            if (isEnabled && m_ToolSystem.activeTool != this)
                m_ToolSystem.ActivatePrefabTool(GetPrefab());
            else if (!isEnabled && m_ToolSystem.activeTool == this)
                m_ToolSystem.ActivatePrefabTool(null);
        }

        // ===== Jobs =====

        public struct SyncTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public ComponentLookup<TempZoning> TempZoningLookup;
            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public int2 Depths;

            public void Execute(int index)
            {
                Entity e = SelectedEntities[index];

                if (TempZoningLookup.TryGetComponent(e, out TempZoning data) && math.any(data.Depths != Depths))
                    ECB.SetComponent(index, e, new TempZoning { Depths = Depths });
                else
                    ECB.AddComponent(index, e, new TempZoning { Depths = Depths });

                ECB.AddComponent<Updated>(index, e);
            }
        }

        public struct CleanupTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public NativeArray<Entity>.ReadOnly Entities;

            public void Execute(int index)
            {
                Entity e = Entities[index];
                if (SelectedEntities.Contains(e))
                    return;

                ECB.RemoveComponent<TempZoning>(index, e);
                ECB.AddComponent<Updated>(index, e);
            }
        }

        public struct SetAdvancedRoadJob : IJob
        {
            public NativeArray<Entity>.ReadOnly Entities;
            public ComponentLookup<TempZoning> TempZoningLookup;
            public EntityCommandBuffer ECB;

            public void Execute()
            {
                foreach (Entity e in Entities)
                {
                    if (!TempZoningLookup.TryGetComponent(e, out TempZoning temp))
                        continue;

                    ECB.RemoveComponent<TempZoning>(e);
                    ECB.AddComponent(e, new AdvancedRoad { Depths = temp.Depths });
                }
            }
        }
    }
}
