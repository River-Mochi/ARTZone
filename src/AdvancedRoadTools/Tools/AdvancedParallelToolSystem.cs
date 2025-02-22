using Game.Prefabs;
using Game.Tools;

namespace AdvancedRoadTools.Tools;

public partial class AdvancedParallelToolSystem : ToolBaseSystem, IARTTool
{
    public const string ToolID =  "Advanced Parallel Tool";
    public override string toolID => "Advanced Parallel Tool";
    private ToolOutputBarrier barrier;
    public AdvancedParallelToolUISystem UISystem;

    public PrefabBase prefab;
    public override PrefabBase GetPrefab() => prefab;

    protected override void OnCreate()
    {
        base.OnCreate();
        barrier = World.GetOrCreateSystemManaged<ToolOutputBarrier>();
        UISystem = World.GetOrCreateSystemManaged<AdvancedParallelToolUISystem>();
        
        
        curveLookup = GetComponentLookup<Curve>(true);

        var definition = new ToolDefinition(typeof(AdvancedParallelToolSystem), toolID, new ToolDefinition.UI
        {
            ImagePath = ToolDefinition.UI.PathPrefix + toolID + ToolDefinition.UI.ImageFormat,
        });
        ToolsHelper.RegisterTool(definition);
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        return prefab.name == toolID;
    }
    

}