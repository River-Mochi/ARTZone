// File: src/Settings/LocaleEN.cs
// English (en-US) strings for Options UI + Panel text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleEN(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "ART — Zoning" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "About" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zone Options" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Do not change already zoned cells during preview/apply.\n" +
                "<Enabled recommended.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Do not change already occupied cells during preview/apply (e.g. buildings).\n" +
                "<Enabled recommended>." },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Toggle Panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Show the ART-Zone panel button (default Shift+Z)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "Toggle Zone Button Panel" },

                // Panel (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Zone Changer" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Change zoning on roads: both sides, left, right, or none.\n" +
                  "Right-click flips the choice. Left-click confirms." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod name" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Display name of this mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Current mod version." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Open the Paradox Mods page." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Join the mod Discord." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
