using System.Linq;
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
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace AdvancedRoadTools.Tools;

[BurstCompile]
public partial class ZoningControllerToolSystem : ToolBaseSystem
{
    private ToolOutputBarrier toolOutputBarrier;
    private ZoningControllerToolUISystem zoningControllerToolUISystem;
    private ToolHighlightSystem toolHighlightSystem;

    private IProxyAction invertZoningAction;

    public override string toolID => "Zone Controller Tool";
    
    private ComponentLookup<AdvancedRoad> advancedRoadLookup;
    private BufferLookup<SubBlock> subBlockLookup;
    private int2 Depths => zoningControllerToolUISystem.Depths;
    private EntityQuery tempAdvancedRoadQuery;
    private EntityQuery soundbankQuery;
    private Entity lastEntity;

    [BurstCompile]
    protected override void OnCreate()
    {
        log.Debug($"{toolID}:{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnCreate();
        toolOutputBarrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
        zoningControllerToolUISystem = World.GetOrCreateSystemManaged<ZoningControllerToolUISystem>();
        toolHighlightSystem = World.GetOrCreateSystemManaged<ToolHighlightSystem>();

        invertZoningAction = AdvancedRoadToolsMod.m_InvertZoningAction;

        tempAdvancedRoadQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TempZoning>()
            .Build(this);

        soundbankQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ToolUXSoundSettingsData>()
            .Build(this);

        advancedRoadLookup = GetComponentLookup<AdvancedRoad>(true);
        subBlockLookup = GetBufferLookup<SubBlock>(true);
        
        ExtendedRoadUpgrades.UpgradesManager.Install();
    }

    /// <inheritdoc/>
    protected override void OnStartRunning()
    {
        log.Debug($"{toolID}:{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnStartRunning();
        applyAction.enabled = true;
        invertZoningAction.enabled = true;
        requireZones = true;
        requireNet = Layer.Road;
        allowUnderground = true;
    }

    ///cleans up actions or whatever else you want to happen when your tool becomes inactive.
    protected override void OnStopRunning()
    {
        log.Debug($"{toolID}:{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnStartRunning();
        applyAction.enabled = false;
        invertZoningAction.enabled = false;
        requireZones = false;
        requireNet = Layer.None;
        allowUnderground = false;
    }

    protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
    {
        log.Debug($"{toolID}:{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnGameLoadingComplete(purpose, mode);
        m_ToolSystem.tools.Remove(this);
        m_ToolSystem.tools.Insert(10, this);
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps = Dependency;

        var ecb = toolOutputBarrier.CreateCommandBuffer();
        bool raycastFlag = GetRaycastResult(out Entity e, out RaycastHit hit);
        var entityChanged = lastEntity != e;
        var soundbank = soundbankQuery.GetSingleton<ToolUXSoundSettingsData>();

        if (entityChanged)
        {
            //Highlight new entity and unhighlight old
            if (lastEntity != Entity.Null)
            {
                toolHighlightSystem.HighlightEntity(lastEntity, false);
            }
            if (e != Entity.Null)
            {
                toolHighlightSystem.HighlightEntity(e, true);
            }
            AudioManager.instance.PlayUISound(soundbank.m_SelectEntitySound);
        }

        if (entityChanged)
        {
            var handleTempJob = new HandleTempJob
            {
                RaycastHit = hit,
                ECB = ecb,
                AdvancedRoadLookup = GetComponentLookup<AdvancedRoad>(true),
                TempZoningLookup = GetComponentLookup<TempZoning>(true),
                SubBlockLookup = GetBufferLookup<SubBlock>(true),
                TempAdvancedRoadEntities = tempAdvancedRoadQuery.ToEntityArray(Allocator.TempJob),
                Depths = Depths,
            }.Schedule(inputDeps);
            inputDeps = JobHandle.CombineDependencies(inputDeps, handleTempJob);
        }

        //Left Click will set the Zoning depth to the current setting
        if (applyAction.WasPressedThisFrame() || raycastFlag)
        {
            var setAdvancedRoadJob = new SetAdvancedRoadJob
            {
                TempZoningLookup = GetComponentLookup<TempZoning>(true),
                RoadEntity = e,
                ECB = ecb
            }.Schedule(inputDeps);
            inputDeps = JobHandle.CombineDependencies(inputDeps, setAdvancedRoadJob);
            
            AudioManager.instance.PlayUISound(soundbank.m_NetBuildSound);
        }

        //Right click inverts the current zoning mode
        if (invertZoningAction.WasPressedThisFrame())
        {
            zoningControllerToolUISystem.InvertZoningMode();
            
            AudioManager.instance.PlayUISound(soundbank.m_NetCancelSound);
        }

        toolOutputBarrier.AddJobHandleForProducer(inputDeps);

        lastEntity = e;
        return inputDeps;
    }


    private new bool GetRaycastResult(out Entity entity, out RaycastHit hit)
    {
        base.GetRaycastResult(out entity, out hit);

        var hasAdvancedRoad = advancedRoadLookup.TryGetComponent(entity, out var data);
        var hasSubBlock = subBlockLookup.TryGetBuffer(entity, out _);

        if (!hasSubBlock) return false;

        switch (hasAdvancedRoad)
        {
            case true
                when math.any(Depths != data.Depths)
                : //if it has advanced data and depths is different from current depths
            case false
                when math.any(Depths != new int2(6))
                : //if it doesn't have an advanced data and depths are game's defaults
                return true;
        }

        return false;
    }

    private PrefabBase toolPrefab;

    public void SetPrefab(PrefabBase prefab) => toolPrefab = prefab;

    public override PrefabBase GetPrefab()
    {
        return toolPrefab;
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        log.Debug($"{toolID}:{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
        }
        else if (!isEnabled && m_ToolSystem.activeTool == this)
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
                        log.Debug(
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

            log.Debug($"[{nameof(HandleTempJob)}] Removed temporary zoning of {entity}");
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
            log.Debug($"[{nameof(SetAdvancedRoadJob)}.Execute()]");

            if (!TempZoningLookup.TryGetComponent(RoadEntity, out var tempZoning)) return;

            var newDepth = tempZoning.Depths;

            ECB.RemoveComponent<TempZoning>(RoadEntity);
            ECB.AddComponent(RoadEntity,
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
                    || (!hasAdvancedRoad && hasSubBlock && !subBlock.IsEmpty &&
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