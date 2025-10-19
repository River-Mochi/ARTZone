// File: src/AdvancedRoadTools/LocaleEN.cs
// Built-in English strings for Options UI + tool palette text.

#nullable enable
namespace AdvancedRoadTools
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting m_Setting)
        {
            this.m_Setting = m_Setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "ART-Zone" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Actions" },

                { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Zone Controller Tool Options" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being overridden during preview and apply phases." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being overridden during preview and apply phases." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)), "Bind a mouse button to invert zoning while the tool is active." },

                { m_Setting.GetBindingKeyLocaleID(AdvancedRoadToolsMod.kInvertZoningActionName), "Invert Zoning" },

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
