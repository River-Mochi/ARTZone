using System;
using System.Linq;
using Colossal.Entities;
using Colossal.Logging;
using Colossal.Serialization.Entities;
using Game;
using Game.Audio;
using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.Zones;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace AdvancedRoadTools.Core;

[BurstCompile]
public partial class ZoningControllerToolSystem : ToolBaseSystem
{
    private ToolOutputBarrier _toolOutputBarrier;
    private ZoningControllerToolUISystem _zoningControllerToolUISystem;
    private ILog log => AdvancedRoadToolsMod.log;
    
    private IProxyAction InvertZoningAction;

    private const string LOG_HEADER = $"[{nameof(ZoningControllerToolSystem)}]";

    public override string toolID => "Zone Controller Tool";

    private EntityQuery allBlocksQuery;
    private EntityQuery highlightedEntityQuery;
    private EntityQuery tempAdvancedRoadQuery;

    [BurstCompile]
    protected override void OnCreate()
    {
        log.Info("OnCreate");
        base.OnCreate();
        _toolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
        _zoningControllerToolUISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();

        InvertZoningAction = AdvancedRoadToolsMod.m_InvertZoningAction;

        allBlocksQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Block, Owner>()
            .Build(this);

        highlightedEntityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Highlighted>()
            .Build(this);

        tempAdvancedRoadQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TempZoning>()
            .Build(this);

        ExtendedRoadUpgrades.UpgradesManager.Install();
    }

    /// <inheritdoc/>
    protected override void OnStartRunning()
    {
        log.Debug("OnStartRunning");
        base.OnStartRunning();
        applyAction.enabled = true;
        InvertZoningAction.enabled = true;
        requireZones = true;
        requireNet = Layer.Road;
        allowUnderground = true;
    }

    ///cleans up actions or whatever else you want to happen when your tool becomes inactive.
    protected override void OnStopRunning()
    {
        log.Debug("OnStopRunning");
        base.OnStartRunning();
        applyAction.enabled = false;
        InvertZoningAction.enabled = false;
        requireZones = false;
        requireNet = Layer.None;
        allowUnderground = false;
    }

    protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
    {
        base.OnGameLoadingComplete(purpose, mode);
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{nameof(OnGameLoadingComplete)} New Order:");
        m_ToolSystem.tools.Remove(this);
        m_ToolSystem.tools.Insert(10, this);

        foreach (ToolBaseSystem toolBaseSystem in m_ToolSystem.tools)
        {
            log.Debug($"{nameof(ZoningControllerToolSystem)}.{nameof(OnGameLoadingComplete)} {toolBaseSystem.toolID}");
        }
    }

    private JobHandle Apply(JobHandle inputDeps)
    {
        this.applyMode = ApplyMode.Apply;
        return inputDeps;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps = Dependency;
        
        var ecb = _toolOutputBarrier.CreateCommandBuffer();
        bool raycastFlag = GetRaycastResult(out Entity e, out RaycastHit hit);

        var depths = new int2(_zoningControllerToolUISystem.DepthLeft,
            _zoningControllerToolUISystem.DepthRight);

        if (!highlightedEntityQuery.IsEmpty || raycastFlag)
        {
            var handleHighlightJob = new HandleHighlightJob
            {
                RaycastHit = hit,
                AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true),
                HighlightedLookup = GetComponentLookup<Highlighted>(true),
                EdgeLookup = GetComponentLookup<Edge>(true),
                SubBlockLookup = GetBufferLookup<SubBlock>(true),
                HighlightedEntities = highlightedEntityQuery.ToEntityArray(Allocator.TempJob),
                ECB = ecb,
                Depths = depths,
            }.Schedule(inputDeps);
            inputDeps = JobHandle.CombineDependencies(inputDeps, handleHighlightJob);
        }

