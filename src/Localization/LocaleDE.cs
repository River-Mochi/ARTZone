// File: src/Localization/LocaleDE.cs
// Purpose: German (de-DE) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleDE : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleDE(Setting setting)
        {
            m_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Aktionen" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Klassisch" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Info" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "Zonenoptionen" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Tastenbelegung" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Kompatibilität" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Benutzeroberfläche" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "VERWENDUNG" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Bereits zonierte Felder nicht zurücksetzen" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Setzt bereits zonierte Zellen während Vorschau/Anwenden nicht zurück.\n\n" +
                    "**[ ✓ ] Aktiviert empfohlen.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Verhindern, dass Gebäude entfernt werden" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Gebäude = belegte Zellen**. Verhindert, dass Vorschau/Anwenden neuer Zonen vorhandene Gebäude zu abgerissenen Gebäuden macht.\n\n" +
                    "**[ ✓ ] Aktiviert empfohlen.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Aktualisierungspanel ein/ausblenden" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Das Easy-Zoning-Panel anzeigen (**Standard: Strg+V**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Kontur-Schaltfläche" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] aktiviert**, zeigt die Kontur-Schaltfläche im Easy-Zoning-Panel für bestehende Straßen an.\n\n" +
                    "Deaktivieren, wenn ein anderer Mod bereits Geländekonturlinien steuert." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Glas-Panelstil" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] aktiviert**, verwendet einen helleren transparenten Panelstil.\n" +
                    "**[   ] deaktiviert**, verwendet ein dunkleres Panel im Vanilla-Stil.\n\n" +
                    "Nur Optik. Es wird kein Blur verwendet." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Anleitung anzeigen" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Die **Anleitung** unten anzeigen oder ausblenden." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Neue Straße>\n" +
                    "1. Straßenmenü öffnen (eine Straße auswählen).\n" +
                    "2. Unten im Straßenwerkzeug-Menü eines der 3 Zonen-Symbole auswählen.\n" +
                    "3. Straße wie gewohnt zeichnen.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = Rechtsklick, LMB = Linksklick\n" +
                    "-----------------------------------------\n\n" +
                    "<Bestehende Straße>\n" +
                    "1. EZ-Aktualisierungspanel öffnen: <Strg+V> drücken, um das Panel ein-/auszublenden\n" +
                    "   (oder <das Symbol oben links> macht dasselbe).\n" +
                    "2. Ein Zonen-Symbol im unteren Panel auswählen.\n" +
                    "3. Eine Straße überfahren + Vorschau ansehen.\n" +
                    "4. <RMB wechselt>: Beide Seiten → Links → Rechts → Keine → ...\n" +
                    "5. <LMB einmal>: anwenden (festlegen).\n" +
                    "6. <LMB halten + ziehen> über mehrere Straßenabschnitte, dann loslassen zum Anwenden.\n" +
                    "7. <Abbrechen:> Maus wegbewegen und **LMB** loslassen.\n\n" +
                    "-------------------------------------------\n" +
                    "<OPTIONALE SCHALTFLÄCHE>\n" +
                    "• <Kontur> zeigt Geländehöhenlinien an." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Klassischer Rechtsklick-Zyklus" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF empfohlen**, damit RMB durch alle 4 Modi wechselt:\n" +
                    "**Beide Seiten → Links → Rechts → Keine → ...**\n\n" +
                    "Vorteil: Die Maus muss seltener zurück zum Werkzeugpanel bewegt werden.\n\n" +
                    "--------------------------------------\n" +
                    "Wenn Klassisch auf ON steht: RMB wechselt nur zwischen zwei getrennten Gruppen:\n" +
                    "Links ↔ Rechts\n" +
                    "Beide Seiten ↔ Keine" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy-Zoning-Aktualisierungspanel ein/ausblenden" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod-Name" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Anzeigename dieses Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Aktuelle Mod-Version." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Die Paradox-Mods-Seite des Autors öffnen." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Dem Mod-Discord beitreten." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
