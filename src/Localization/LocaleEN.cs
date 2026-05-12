// File: src/Localization/LocaleEN.cs
// Purpose: English (en-US) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleEN(Setting setting)
        {
            m_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            string title = Mod.ModName;
            if (!string.IsNullOrEmpty(Mod.ModVersion))
            {
                title = title + " (" + Mod.ModVersion + ")";
            }

            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title
                { m_Settings.GetSettingsLocaleID(), title },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Legacy" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "About" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "Zone Options" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Key bindings" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibility" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "UI" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USAGE" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Prevent buildings from being removed" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Buildings = occupied cells**. Prevents preview/apply of new zones from turning existing buildings into condemned.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Do not reset existing zoned squares" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Do not reset already zoned cells during preview/apply.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Toggle Update Panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Show the Easy Zoning panel (**default Ctrl+V**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Contour button" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] enabled**, show the Contour button in the Easy Zoning existing-roads panel.\n\n" +
                    "● Disable this if a smaller panel is preferred or another mod handles terrain contour lines." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Glass panel style" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] enabled**, use the clearer translucent panel style.\n" +
                    "**[   ] disabled**, use a gray panel.\n\n" +
                    "Visual style only." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseOrangeRemovePreviewEdge)), "◉ Orange remove-preview edge" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseOrangeRemovePreviewEdge)),
                    "**[ ✓ ] enabled**, use a brighter orange border for cells that will be removed.\n" +
                    "**[   ] disabled**, keep the vanilla red border.\n\n" +
                    "Only changes the remove-preview border. Fill stays vanilla for now." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Remove-preview edge opacity" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Adjusts only the orange remove-preview border opacity.\n\n" +
                    "Does not change normal zoning colors or the white add-preview cells." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Show Instructions" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Show or hide the **usage instructions** below." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<New Road>\n" +
                    "1. Open Roads panel (pick a road).\n" +
                    "2. Bottom of road tool panel: use the 3 EZ icons for Both / Left / Right.\n" +
                    "   Click the selected button again for None.\n" +
                    "3. Draw as usual.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = right-click, <LMB> = left-click\n" +
                    "-----------------------------------------\n\n" +
                    "<Existing Road>\n" +
                    "1. Open EZ Update panel: click <Ctrl+V> to turn the panel On/Off\n" +
                    "   (<top-left icon> does the same).\n" +
                    "2. Use the 3 EZ icons for Both / Left / Right.\n" +
                    "   Click the button again for None.\n" +
                    "3. Hover + preview a road.\n" +
                    "4. Red preview = cells to be removed.\n" +
                    "5. <RMB cycles>: Both → Left → Right → None → ...\n" +
                    "6. <LMB one time>: applies (locks it in).\n" +
                    "7. <LMB hold + drag> along many road sections, release to apply.\n" +
                    "8. <Cancel:> move mouse away and release **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<OPTIONAL BUTTON>\n" +
                    "• <Contour> shows terrain elevation lines." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Legacy right-click cycle" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF is recommended**\n" +
                    "Off means RMB cycles all 4 modes: **Both → Left → Right → None → ...**\n\n" +
                    "Disabled Advantage: less need to move the mouse back to the tool panel.\n\n" +
                    "--------------------------------------\n" +
                    "If Legacy is ON: RMB toggles in two separate sets:\n" +
                    "Left ↔ Right only\n" +
                    "Both ↔ None only"
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
