using Colossal.Serialization.Entities;
using Colossal.UI.Binding;
using Game;
using Game.UI;
using Colossal.Logging;
using Game.Prefabs;
using Game.Tools;
using Unity.Entities;

namespace AdvancedRoadTools.Core;

public partial class ZoningControllerToolUISystem : UISystemBase
{
    private const string ModId = "AdvancedRoadTools";

    private ValueBinding<int> _zoningMode;
    private ValueBinding<int> _zoningDepthLeft;
    private ValueBinding<int> _zoningDepthRight;

    private ValueBinding<bool> _isRoadPrefab;

    public int DepthLeft => _zoningDepthLeft.value;
    public int DepthRight => _zoningDepthRight.value;
    public ZoningMode ZoningMode => (ZoningMode)_zoningMode.value;

    public ToolSystem m_ToolSystem;
    public ZoningControllerToolSystem m_ZoningControllerToolSystem;
    public ILog log => AdvancedRoadToolsMod.log;

    protected override void OnCreate()
    {
        base.OnCreate();

        var prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

        AddBinding(_zoningMode = new ValueBinding<int>(ModId, "ZoningMode", (int)ZoningMode.Both));
        AddBinding(_zoningDepthLeft = new ValueBinding<int>(ModId, "ZoningDepthLeft", 6));
        AddBinding(_zoningDepthRight = new ValueBinding<int>(ModId, "ZoningDepthRight", 6));
        AddBinding(_isRoadPrefab = new ValueBinding<bool>(ModId, "IsRoadPrefab", false));


        AddBinding(new TriggerBinding<int>(ModId, "ChangeZoningMode", ChangeZoningMode));
        AddBinding(new TriggerBinding(ModId, "FlipBothMode", FlipBothMode));
        AddBinding(new TriggerBinding(ModId, "ToggleZoneControllerTool", ToggleTool));
        
        m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
            m_ToolSystem.EventPrefabChanged += EventPrefabChanged;
        m_ZoningControllerToolSystem = World.GetOrCreateSystemManaged<ZoningControllerToolSystem>();
    }

    private void EventPrefabChanged(PrefabBase obj)
    {
        _isRoadPrefab.Update(obj is RoadPrefab);
    }

    private void ToggleTool() => m_ZoningControllerToolSystem.SetToolEnabled(m_ToolSystem.activeTool == m_ZoningControllerToolSystem);


    private void FlipBothMode()
    {
        if (ZoningMode == ZoningMode.Both)
        {
            _zoningMode.Update((int)ZoningMode.None);
            _zoningDepthLeft.Update(0);
            _zoningDepthRight.Update(0);
        }
        else
        {
            _zoningMode.Update((int)ZoningMode.Both);
            _zoningDepthLeft.Update(6);
            _zoningDepthRight.Update(6);
        }

    }

    private void ChangeZoningMode(int value)
    {
        var mode = (ZoningMode)value;
        //AdvancedRoadToolsMod.log.Info(mode);

        _zoningMode.Update(value);

        _zoningDepthLeft.Update((mode & ZoningMode.Left) == 0 ? 0 : 6);
        _zoningDepthRight.Update((mode & ZoningMode.Right) == 0 ? 0 : 6);
    }

    public void ChangeZoningMode(ZoningMode mode)
    {
        ChangeZoningMode((int)mode);
    }

    public void InvertZoningMode()
    {
        ChangeZoningMode(~ZoningMode);
    }
}