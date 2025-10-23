// English (en-US) for the Options UI + asset/palette text

namespace AdvancedRoadTools
{
    using System.Collections.Generic;
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
                // Options > Settings title
                { s_Settings.GetSettingsLocaleID(), "ART — Zoning" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "About" },

                // Groups
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zone Controller Options" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Prevent zoned cells from being overridden during preview and apply phases.\n Enable is recommended." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Prevent occupied cells from being overridden during preview and apply phases.\n Enable is recommended." },

                // Keybinds
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning (mouse)" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Click (right mouse button default) to invert left or right side while the zoning tool is active." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Toggle Zone Control panel" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Show/hide the Zone Control panel." },

                // Binding titles in the binding dialog
                { s_Settings.GetBindingKeyLocaleID(AdvancedRoadToolsMod.kInvertZoningActionName), "Invert Zoning" },
                { s_Settings.GetBindingKeyLocaleID(AdvancedRoadToolsMod.kToggleToolActionName),   "Toggle Zone Control Panel" },

                // Palette (Road Services tile) — ensure ToolID matches instantiated prefab name
                { $"Assets.NAME[{Systems.ZoningControllerToolSystem.ToolID}]", "Zone Changer" },
                { $"Assets.DESCRIPTION[{Systems.ZoningControllerToolSystem.ToolID}]",
                  "Change zoning on desired side of roads: both, left, right, or none.\n Right-click inverts the choice." },

                // About tab
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod Name" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Display name of this mod." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Current mod version." },
#if DEBUG
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "Informational Version" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "Version + commit id" },
#endif
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Open the Paradox Mods page in your browser." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Join the Discord for this mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
