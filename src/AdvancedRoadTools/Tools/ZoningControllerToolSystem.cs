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
public partial class ZoningControllerToolSystem : ToolBaseSystem, IARTTool
{
    private ToolOutputBarrier toolOutputBarrier;
    private ZoningControllerToolUISystem zoningControllerToolUISystem;
    private ToolHighlightSystem toolHighlightSystem;

    private IProxyAction invertZoningAction;

    public const string ToolID =  "Zone Controller Tool";
    public override string toolID => ToolID;

    private ComponentLookup<AdvancedRoad> advancedRoadLookup;
    private BufferLookup<SubBlock> subBlockLookup;
    private int2 Depths => zoningControllerToolUISystem.Depths;
    private ZoningMode ZoningMode => zoningControllerToolUISystem.ZoningMode;
    private EntityQuery tempAdvancedRoadQuery;
    private EntityQuery soundbankQuery;
    private Entity lastEntity;
    private PrefabBase toolPrefab;

    [BurstCompile]
    protected override void OnCreate()
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
        
        var definition = new ToolDefinition(typeof(ZoningControllerToolSystem), toolID, new ToolDefinition.UI
        {
            ImagePath = ToolDefinition.UI.PathPrefix + toolID + ToolDefinition.UI.ImageFormat,
        });

        ToolsHelper.RegisterTool(definition);
    }

    /// <inheritdoc/>
    protected override void OnStartRunning()
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnStartRunning();
        applyAction.enabled = false;
        invertZoningAction.enabled = false;
        requireZones = false;
        requireNet = Layer.None;
        allowUnderground = false;
    }

    protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
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
                AudioManager.instance.PlayUISound(soundbank.m_SelectEntitySound);
            }
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
        if (applyAction.WasPressedThisFrame() && raycastFlag)
        {
            log.Info($"{toolID}:Setting advanced road of {e}");
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
            log.Debug($"{toolID}:Inverting zoning configuration from {ZoningMode} to {~ZoningMode}");
            zoningControllerToolUISystem.InvertZoningMode();

            AudioManager.instance.PlayUISound(soundbank.m_NetCancelSound);
        }

        toolOutputBarrier.AddJobHandleForProducer(inputDeps);

        lastEntity = e;
        return inputDeps;
    }

    private new bool GetRaycastResult(out Entity entity, out RaycastHit hit)
    {
        if (!base.GetRaycastResult(out entity, out hit)) return false;

        var hasAdvancedRoad = advancedRoadLookup.TryGetComponent(entity, out var data);
        var hasSubBlock = subBlockLookup.TryGetBuffer(entity, out _);

        if (!hasSubBlock)
        {
            entity = Entity.Null;
            return false;
        }

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

        entity = Entity.Null;
        return false;
    }

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
}