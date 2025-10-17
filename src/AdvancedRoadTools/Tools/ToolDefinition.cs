// File: src/AdvancedRoadTools/Tools/ToolDefinition.cs
// Purpose: Small POD describing a tool + its UI icon path and palette priority.

using System;
using System.Collections.Generic;
using Game.Net;
using Game.Prefabs;

namespace AdvancedRoadTools.Tools
{
    public struct ToolDefinition : IEquatable<ToolDefinition>
    {
        public Type Type;
        public string ToolID;
        public int Priority;
        public bool Underground;
        public UI ui;
        public PlacementFlags PlacementFlags;
        public CompositionFlags SetFlags;
        public CompositionFlags UnsetFlags;
        public IEnumerable<NetPieceRequirements> SetState;
        public IEnumerable<NetPieceRequirements> UnsetState;

        public ToolDefinition(Type toolSystemType, string toolId, int priority = 60, UI ui = default)
        {
            Type = toolSystemType;
            ToolID = toolId;
            Priority = priority;
            Underground = false;
            this.ui = ui;
            PlacementFlags = default;
            SetFlags = default;
            UnsetFlags = default;
            SetState = null;
            UnsetState = null;
        }

        public ToolDefinition(Type toolSystemType, string toolId, UI ui)
            : this(toolSystemType, toolId, 60, ui) { }

        public struct UI
        {
            // We pack images with webpack so they end up under our mod’s COUI path.
            // Keep the folder names exactly as in your project tree.
            public const string PathPrefix = "coui://AdvancedRoadTools/UI/images/Tool_Icon/";
            public const string ImageFormat = ".png";

            public string ImagePath;

            public UI(string imagePath)
            {
                ImagePath = imagePath;
            }
        }

        public bool Equals(ToolDefinition other) => ToolID == other.ToolID;
        public override bool Equals(object obj) => obj is ToolDefinition other && Equals(other);
        public override int GetHashCode() => ToolID != null ? ToolID.GetHashCode() : 0;
    }
}
