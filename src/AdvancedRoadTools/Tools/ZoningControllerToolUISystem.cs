using Colossal.UI.Binding;
using Game;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using Unity.Mathematics;

namespace AdvancedRoadTools.Tools;

public partial class ZoningControllerToolUISystem : UISystemBase
{
    private ValueBinding<int> zoningMode;
    private ValueBinding<int> zoningDepthLeft;
    private ValueBinding<int> zoningDepthRight;

    private ValueBinding<bool> isRoadPrefab;

    public int DepthLeft => zoningDepthLeft.value;
    public int DepthRight => zoningDepthRight.value;
    public ZoningMode ZoningMode => (ZoningMode)zoningMode.value;
    public bool InvertedLastFrame;

    public int2 Depths
    {
        get => new(zoningDepthLeft.value, zoningDepthRight.value);
        set
        {
            var newZoningMode = ZoningMode.Both;
            if (value.x == 0)
                newZoningMode ^= ZoningMode.Left;
            if (value.y == 0)
                newZoningMode ^= ZoningMode.Right;
            
            ChangeZoningMode(newZoningMode);
        }
    }

    private ToolSystem mainToolSystem;
    private ZoningControllerToolSystem toolSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        AddBinding(zoningMode = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ZoningMode", (int)ZoningMode.Both));
        AddBinding(zoningDepthLeft = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ZoningDepthLeft", 6));
        AddBinding(zoningDepthRight = new ValueBinding<int>(AdvancedRoadToolsMod.ModID, "ZoningDepthRight", 6));
        AddBinding(isRoadPrefab = new ValueBinding<bool>(AdvancedRoadToolsMod.ModID, "IsRoadPrefab", false));


        AddBinding(new TriggerBinding<int>(AdvancedRoadToolsMod.ModID, "ChangeZoningMode", ChangeZoningMode));
        AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "FlipBothMode", FlipBothMode));
        AddBinding(new TriggerBinding(AdvancedRoadToolsMod.ModID, "ToggleZoneControllerTool", ToggleTool));
        
        mainToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            mainToolSystem.EventPrefabChanged += EventPrefabChanged;
        toolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        InvertedLastFrame = false;
    }

    private void EventPrefabChanged(PrefabBase obj)
    {
        isRoadPrefab.Update(obj is RoadPrefab);
    }

    private void ToggleTool() => toolSystem.SetToolEnabled(mainToolSystem.activeTool != toolSystem);


    private void FlipBothMode()
    {
        if (ZoningMode == ZoningMode.Both)
        {
            zoningMode.Update((int)ZoningMode.None);
            zoningDepthLeft.Update(0);
            zoningDepthRight.Update(0);
        }
        else
        {
            zoningMode.Update((int)ZoningMode.Both);
            zoningDepthLeft.Update(6);
            zoningDepthRight.Update(6);
        }

    }

    private void ChangeZoningMode(int value)
    {
        var mode = (ZoningMode)value;
        //log.Info(mode);

        zoningMode.Update(value);

        zoningDepthLeft.Update((mode & ZoningMode.Left) == 0 ? 0 : 6);
        zoningDepthRight.Update((mode & ZoningMode.Right) == 0 ? 0 : 6);
    }

    public void ChangeZoningMode(ZoningMode mode)
    {
        ChangeZoningMode((int)mode);
    }

    public void InvertZoningMode()
    {
        InvertedLastFrame = true;
        ChangeZoningMode(~ZoningMode);
    }
}