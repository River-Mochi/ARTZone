using Game.Prefabs;
using Game.Tools;

namespace AdvancedRoadTools.Tools;

public partial class AdvancedParallelToolSystem : ToolBaseSystem
{
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
        
        
    }

    public override bool TrySetPrefab(PrefabBase prefab)
    {
        return prefab.name == toolID;
    }
    

}