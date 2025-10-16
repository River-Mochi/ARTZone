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

        // NOTE: ToolsHelper reads definition.ui.ImagePath for UIObject.m_Icon
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
            // If no UI is provided, use the default icon path.
            this.ui = ui ?? new UI();
            SetState = _ => { };
        }

        public sealed class UI
        {
            // COUI root is the UI package id from UI/mod.json ("AdvancedRoadTools").
            // Webpack emits: images/ToolsIcon.png  (from images/Tool_Icon/ToolsIcon.png)
            public const string IconPath = "coui://AdvancedRoadTools/images/ToolsIcon.png";

            // This is what ToolsHelper assigns to UIObject.m_Icon.
            public string ImagePath
            {
                get; set;
            }

            // Default to the emitted icon path.
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
