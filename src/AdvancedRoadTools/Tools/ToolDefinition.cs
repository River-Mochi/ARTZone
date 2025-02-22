using System;

namespace AdvancedRoadTools.Tools;

public struct ToolDefinition(Type toolSystemType, string toolId, int priority = 60, ToolDefinition.UI ui = default) : IEquatable<ToolDefinition>
{
    public readonly Type Type = toolSystemType;
    public readonly string ToolID = toolId;
    public int Priority = priority;
    public UI ui = ui;

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