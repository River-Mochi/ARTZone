// Tools/ToolDefinition.cs
// Definition of a tool button + metadata. Uses SVG icon.

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
            Type = systemType;
            ToolID = id;
            Priority = priority;
            this.ui = ui ?? new UI();   // default SVG icon
            SetState = _ => { };
        }

        public sealed class UI
        {
            // COUI root is the UI package id from UI/mod.json ("AdvancedRoadTools").
            // Webpack emits: images/ZoneControllerTool.svg
            public const string IconPath = "coui://AdvancedRoadTools/images/ZoneControllerTool.svg";

            public string ImagePath
            {
                get; set;
            }

            public UI()
            {
                ImagePath = IconPath;
            }

            public UI(string imagePath)
            {
                ImagePath = imagePath;
            }
        }
    }
}
