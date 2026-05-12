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
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Protections" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Key bindings" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibility" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Visuals" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USAGE" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Prevent removal of buildings" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Buildings = occupied cells**. Prevents preview/apply of new zones from turning existing buildings into condemned.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Prevent reset of already painted/zoned squares" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Do not reset already zoned cells during preview/apply.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Update Panel On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Keybind** to show the Easy Zoning panel quickly\n" +
                    "**default Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Contour button" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] enabled**, show the Contour terrain button in the mod's existing roads update panel.\n\n" +
                    "● Disable this if a smaller panel is preferred or another mod handles terrain lines." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Glass panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] enabled**, use a clear translucent style for the panel.\n" +
                    "**[   ] disabled**, use a gray panel.\n\n" +
                    "Visual style only." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Remove cells border" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Border color for cells to be removed preview.\n\n" +
                    "<Orange> = brighter and easier to see.\n" +
                    "<Red> = stronger red border for grass/trees.\n" +
                    "<Vanilla red> = match the game's default look." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Border opacity" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Adjusts the remove-preview border opacity.\n\n" +
                    "<100%> keeps the preview's normal translucency." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Remove cells fill" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Fill style for cells to be removed preview overlay.\n\n" +
                    "<Vanilla red> = current game look.\n" +
                    "<White> = cleaner contrast.\n" +
                    "<Orange> = matches the orange border.\n" +
                    "<None> = border only." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Fill opacity" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Adjusts the fill opacity for preview of removeable cells.\n\n" +
                    "<100%> keeps the preview's normal translucency.\n" +
                    "Ignored if <Remove fill> is set to <None>." },

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
