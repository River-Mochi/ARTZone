// Tools/ToolDefinition.cs
// Definition of a tool button + metadata (no PlacementFlags dependency)

namespace AdvancedRoadTools.Tools
{
    using System;

    public sealed class ToolDefinition
    {
        public Type Type
        {
            get;
        }
        public string ToolID
        {
            get;
        }
        public int Priority
        {
            get;
        }

        public UI ui
        {
            get;
        }

        // Optional callback a system can use to reflect enabled/disabled state in UI.
        public Action<bool> SetState
        {
            get; set;
        }

        public ToolDefinition(Type systemType, string id, int priority, UI ui)
        {
            this.Type = systemType;
            this.ToolID = id;
            this.Priority = priority;
            this.ui = ui ?? new UI(UI.IconPath);
            this.SetState = _ => { };
        }

        public sealed class UI
        {
            // This resolves against your UI package id from UI/mod.json
            public const string IconPath = "coui://AdvancedRoadTools/images/Tool Icon/ToolsIcon.png";

            public string ImagePath
            {
                get; set;
            }

            public UI(string imagePath)
            {
                ImagePath = imagePath;
            }
        }
    }
}
