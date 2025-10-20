// File: src/Settings/LocaleEN.cs
/// <summary>
/// English locale (en-US)
/// </summary>
namespace ARTZone
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleEN(Setting setting)
        {
            s_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "About" },

                // Groups
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zone Controller Tool Options" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Prevent zoned cells from being overridden during preview and apply phases." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Prevent occupied cells from being overridden during preview and apply phases." },

                // Keybinds
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Bind a mouse button to invert zoning while the tool is active." },

                // Shift+Z (Toggle tool)
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Toggle Zone Controller Tool" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Open/close the Zone Controller tool." },

                // Binding titles in the binding dialog
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Invert Zoning" },
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName),   "Toggle Zone Controller Tool" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Zone Controller" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Control how zoning behaves along a road.\nChoose zoning on both sides, only left or right, or none.\nRight-click (by default) inverts the configuration." },

                // About — read-only labels
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)), "Mod Name" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),  "Display name of this mod." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Current mod version." },

#if DEBUG
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "Informational Version" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "Mod Version with Commit ID" },
#endif

                // About tab links
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Open the Paradox Mods page in your browser." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Open the Discord for ART-Zone in your browser." },
            };
        }

        public void Unload()
        {
        }
    }
}
