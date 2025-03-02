using System.Linq;
using Colossal.Serialization.Entities;
using Game;
using Game.Audio;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using AgeMask = Game.Tools.AgeMask;

namespace AdvancedRoadTools.Tools;

public partial class AdvancedParallelToolSystem : ToolBaseSystem
{
    public const string ToolID = "Advanced Parallel Tool";
    public override string toolID => ToolID;
    private ToolOutputBarrier barrier;
    public AdvancedParallelToolUISystem UISystem;
    private ToolHighlightSystem highlightSystem;

    private PrefabBase toolPrefab;
    private EntityQuery soundbankQuery;
    private EntityQuery tempParallelQuery;
    private ComponentLookup<Curve> curveLookup;
    private ComponentLookup<TempParallel> tempParallelLookup;
    private Entity lastEntity;
    public override PrefabBase GetPrefab() => toolPrefab;

    protected override void OnCreate()
    {
        log.Debug($"{nameof(AdvancedParallelToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnCreate();
        barrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
        UISystem = World.GetOrCreateSystemManaged<AdvancedParallelToolUISystem>();
        highlightSystem = World.GetOrCreateSystemManaged<ToolHighlightSystem>();

        soundbankQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ToolUXSoundSettingsData>()
            .Build(this);
        
        tempParallelQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<TempParallel>()
            .Build(this);

        curveLookup = GetComponentLookup<Curve>(true);
        tempParallelLookup = GetComponentLookup<TempParallel>(true);

        var definition = new ToolDefinition(typeof(AdvancedParallelToolSystem), toolID, new ToolDefinition.UI
        {
            ImagePath = ToolDefinition.UI.PathPrefix + toolID + ToolDefinition.UI.ImageFormat,
        })
        {
            PlacementFlags = PlacementFlags.UndergroundUpgrade | PlacementFlags.IsUpgrade
        };
        ToolsHelper.RegisterTool(definition);
    }

    protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
    {
        log.Debug($"{nameof(AdvancedParallelToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnGameLoadingComplete(purpose, mode);

        m_ToolSystem.tools.Remove(this);
        m_ToolSystem.tools.Insert(5, this);
    }

    protected override void OnStartRunning()
    {
        log.Debug($"{nameof(AdvancedParallelToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnStartRunning();
        applyAction.enabled = true;
        requireNet = Layer.Road;
        allowUnderground = true;
    }


    protected override void OnStopRunning()
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnStopRunning();
        applyAction.enabled = false;
        requireNet = Layer.None;
        allowUnderground = false;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        inputDeps = JobHandle.CombineDependencies(Dependency, inputDeps);
        curveLookup.Update(this);
        tempParallelLookup.Update(this);

        var ecb = barrier.CreateCommandBuffer();
        var soundbank = soundbankQuery.GetSingleton<ToolUXSoundSettingsData>();
        bool hasHit = GetRaycastResult(out Entity e, out RaycastHit hit);
        var entityChanged = lastEntity != e;


        if (entityChanged)
        {
            //Highlight new entity and unhighlight old
            if (lastEntity != Entity.Null)
            {
                highlightSystem.HighlightEntity(lastEntity, false);
            }

            if (e != Entity.Null)
            {
                highlightSystem.HighlightEntity(e, true);
                AudioManager.instance.PlayUISound(soundbank.m_SelectEntitySound);
            }
        }

        if (hasHit || !tempParallelQuery.IsEmpty)
        {
            var handleTempJob = new HandleTempJob
            {
                RaycastHit = hit,
                ECB = ecb,
                TempParallelLookup = tempParallelLookup,
                Entities = tempParallelQuery.ToEntityArray(Allocator.Temp),

            }.Schedule(inputDeps);
            inputDeps = JobHandle.CombineDependencies(inputDeps, handleTempJob);
        }

        //Left Click will set the Zoning depth to the current setting
        if (applyAction.WasPressedThisFrame() && hasHit)
        {
            log.Info($"{toolID}:Apply Action");
            AudioManager.instance.PlayUISound(soundbank.m_NetBuildSound);
        }

        barrier.AddJobHandleForProducer(inputDeps);
        lastEntity = e;
        return inputDeps;
    }

    public override void InitializeRaycast()
    {
        base.InitializeRaycast();

        this.m_ToolRaycastSystem.typeMask = TypeMask.Net;
        this.m_ToolRaycastSystem.netLayerMask = Layer.Road;
    }

    public new bool GetRaycastResult(out Entity entity, out RaycastHit hit)
    {
        return base.GetRaycastResult(out entity, out hit) && curveLookup.HasComponent(entity);
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        if (prefab.name != toolID)
            return false;

        log.Debug($"{toolID}:Selected");
        toolPrefab = prefab;
        return true;
    }

    public struct HandleTempJob : IJob
    {
        public RaycastHit RaycastHit;
        public EntityCommandBuffer ECB;
        public ComponentLookup<TempParallel> TempParallelLookup;
        public NativeArray<Entity> Entities;

        public void Execute()
        {
            var entity = RaycastHit.m_HitEntity;

            if (entity != Entity.Null && !TempParallelLookup.HasComponent(entity))
            {
                ECB.AddComponent(entity, new TempParallel()
                {
                    RootEntity = entity,
                    Side = ZoningMode.Both,
                    Count = new int2(4,2),
                    Spacing = new float2(8,6)
                });
                ECB.AddComponent<Updated>(entity);
            }

            foreach (var tempParallelEntity in Entities.Where(temp =>
                         temp != entity))
            {
                ECB.RemoveComponent<TempParallel>(tempParallelEntity);
                ECB.AddComponent<Updated>(tempParallelEntity);
            }

            Entities.Dispose();
        }
    }
}

public struct TempParallel : IComponentData
{
    public Entity RootEntity;
    public ZoningMode Side;

    public int2 Count;
    public float2 Spacing;

    public float LeftSpacing
    {
        get => Spacing.x;
        set => Spacing.x = math.clamp(value, 0, float.MaxValue);
    }

    public float RightSpacing
    {
        get => Spacing.y;
        set => Spacing.y = math.clamp(value, 0, float.MaxValue);
    }

    public int LeftCount
    {
        get => Count.x;
        set => Count.x = math.clamp(0, int.MaxValue, value);
    }

    public int RightCount
    {
        get => Count.y;
        set => Count.y = math.clamp(0, int.MaxValue, value);
    }
}