        if (!tempAdvancedRoadQuery.IsEmpty || raycastFlag)
        {
            var handleTempJob = new HandleTempJob
            {
                RaycastHit = hit,
                ECB = ecb,
                AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true),
                TempZoningLookup = GetComponentLookup<TempZoning>(true),
                SubBlockLookup = GetBufferLookup<SubBlock>(true),
                TempAdvancedRoadEntities = tempAdvancedRoadQuery.ToEntityArray(Allocator.TempJob),
                Depths = depths,
            }.Schedule(inputDeps);
            inputDeps = JobHandle.CombineDependencies(inputDeps, handleTempJob);
        }

        
        //Left Click will set the Zoning depth to the current setting
        if (applyAction.WasPressedThisFrame() && raycastFlag)
        {
            log.Debug($"{LOG_HEADER} ({nameof(OnUpdate)}) - Left Click on {toolID}");
            try
            {
                var m_SoundQuery = GetEntityQuery(ComponentType.ReadOnly<ToolUXSoundSettingsData>());
                AudioManager m_AudioManager = AudioManager.instance;
                m_AudioManager.PlayUISound(m_SoundQuery.GetSingleton<ToolUXSoundSettingsData>().m_NetBuildSound);
            }
            catch (Exception ex)
            {
                AdvancedRoadToolsMod.log.Error("Failed to play audio: " + ex.Message);
            }

            var setAdvancedRoadJob = new SetAdvancedRoadJob()
            {
                TempZoningLookup = GetComponentLookup<TempZoning>(true),
                RoadEntity = e,
                ECB = ecb
            }.Schedule(inputDeps);
            inputDeps = JobHandle.CombineDependencies(inputDeps, setAdvancedRoadJob);
        }

        //Right click inverts the current zoning mode
        if (InvertZoningAction.WasPressedThisFrame())
        {
            _zoningControllerToolUISystem.InvertZoningMode();
            try
            {
                var m_SoundQuery = GetEntityQuery(ComponentType.ReadOnly<ToolUXSoundSettingsData>());
                AudioManager m_AudioManager = AudioManager.instance;
                m_AudioManager.PlayUISound(m_SoundQuery.GetSingleton<ToolUXSoundSettingsData>().m_NetCancelSound);
            }
            catch (Exception ex)
            {
                AdvancedRoadToolsMod.log.Error("Failed to play audio: " + ex.Message);
            }
        }

        _toolOutputBarrier.AddJobHandleForProducer(inputDeps);

        return inputDeps;
    }

    private PrefabBase m_Prefab;

    public void SetPrefab(PrefabBase prefab) => m_Prefab = prefab;

    public override PrefabBase GetPrefab()
    {
        return m_Prefab;
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        bool isRightPrefab = prefab.name == toolID;

        return isRightPrefab;
    }

    public override void InitializeRaycast()
    {
        base.InitializeRaycast();

        this.m_ToolRaycastSystem.typeMask = TypeMask.Net;
        this.m_ToolRaycastSystem.netLayerMask = Layer.Road;
    }
    
    public void SetToolEnabled(bool isEnabled)
    {
        if (isEnabled && m_ToolSystem.activeTool != this)
        {
            m_ToolSystem.selected = Entity.Null;
            m_ToolSystem.activeTool = this;
        } else if (!isEnabled && m_ToolSystem.activeTool == this)
        {
            m_ToolSystem.selected = Entity.Null;
            m_ToolSystem.activeTool = m_DefaultToolSystem;
        }
    } 
    
    /// <summary>
    /// Updates the highlighted entities and sets them up to preview what the changes will be like
    /// </summary>
    public struct HandleTempJob : IJob
    {
        public RaycastHit RaycastHit;
        public EntityCommandBuffer ECB;
        public ComponentLookup<AdvancedRoad> AdvancedRoadLookup;
        public ComponentLookup<TempZoning> TempZoningLookup;
        public NativeArray<Entity> TempAdvancedRoadEntities;
        public BufferLookup<SubBlock> SubBlockLookup;
        public int2 Depths;
        public bool SettingsChanged;

        public void Execute()
        {
            var entity = RaycastHit.m_HitEntity;
            var hasAdvancedRoad = AdvancedRoadLookup.TryGetComponent(entity, out var data);
            var hasTempZoning = TempZoningLookup.TryGetComponent(entity, out var temp);
            var hasSubBlocks = SubBlockLookup.TryGetBuffer(entity, out var subBlocks);

            if (entity != Entity.Null)
            {
                if ((!hasTempZoning || math.any(Depths != temp.Depths)) && (hasSubBlocks && subBlocks.Length > 0))
                {
                    if ((hasAdvancedRoad && math.any(data.Depths != Depths)) //IF has advance road an its different
                        || (!hasAdvancedRoad &&
                            math.any(Depths !=
                                     new int2(6)))) //OR doesn't have advance road and its different from default
                    {
                        AdvancedRoadToolsMod.log.Debug(
                            $"[{nameof(HandleTempJob)}] Setting temporary zoning of {entity} ({Depths})");
                        ECB.AddComponent(entity, new TempZoning
                        {
                            Depths = Depths
                        });
                        ECB.AddComponent<Updated>(entity);
                    }
                }
            }

            foreach (var tempAdvancedRoadEntity in TempAdvancedRoadEntities.Where(tempAdvancedRoad =>
                         tempAdvancedRoad != entity))
            {
                DeleteTemp(tempAdvancedRoadEntity);
            }

            TempAdvancedRoadEntities.Dispose();
        }

        private void DeleteTemp(Entity entity)
        {
            ECB.RemoveComponent<TempZoning>(entity);
            ECB.AddComponent<Updated>(entity);

            AdvancedRoadToolsMod.log.Debug($"[{nameof(HandleTempJob)}] Removed temporary zoning of {entity}");
        }
    }

    /// <summary>
    /// Sets the previewed zoning
    /// </summary>
    public struct SetAdvancedRoadJob : IJob
    {
        public Entity RoadEntity;
        public ComponentLookup<TempZoning> TempZoningLookup;
        public EntityCommandBuffer ECB;

        public void Execute()
        {
            AdvancedRoadToolsMod.log.Debug($"[{nameof(SetAdvancedRoadJob)}.Execute()]");

            if (!TempZoningLookup.TryGetComponent(RoadEntity, out var tempZoning)) return;
            
            var newDepth = tempZoning.Depths;
            
            ECB.RemoveComponent<TempZoning>(RoadEntity);
            ECB.AddComponent<AdvancedRoad>(RoadEntity,
                new AdvancedRoad { Depths = newDepth });
        }
    }

    /// <summary>
    /// Highlights the moused over road and unhighlights other entities
    /// </summary>
    public struct HandleHighlightJob : IJob
    {
        public RaycastHit RaycastHit;
        public ComponentLookup<AdvancedRoad> AdvancedRoadLookup;
        public ComponentLookup<Highlighted> HighlightedLookup;
        public ComponentLookup<Edge> EdgeLookup;
        public BufferLookup<SubBlock> SubBlockLookup;
        public NativeArray<Entity> HighlightedEntities;
        public EntityCommandBuffer ECB;
        public int2 Depths;

        public void Execute()
        {
            var entity = RaycastHit.m_HitEntity;

            var hasAdvancedRoad = AdvancedRoadLookup.TryGetComponent(entity, out var data);
            var hasSubBlock = SubBlockLookup.TryGetBuffer(entity, out var subBlock);

            if (!HighlightedLookup.HasComponent(entity) && entity != Entity.Null) //IF is not highlighted
            {
                if ((hasAdvancedRoad && hasSubBlock && !subBlock.IsEmpty &&
                     math.any(data.Depths != Depths)) //if advanced data depths is different from current depths
                    || (!hasAdvancedRoad && hasSubBlock && !subBlock.IsEmpty  &&
                        math.any(Depths !=
                                 new int2(6)))) //if it doesn't have an advanced data and depths are game's defaults
                {
                    HighlightEntity(entity);
                }
            }

            foreach (var highlightedEntity in HighlightedEntities.Where(
                         highlightedEntity => highlightedEntity != entity))
            {
                UnhighlightEntity(highlightedEntity);
            }

            HighlightedEntities.Dispose();
        }

        private void HighlightEntity(Entity entity)
        {
            ECB.AddComponent<Highlighted>(entity);
            ECB.AddComponent<BatchesUpdated>(entity);

            if (!EdgeLookup.TryGetComponent(entity, out var edge)) return;

            ECB.AddComponent<Updated>(edge.m_Start);
            ECB.AddComponent<Updated>(edge.m_End);
        }

        private void UnhighlightEntity(Entity entity)
        {
            ECB.RemoveComponent<Highlighted>(entity);
            ECB.AddComponent<BatchesUpdated>(entity);

            if (!EdgeLookup.TryGetComponent(entity, out var edge)) return;

            ECB.AddComponent<Updated>(edge.m_Start);
            ECB.AddComponent<Updated>(edge.m_End);
        }
    }
}