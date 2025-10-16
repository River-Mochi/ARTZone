// File: src/AdvancedRoadTools/Tools/ToolDefinition.cs
// Tool metadata. Palette icon comes from COUI (copied by .csproj from UI/images/**).

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

        public System.Action<bool> SetState
        {
            get; set;
        }

        public ToolDefinition(Type systemType, string id, int priority, UI ui)
        {
            Type = systemType;
            ToolID = id;
            Priority = priority;
            this.ui = ui ?? new UI();
            SetState = _ => { };
        }

        public sealed class UI
        {
            // Correct COUI path (single slash after mod id).
            public const string DefaultIconCouiPath = "coui://AdvancedRoadTools/UI/images/Tool_Icon/ToolsIcon.png";

            public string? ImagePath
            {
                get; set;
            }
            public string? SpriteName
            {
                get; set;
            } // Optional if you map sprites in UI/mod.json

            public UI()
            {
                ImagePath = DefaultIconCouiPath;
            }
            public UI(string iconCouiPath)
            {
                ImagePath = iconCouiPath;
            }

            public static UI FromSprite(string spriteName) => new UI { ImagePath = null, SpriteName = spriteName };
        }
    }
}
