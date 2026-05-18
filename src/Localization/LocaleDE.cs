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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Aktionen" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Klassisch" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Info" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Schutzfunktionen" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Tastenbelegung" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Kompatibilität" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Optik" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "NUTZUNG" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Entfernen von Gebäuden verhindern" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Gebäude = belegte Zellen**. Verhindert, dass Vorschau/Anwenden Gebäude als Abrissfall markiert.\n\n" +
                    "**[ ✓ ] Aktiviert empfohlen.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Zurücksetzen bereits gemalter/gezonter Felder verhindern" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Bereits gezonte Zellen werden bei Vorschau/Anwenden nicht zurückgesetzt.\n\n" +
                    "**[ ✓ ] Aktiviert empfohlen.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Update-Panel An/Aus" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Tastenbelegung**, um das Easy Zoning-Panel schnell anzuzeigen\n" +
                    "**Standard Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "Konturlinien" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "Schaltfläche anzeigen" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] aktiviert**, zeigt die Contour-Gelände-Schaltfläche im Update-Panel für bestehende Straßen des Mods.\n\n" +
                    "● Deaktivieren, wenn ein kleineres Panel gewünscht ist oder ein anderer Mod Geländelinien übernimmt." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Glas-Panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] aktiviert**, verwendet einen klaren transluzenten Stil für das Panel.\n" +
                    "**[   ] deaktiviert**, verwendet ein graues Panel.\n\n" +
                    "Nur visueller Stil." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Randfarbe: Vorschau-Entfernung" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Randfarbe für die Vorschau der zu entfernenden Zellen.\n\n" +
                    "<Orange> = heller und leichter zu erkennen.\n" +
                    "<Rot> = stärkerer roter Randkontrast.\n" +
                    "<Vanilla-Rot> = entspricht der Standardoptik des Spiels." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Randdeckkraft" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Passt die Deckkraft des Entfernen-Vorschau-Rands an.\n\n" +
                    "<100%> behält die normale Transparenz der Vorschau.\n" +
                    "<0%> blendet den Rand aus." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Füllfarbe: Vorschau-Entfernung" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Füllfarbstil für die Vorschau von Zellen, die entfernt werden können.\n\n" +
                    "<Vanilla-Rot> = aktuelle Spieloptik.\n" +
                    "<Weiß> = klarerer Kontrast.\n" +
                    "<Orange> = passt zum orangefarbenen Rand.\n" +
                    "<Keine> = nur Rand, minimalistisch" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Fülldeckkraft" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Passt die Fülldeckkraft für die Vorschau entfernbarer Zellen an.\n\n" +
                    "<100%> behält die normale Transparenz der Vorschau.\n" +
                    "<0%> blendet die Füllung aus.\n" +
                    "Wird ignoriert, wenn <Entfernen-Füllung> auf <Keine> steht." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "Hoher Kontrast" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "Schaltet Glaspanel EIN, orangefarbenen Rand, 100% Randdeckkraft und keine Füllung ein." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "Spielfarbe" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "Verwendet roten Rand und rote Füllung wie die Vorschau des Zoning-Tools im Spiel." },

                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "Orange" },
                { "EasyZoning.Dropdown.Color.Red", "Rot" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "Vanilla-Rot" },
                { "EasyZoning.Dropdown.Color.White", "Weiß" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "Keine (nur Rand)" },            

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Anleitung anzeigen" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Zeigt oder verbirgt die **Nutzungsanleitung** unten." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Neue Straße>\n" +
                    "1. Straßen-Panel öffnen (eine Straße wählen).\n" +
                    "2. Unten im Straßenwerkzeug-Panel: die 3 EZ-Symbole für Beide / Links / Rechts verwenden.\n" +
                    "   Den ausgewählten Button erneut anklicken für Keine.\n" +
                    "3. Wie gewohnt zeichnen.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = Rechtsklick, <LMB> = Linksklick\n" +
                    "-----------------------------------------\n\n" +
                    "<Bestehende Straße>\n" +
                    "1. EZ Update-Panel öffnen: <Ctrl+V> klicken, um das Panel An/Aus zu schalten\n" +
                    "   (<Symbol oben links> macht dasselbe).\n" +
                    "2. Die 3 EZ-Symbole für Beide / Links / Rechts verwenden.\n" +
                    "   Den Button erneut anklicken für Keine.\n" +
                    "3. Straße anvisieren + Vorschau anzeigen.\n" +
                    "4. Rote Vorschau = zu entfernende Zellen.\n" +
                    "5. <RMB wechselt>: Beide → Links → Rechts → Keine → ...\n" +
                    "6. <LMB einmal>: anwenden (fixiert es).\n" +
                    "7. <LMB halten + ziehen> entlang vieler Straßenabschnitte, loslassen zum Anwenden.\n" +
                    "8. <Abbrechen:> Maus wegbewegen und **LMB** loslassen.\n\n" +
                    "-------------------------------------------\n" +
                    "<OPTIONALER BUTTON>\n" +
                    "<◎ Höhenlinien> zeigt Geländehöhenlinien." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Klassischer Rechtsklick-Zyklus" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF wird empfohlen**\n" +
                    "OFF bedeutet, dass RMB alle 4 Modi durchschaltet: **Beide → Links → Rechts → Keine → ...**\n\n" +
                    "Deaktivierter Vorteil: weniger Zurückbewegen der Maus zum Werkzeug-Panel nötig.\n\n" +
                    "--------------------------------------\n" +
                    "Wenn Klassisch ON ist: RMB schaltet in zwei getrennten Gruppen um:\n" +
                    "Nur Links ↔ Rechts\n" +
                    "Nur Beide ↔ Keine"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning Update-Panel umschalten" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod-Name" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Anzeigename dieses Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Aktuelle Mod-Version." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Öffnet die Paradox Mods-Seite des Autors." },
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
