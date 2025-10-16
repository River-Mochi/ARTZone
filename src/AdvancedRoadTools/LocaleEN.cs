// File: src/AdvancedRoadTools/LocaleEN.cs
// English strings for Options UI and tool asset names.

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
                // Mod + Tab
                { setting.GetSettingsLocaleID(), "Advanced Road Tools" },
                { setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                // Group headers in order
                { setting.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zone Controller Tool Options" },
                { setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key binding" },

                // Toggles
                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)),    "Prevent zoned cells from being removed" },
                { setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),     "Prevent zoned cells from being overridden during preview and apply phases of the tool." },

                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Prevent occupied cells from being overridden during preview and apply phases of the tool." },

                // Binding row
                { setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Bind a mouse button to invert zoning while the tool is active." },

                // Action title (shown in binding UI)
                { setting.GetBindingKeyLocaleID(AdvancedRoadToolsMod.kInvertZoningActionName), "Invert Zoning" },

                // Tool asset strings (palette name/tooltip)
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Zone Controller" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Control how zoning behaves along a road.\nChoose both sides, left, right, or none.\nRight-click (by default) inverts the current zoning configuration." },
            };
        }

        public void Unload()
        {
        }
    }
}
