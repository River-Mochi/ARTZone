// File: src/AdvancedRoadTools/LocaleEN.cs
// Built-in English strings for Options UI + tool palette text.

#nullable enable
namespace AdvancedRoadTools
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting setting;
        public LocaleEN(Setting setting)
        {
            this.setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { setting.GetSettingsLocaleID(), "Advanced Road Tools" },
                { setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Zone Controller Tool Options" },
                { setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },

                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being overridden during preview and apply phases." },

                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being overridden during preview and apply phases." },

                { setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)), "Bind a mouse button to invert zoning while the tool is active." },

                { setting.GetBindingKeyLocaleID(AdvancedRoadToolsMod.kInvertZoningActionName), "Invert Zoning" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Zone Controller" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Control how zoning behaves along a road.\nChoose zoning on both sides, only left or right, or none.\nRight-click (by default) inverts the configuration." },
            };
        }

        public void Unload()
        {
        }
    }
}
