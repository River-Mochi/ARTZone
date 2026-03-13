// File: src/Localization/LocaleEN.cs
// Purpose: English (en-US) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleEN(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Legacy" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "About" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zone Options" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),      "USAGE" },

                // Legacy group header hidden (tab should show only the toggle)
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Do not reset existing zoned squares" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Do not reset already zoned cells during preview/apply.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent buildings from being removed" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Buildings = occupied cells**. Prevents preview/apply of new zones from turning existing buildings into condemned.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Toggle Update Panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Show the Easy Zoning panel (**default Ctrl+V**)." },

                // USAGE toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Show Instructions" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Show or hide the **usage instructions** below."
                },

                // Multiline body is localized via the LABEL field (CitizenCleaner style)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<New Road>\n" +
                    "1. Open Roads panel (pick a road).\n" +
                    "2. At bottom of the road tool panel: pick one of the 3 EZ icons.\n" +
                    "3. Draw as usual.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB ~ right-click, LMB ~ left-click\n" +
                    "-----------------------------------------\n\n" +
                    "<Existing Road>\n" +
                    "1. Open EZ Update panel: click <Ctrl+V> to turn the panel On/Off \n" +
                    "   (or <top-left icon> does the same).\n" +
                    "2. Select a zone icon from the bottom panel (e.g. Left-side only)\n" +
                    "3. Hover + preview a road.\n" +
                    "4. <RMB cycles>: Both → Left → Right → None → ...\n" +
                    "5. <LMB one time>: applies (locks it in).\n" +
                    "6. <LMB hold + drag> along many road sections, release to apply.\n" +
                    "7. <Cancel:> move mouse away and release **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<OPTIONAL BUTTON>\n" +
                    "•  <Contour> shows terrain elevation lines."
                },

                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Legacy right-click cycle" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Recommend OFF** so that RMB cycles all 4 modes:\n" +
                    "**Both → Left → Right → None → ...**\n\n" +
                    "Advantage: less need to move the mouse back to the tool panel.\n\n" +
                    "--------------------------------------\n" +
                    "If Legacy is ON: RMB toggles in two separate sets:\n" +
                    "Left ↔ Right\n" +
                    "Both ↔ None\n" +
                    "For players who might want a limited right-click"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Toggle Easy Zoning Update Panel" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod name" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Display name of this mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Current mod version." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Open the author's Paradox Mods page." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Join the mod Discord." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
