// File: src/AdvancedRoadTools/Settings/LocaleEN.cs
// English en-US for the Settings UI + asset/palette text.

namespace AdvancedRoadTools.Settings
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting _settings;
        public LocaleEN(Setting setting)
        {
            _settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { _settings.GetSettingsLocaleID(), "Advanced Road Tools" },

                // Tabs
                { _settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { _settings.GetOptionTabLocaleID(Setting.kAboutTab),   "About" },

                // Groups
                { _settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zone Controller Tool Options" },
                { _settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },
                { _settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { _settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { _settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { _settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Prevent zoned cells from being overridden during preview and apply phases." },

                { _settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { _settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Prevent occupied cells from being overridden during preview and apply phases." },

                // Keybinds
                { _settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning (Mouse)" },
                { _settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Hold the bound mouse button to invert zoning while the tool is active." },

                // Shift+Z (Toggle tool)
                { _settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Toggle Zone Controller Tool" },
                { _settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Open/close the Zone Controller tool (default: Shift+Z)." },

                // Binding titles in the binding dialog
                { _settings.GetBindingKeyLocaleID(AdvancedRoadToolsMod.kInvertZoningActionName), "Invert Zoning" },
                { _settings.GetBindingKeyLocaleID(AdvancedRoadToolsMod.kToggleToolActionName),   "Toggle Zone Controller Tool" },

                // Palette/asset text (Road Services tile)
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Zone Controller" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Control how zoning behaves along a road.\nChoose zoning on both sides, only left or right, or none.\nRight-click (by default) inverts the configuration." },

                // About — read-only labels
                { _settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod Name" },
                { _settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Display name of this mod." },
                { _settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { _settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Current mod version." },
            };
        }

        public void Unload()
        {
        }
    }
}
