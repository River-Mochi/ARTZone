// Tools/ToolDefinition.cs
// Definition of a tool button + metadata. C# icon path matches webpack output.

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
            this.ui = ui ?? new UI();   // default icon
            SetState = _ => { };
        }

        public sealed class UI
        {
            // Webpack places the file at Mods/<id>/images/ToolsIcon.png and it is served under:
            // coui://ui-mods/<id>/images/ToolsIcon.png
            public const string IconPath = "coui://ui-mods/AdvancedRoadTools/images/ToolsIcon.png";

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
