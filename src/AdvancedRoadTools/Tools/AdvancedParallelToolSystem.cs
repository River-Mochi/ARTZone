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

namespace AdvancedRoadTools.Tools;

public partial class AdvancedParallelToolSystem : ToolBaseSystem, IARTTool
{
    public const string ToolID =  "Advanced Parallel Tool";
    public override string toolID => "Advanced Parallel Tool";
    private ToolOutputBarrier barrier;
    public AdvancedParallelToolUISystem UISystem;
    private ToolHighlightSystem highlightSystem;

    private PrefabBase toolPrefab;
    private EntityQuery soundbankQuery;
    private ComponentLookup<Curve> curveLookup;
    private Entity lastEntity;
    public override PrefabBase GetPrefab() => toolPrefab;

    protected override void OnCreate()
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnCreate();
        barrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
        UISystem = World.GetOrCreateSystemManaged<AdvancedParallelToolUISystem>();
        highlightSystem = World.GetOrCreateSystemManaged<ToolHighlightSystem>();
        
        soundbankQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<ToolUXSoundSettingsData>()
            .Build(this);
        
        curveLookup = GetComponentLookup<Curve>(true);

        var definition = new ToolDefinition(typeof(AdvancedParallelToolSystem), toolID, new ToolDefinition.UI
        {
            ImagePath = ToolDefinition.UI.PathPrefix + toolID + ToolDefinition.UI.ImageFormat,
        });
        ToolsHelper.RegisterTool(definition);
    }

    protected override void OnGameLoadingComplete(Purpose purpose, GameMode mode)
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnGameLoadingComplete(purpose, mode);
        
        m_ToolSystem.tools.Remove(this);
        m_ToolSystem.tools.Insert(10, this);
    }

    protected override void OnStartRunning()
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnStartRunning();
        applyAction.enabled = true;
    }

    
    protected override void OnStopRunning()
    {
        log.Debug($"{nameof(ZoningControllerToolSystem)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        base.OnStopRunning();
        applyAction.enabled = false;
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ecb = barrier.CreateCommandBuffer();
        var soundbank = soundbankQuery.GetSingleton<ToolUXSoundSettingsData>();
        bool hasHit = GetRaycastResult(out Entity e, out RaycastHit hit);
        var entityChanged = lastEntity != e;
        
        curveLookup.Update(this);
        
        
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
        return prefab.name == toolID;
    }


    public void SetPrefab(PrefabBase prefab) => toolPrefab = prefab;
}