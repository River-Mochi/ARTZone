// File: src/Settings/LocaleEN.cs
// English (en-US) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleEN(Setting setting) => s_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART — Zoning" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "About" },

                // Groups
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zone Tool Options" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Do not override already zoned cells during preview/apply.\n Recommend Enabled." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Do not override already occupied cells during preview/apply.\n Recommend Enabled" },

                // Keybind (only one visible)
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Toggle Zone Control panel" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Show or hide the ART Zone Controller panel (default Shift+Z)." },

                // Binding title in the keybinding dialog
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "Toggle Zone Control panel" },

                // Palette (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Zone Changer" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Change zoning on roads: both sides, left, right, or none. Right-click flips the choice. Left Click confirms." },

                // About tab labels
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod name" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Display name of this mod." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Current mod version." },
#if DEBUG
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "Informational version" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "Version + build info" },
#endif
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Open the Paradox Mods page." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Join the mod Discord." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
