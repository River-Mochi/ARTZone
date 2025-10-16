// File: src/AdvancedRoadTools/Tools/ZoningControllerToolSystem.cs
// Input fix: keep invert action as ProxyAction (no Enable/Disable), use WasPressedThisFrame().

namespace AdvancedRoadTools.Tools
{
    using System;
    using AdvancedRoadTools.Components;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Audio;
    using Game.Common;
    using Game.Input;
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
        private ToolOutputBarrier m_ToolOutputBarrier = null!;
        private ZoningControllerToolUISystem m_ZoningControllerToolUISystem = null!;
        private ToolHighlightSystem m_ToolHighlightSystem = null!;

        // Keep as ProxyAction and just poll it; don’t try to enable/disable.
        private ProxyAction m_InvertZoningAction = default!;

        public const string ToolID = "Zone Controller Tool";
        public override string toolID => ToolID;

        private ComponentLookup<AdvancedRoad> m_AdvancedRoadLookup;
        private BufferLookup<SubBlock> m_SubBlockLookup;
        private EntityQuery m_TempZoningQuery;
        private EntityQuery m_SoundbankQuery;
        private PrefabBase m_ToolPrefab = null!;

        private NativeList<Entity> m_SelectedEntities;

        private int2 Depths => m_ZoningControllerToolUISystem.ToolDepths;
        private ZoningMode ZoningMode => m_ZoningControllerToolUISystem.ToolZoningMode;

        protected override void OnCreate()
        {
            AdvancedRoadToolsMod.s_Log.Debug($"{nameof(ZoningControllerToolSystem)}.{nameof(OnCreate)}");
            base.OnCreate();

            m_ToolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
            m_ZoningControllerToolUISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
            m_ToolHighlightSystem = World.GetOrCreateSystemManaged<ToolHighlightSystem>();

            // Get the action that Settings registered
            m_InvertZoningAction = AdvancedRoadToolsMod.m_InvertZoningAction;

            m_TempZoningQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TempZoning>()
                .Build(this);

            m_SoundbankQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<ToolUXSoundSettingsData>()
                .Build(this);

            m_AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true);
            m_SubBlockLookup = GetBufferLookup<SubBlock>(true);

            m_SelectedEntities = new NativeList<Entity>(Allocator.Persistent);

