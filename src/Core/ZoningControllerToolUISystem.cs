using System;
using System.Threading.Tasks;
using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using Game;
using Game.UI;
using Unity.Mathematics;
using AdvancedRoadTools.ExtendedRoadUpgrades;
using Game.SceneFlow;
using UnityEngine;
using cohtml.Net;

namespace AdvancedRoadTools.Core;

public partial class ZoningControllerToolUISystem : UISystemBase
{
    private const string ModId = "Zoning_Controller";

    public ZoningMode CurrentZoningMode => (ZoningMode)m_zoningMode.value;

    public int CurrentZoningDepth => m_zoningDepth.value;
    
    private ValueBinding<int> m_zoningMode;
    private ValueBinding<int> m_zoningDepth;

    private cohtml.Net.View m_UiView;

    protected override void OnCreate()
    {
        base.OnCreate();
        
        AddBinding(m_zoningMode = new ValueBinding<int>(ModId, "ZoningMode", (int)ZoningMode.Both));
        AddBinding(m_zoningDepth = new ValueBinding<int>(ModId, "ZoningDepth", 6));
        
        
        AddBinding(new TriggerBinding<int>(ModId, "ChangeToolMode", ChangeZoningMode));
        AddBinding(new TriggerBinding<int>(ModId, "ChangeToolDepth", ChangeZoningDepth));
        AddBinding(new TriggerBinding(ModId, "depth-up-arrow", IncreaseDepth));
        AddBinding(new TriggerBinding(ModId, "depth-down-arrow", DecreaseDepth));
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        m_UiView = GameManager.instance.userInterface.view.View;
        
        m_UiView.ExecuteScript("if (yyTreeController == null) var yyTreeController = {};");
        
    }

    private void DecreaseDepth()
    {
        var depth = m_zoningDepth.value;
        depth = math.clamp(depth-1, 0, int.MaxValue);
        
        m_zoningDepth.Update(depth);
    }

    private void IncreaseDepth()
    {
        var depth = m_zoningDepth.value;
        depth = math.clamp(depth+1, 0, int.MaxValue);
        
        m_zoningDepth.Update(depth);
    }

    private void ChangeZoningDepth(int value)
    {
        if(value < 0)
            log.Error("Zoning depth must be greater than 0");
        m_zoningDepth.Update(value);
    }

    protected override void OnGamePreload(Purpose purpose, GameMode mode)
    {
        base.OnGamePreload(purpose, mode);
        UpgradesManager.Install();
    }

    private void ChangeZoningMode(int value)
    {
        var mode = (ZoningMode)value;
        switch (mode)
        {
            case ZoningMode.None:
                m_zoningMode.Update((int)ZoningMode.None);
                m_zoningDepth.Update(0);
                break;
            case ZoningMode.Right:
                m_zoningMode.Update((int)ZoningMode.None);
                break;
            case ZoningMode.Left:
                m_zoningMode.Update((int)ZoningMode.None);
                break;
            case ZoningMode.Both:
                m_zoningMode.Update((int)ZoningMode.None);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}