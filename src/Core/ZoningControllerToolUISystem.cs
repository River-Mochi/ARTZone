using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using Game;
using Game.UI;
using Unity.Mathematics;
using AdvancedRoadTools.ExtendedRoadUpgrades;

namespace AdvancedRoadTools.Core;

public partial class ZoningControllerToolUISystem : UISystemBase
{
    private const string ModId = "AdvancedRoadTools";
    
    private ValueBinding<int> _zoningMode;
    private ValueBinding<int> _zoningDepthLeft;
    private ValueBinding<int> _zoningDepthRight;
    
    public int DepthLeft => _zoningDepthLeft.value;
    public int DepthRight => _zoningDepthRight.value;
    public ZoningMode ZoningMode => (ZoningMode)_zoningMode.value;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        AddBinding(_zoningMode = new ValueBinding<int>(ModId, "ZoningMode", (int)ZoningMode.Both));
        AddBinding(_zoningDepthLeft = new ValueBinding<int>(ModId, "ZoningDepthLeft", 6));
        AddBinding(_zoningDepthRight = new ValueBinding<int>(ModId, "ZoningDepthRight", 6));
        
        
        AddBinding(new TriggerBinding<int>(ModId, "ChangeZoningMode", ChangeZoningMode));
        AddBinding(new TriggerBinding(ModId, "depth-up-left-arrow", () => IncreaseDepth(_zoningDepthLeft.value, ZoningMode.Left)));
        AddBinding(new TriggerBinding(ModId, "depth-down-left-arrow", () => DecreaseDepth(_zoningDepthLeft.value, ZoningMode.Left)));
        AddBinding(new TriggerBinding(ModId, "depth-up-right-arrow", () => IncreaseDepth(_zoningDepthRight.value, ZoningMode.Right)));
        AddBinding(new TriggerBinding(ModId, "depth-down-right-arrow", () => DecreaseDepth(_zoningDepthRight.value, ZoningMode.Right)));
    }

    private void DecreaseDepth(int value, ZoningMode mode)
    {
        ChangeZoningDepth(value-1, mode);
    }

    private void IncreaseDepth(int value, ZoningMode mode)
    {
        ChangeZoningDepth(value+1, mode);
    }

    private void ChangeZoningDepth(int value, ZoningMode mode)
    {
        log.Info(value);

        value = math.clamp(value, 0, AdvancedRoadToolsMod.m_Setting.MaxDepth);

        if ((mode & ZoningMode.Left) == ZoningMode.Left)
        {
            if(value == 0)
                _zoningMode.Update((int)((ZoningMode)_zoningMode.value ^ ZoningMode.Left));
            else
                _zoningDepthLeft.Update(value);
        }
        else if ((mode & ZoningMode.Right) == ZoningMode.Right)
        {
            if(value == 0)
                _zoningMode.Update((int)((ZoningMode)_zoningMode.value ^ ZoningMode.Right));
            else
                _zoningDepthRight.Update(value);
        }
    }

    protected override void OnGamePreload(Purpose purpose, GameMode mode)
    {
        base.OnGamePreload(purpose, mode);
        UpgradesManager.Install();
    }

    private void ChangeZoningMode(int value)
    {
        var mode = (ZoningMode)value;
        AdvancedRoadToolsMod.log.Info(mode);
        
        _zoningMode.Update(value);
    }
}