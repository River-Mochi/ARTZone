// File: src/AdvancedRoadTools/Tools/ToolDefinition.cs
// Tool metadata. Palette icon uses PNG emitted by webpack.

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

        public Action<bool> SetState
        {
            get; set;
        }

        public ToolDefinition(Type systemType, string id, int priority, UI ui)
        {
            Type = systemType;
            ToolID = id;
            Priority = priority;
            this.ui = ui ?? new UI(); // default icon
            SetState = _ => { };
        }

        public sealed class UI
        {
            // Webpack emits: images/ToolsIcon.png  (copied from UI/images/Tool_Icon/ToolsIcon.png)
            public const string IconPath = "coui://AdvancedRoadTools/images/ToolsIcon.png";

            public string ImagePath
            {
                get; set;
            }

            public UI()
            {
                ImagePath = IconPath;
            }
            public UI(string path)
            {
                ImagePath = path;
            }
        }
    }
}