            ToolDefinition definition = new ToolDefinition(
                 typeof(ZoningControllerToolSystem),
                 toolID,
                 59,
                 new ToolDefinition.UI() // COUI path for ToolsIcon.png
            );
            ToolsHelper.RegisterTool(definition);
        }

        protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
        {
            AdvancedRoadToolsMod.s_Log.Debug($"{nameof(ZoningControllerToolSystem)}.{nameof(OnGameLoadingComplete)}");
            base.OnGameLoadingComplete(purpose, mode);

            // Put the tool near the front of the palette
            m_ToolSystem.tools.Remove(this);
            m_ToolSystem.tools.Insert(5, this);
        }

        protected override void OnStartRunning()
        {
            AdvancedRoadToolsMod.s_Log.Debug($"{nameof(ZoningControllerToolSystem)}.{nameof(OnStartRunning)}");
            base.OnStartRunning();
            // Don’t toggle m_InvertZoningAction; just poll it each frame.
            applyAction.enabled = true;

            requireZones = true;
            requireNet = Layer.Road;
            allowUnderground = true;
        }

        protected override void OnStopRunning()
        {
            AdvancedRoadToolsMod.s_Log.Debug($"{nameof(ZoningControllerToolSystem)}.{nameof(OnStopRunning)}");
            base.OnStopRunning();
            applyAction.enabled = false;

            requireZones = false;
            requireNet = Layer.None;
            allowUnderground = false;
        }

        private enum Mode
        {
            None, Select, Apply, Cancel, Preview
        }
        private Mode m_Mode;
        private Entity m_PreviewEntity;

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps = Dependency;
            m_AdvancedRoadLookup.Update(this);
            m_SubBlockLookup.Update(this);

            EntityCommandBuffer ecb = m_ToolOutputBarrier.CreateCommandBuffer();
            bool hasHit = GetRaycastResult(out Entity hitEntity, out RaycastHit hit);
            ToolUXSoundSettingsData soundbank = m_SoundbankQuery.GetSingleton<ToolUXSoundSettingsData>();

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

            switch (m_Mode)
            {
                case Mode.Preview:
                    if (m_PreviewEntity != hitEntity)
                    {
                        if (m_PreviewEntity != Entity.Null)
                        {
                            m_ToolHighlightSystem.HighlightEntity(m_PreviewEntity, false);
                            m_SelectedEntities.Clear();
                            m_PreviewEntity = Entity.Null;
                        }

                        if (hasHit)
                        {
                            m_ToolHighlightSystem.HighlightEntity(hitEntity, true);
                            m_SelectedEntities.Add(hitEntity);
                            m_PreviewEntity = hitEntity;
                        }
                    }
                    break;

                case Mode.Select when hasHit:
                    if (!m_SelectedEntities.Contains(hitEntity))
                    {
                        m_SelectedEntities.Add(hitEntity);
                        m_ToolHighlightSystem.HighlightEntity(hitEntity, true);
                        AudioManager.instance.PlayUISound(soundbank.m_SelectEntitySound);
                    }
                    break;

                case Mode.Apply:
                    JobHandle setJob = new SetAdvancedRoadJob
                    {
                        TempZoningLookup = GetComponentLookup<TempZoning>(true),
                        Entities = m_SelectedEntities.AsArray().AsReadOnly(),
                        ECB = ecb
                    }.Schedule(inputDeps);
                    inputDeps = JobHandle.CombineDependencies(inputDeps, setJob);

                    foreach (Entity se in m_SelectedEntities)
                        m_ToolHighlightSystem.HighlightEntity(se, false);
                    m_SelectedEntities.Clear();
                    AudioManager.instance.PlayUISound(soundbank.m_NetBuildSound);
                    break;

                case Mode.Cancel:
                    foreach (Entity se in m_SelectedEntities)
                        m_ToolHighlightSystem.HighlightEntity(se, false);
                    AudioManager.instance.PlayUISound(soundbank.m_NetCancelSound);
                    m_SelectedEntities.Clear();
                    break;
            }

            // RMB invert (or whatever the user bound in Options)
            if (m_InvertZoningAction.WasPressedThisFrame())
            {
                m_ZoningControllerToolUISystem.InvertZoningMode();
            }

            JobHandle syncTempJob = new SyncTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                TempZoningLookup = GetComponentLookup<TempZoning>(true),
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                Depths = Depths
            }.Schedule(m_SelectedEntities.Length, 32, inputDeps);

            NativeArray<Entity> tempZoningEntities = m_TempZoningQuery.ToEntityArray(Allocator.TempJob);
            inputDeps = JobHandle.CombineDependencies(inputDeps, syncTempJob);

            JobHandle cleanupTempJob = new CleanupTempJob
            {
                ECB = m_ToolOutputBarrier.CreateCommandBuffer().AsParallelWriter(),
                SelectedEntities = m_SelectedEntities.AsArray().AsReadOnly(),
                Entities = tempZoningEntities.AsReadOnly()
            }.Schedule(tempZoningEntities.Length, 32, inputDeps);

            inputDeps = JobHandle.CombineDependencies(inputDeps, cleanupTempJob);

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

            switch (hasAdvancedRoad)
            {
                case true when math.any(Depths != data.Depths):
                case false when math.any(Depths != new int2(6)):
                    return true;
            }

            entity = Entity.Null;
            return false;
        }

        public override PrefabBase GetPrefab() => m_ToolPrefab;

        public override bool TrySetPrefab(PrefabBase prefab)
        {
            if (prefab.name != toolID)
                return false;

            AdvancedRoadToolsMod.s_Log.Debug($"{toolID}:Selected");
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
                Entity entity = SelectedEntities[index];

                if (TempZoningLookup.TryGetComponent(entity, out TempZoning data) && math.any(data.Depths != Depths))
                {
                    ECB.SetComponent(index, entity, new TempZoning { Depths = Depths });
                }
                else
                {
                    ECB.AddComponent(index, entity, new TempZoning { Depths = Depths });
                }
                ECB.AddComponent<Updated>(index, entity);
            }
        }

        public struct CleanupTempJob : IJobParallelFor
        {
            public EntityCommandBuffer.ParallelWriter ECB;
            public NativeArray<Entity>.ReadOnly SelectedEntities;
            public NativeArray<Entity>.ReadOnly Entities;

            public void Execute(int index)
            {
                Entity entity = Entities[index];
                if (SelectedEntities.Contains(entity))
                    return;

                ECB.RemoveComponent<TempZoning>(index, entity);
                ECB.AddComponent<Updated>(index, entity);
            }
        }

        public struct SetAdvancedRoadJob : IJob
        {
            public NativeArray<Entity>.ReadOnly Entities;
            public ComponentLookup<TempZoning> TempZoningLookup;
            public EntityCommandBuffer ECB;

            public void Execute()
            {
                foreach (Entity entity in Entities)
                {
                    if (!TempZoningLookup.TryGetComponent(entity, out TempZoning temp))
                        continue;

                    ECB.RemoveComponent<TempZoning>(entity);
                    ECB.AddComponent(entity, new AdvancedRoad { Depths = temp.Depths });
                }
            }
        }
    }
}
