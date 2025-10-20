// File: src/LocaleDE.cs
/// <summary>
/// German locale (de-DE)
/// </summary>
namespace ARTZone
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleDE : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleDE(Setting setting)
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
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Aktionen" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Über" },

                // Groups (Actions tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Optionen des Zonierungswerkzeugs" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Tastenbelegung" },

                // Groups (About tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "Links" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Verhindere das Entfernen zonierter Felder" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Verhindert das Überschreiben vorhandener Zonen während der Vorschau oder Anwendung." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Verhindere das Entfernen belegter Felder" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Verhindert das Überschreiben belegter Felder mit Gebäuden." },

                // Keybind
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Maustaste zum Umdrehen der Zonierung" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Weise eine Maustaste zu, um die Zonierungsrichtung umzuschalten." },
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Zonierung umkehren" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Zonierungssteuerung" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Steuert, wie die Zonierung entlang einer Straße funktioniert.\nWähle zwischen beiden Seiten, nur links, nur rechts oder keiner.\nRechtsklick zum Umdrehen der Einstellung." },

                // About tab link buttons
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Öffne die Paradox-Mods-Seite im Browser." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Öffne den ART-Zone-Discord im Browser." },
            };
        }

        public void Unload()
        {
        }
    }
}
