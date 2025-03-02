using System;
using System.Collections.Generic;
using Game.Net;
using Game.Prefabs;

namespace AdvancedRoadTools.Tools;

public struct ToolDefinition(Type toolSystemType, string toolId, int priority = 60, ToolDefinition.UI ui = default) : IEquatable<ToolDefinition>
{
    public Type Type = toolSystemType;
    public string ToolID = toolId;
    public int Priority = priority;
    public bool Underground = false;
    public UI ui = ui;
    public PlacementFlags PlacementFlags;
    public CompositionFlags SetFlags;
    public CompositionFlags UnsetFlags;
    public IEnumerable<NetPieceRequirements> SetState;
    public IEnumerable<NetPieceRequirements> UnsetState;

    public ToolDefinition(Type toolSystemType,string toolId, UI ui) : this(toolSystemType, toolId, 60, ui)
    {
        
    }

    public struct UI(string imagePath)
    {
        public const string PathPrefix = "coui://ui-mods/images/";
        public const string ImageFormat = ".svg";
        public string ImagePath = imagePath;
    }


    public bool Equals(ToolDefinition other)
    {
        return ToolID == other.ToolID;
    }

    public override bool Equals(object obj)
    {
        return obj is ToolDefinition other && Equals(other);
    }

    public override int GetHashCode()
    {
        return (ToolID != null ? ToolID.GetHashCode() : 0);
    }
}