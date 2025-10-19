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
        private readonly Setting m_Setting;
        public LocaleDE(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { m_Setting.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Aktionen" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "Über" },

                // Groups (Actions tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Optionen des Zonierungswerkzeugs" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Tastenbelegung" },

                // Groups (About tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "Links" },

                // Toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Verhindere das Entfernen zonierter Felder" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Verhindert das Überschreiben vorhandener Zonen während der Vorschau oder Anwendung." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Verhindere das Entfernen belegter Felder" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Verhindert das Überschreiben belegter Felder mit Gebäuden." },

                // Keybind
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Maustaste zum Umdrehen der Zonierung" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Weise eine Maustaste zu, um die Zonierungsrichtung umzuschalten." },
                { m_Setting.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Zonierung umkehren" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Zonierungssteuerung" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Steuert, wie die Zonierung entlang einer Straße funktioniert.\nWähle zwischen beiden Seiten, nur links, nur rechts oder keiner.\nRechtsklick zum Umdrehen der Einstellung." },

                // About tab link buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Öffne die Paradox-Mods-Seite im Browser." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Öffne den ART-Zone-Discord im Browser." },
            };
        }

        public void Unload()
        {
        }
    }
}
