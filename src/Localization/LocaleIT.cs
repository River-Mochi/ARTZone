// File: src/Localization/LocaleIT.cs
// Purpose: Italian (it-IT) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleIT : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleIT(Setting setting)
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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Azioni" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Classico" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Info" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Protezioni" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Scorciatoie da tastiera" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilità" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Aspetto" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USO" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Impedisci rimozione edifici" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edifici = celle occupate**. Impedisce ad anteprima/applica di rendere gli edifici da demolire.\n\n" +
                    "**[ ✓ ] Attivato consigliato.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Impedisci reset dei quadrati già dipinti/zonizzati" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Non reimposta le celle già zonizzate durante anteprima/applica.\n\n" +
                    "**[ ✓ ] Attivato consigliato.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Pannello aggiornamento On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Scorciatoia** per mostrare rapidamente il pannello Easy Zoning\n" +
                    "**predefinito Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Pulsante contorno" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] attivato**, mostra il pulsante terreno Contour nel pannello di aggiornamento strade esistenti del mod.\n\n" +
                    "● Disattivalo se preferisci un pannello più piccolo o se un altro mod gestisce le linee del terreno." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Pannello vetro" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] attivato**, usa uno stile traslucido chiaro per il pannello.\n" +
                    "**[   ] disattivato**, usa un pannello grigio.\n\n" +
                    "Solo stile visivo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Colore bordo: rimozioni in anteprima" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Colore del bordo per l’anteprima delle celle da rimuovere.\n\n" +
                    "<Arancione> = più brillante e più facile da vedere.\n" +
                    "<Rosso> = contrasto del bordo rosso più forte.\n" +
                    "<Rosso vanilla> = corrisponde all’aspetto predefinito del gioco." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Opacità bordo" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Regola l’opacità del bordo dell’anteprima di rimozione.\n\n" +
                    "<100%> mantiene la normale traslucenza dell’anteprima.\n" +
                    "<0%> nasconde il bordo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Colore riempimento: rimozioni in anteprima" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Stile colore di riempimento per l’anteprima delle celle che possono essere rimosse.\n\n" +
                    "<Rosso vanilla> = aspetto attuale del gioco.\n" +
                    "<Bianco> = contrasto più pulito.\n" +
                    "<Arancione> = corrisponde al bordo arancione.\n" +
                    "<Nessuno> = solo bordo, minimalista" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Opacità riempimento" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Regola l’opacità del riempimento per l’anteprima delle celle rimovibili.\n\n" +
                    "<100%> mantiene la normale traslucenza dell’anteprima.\n" +
                    "<0%> nasconde il riempimento.\n" +
                    "Ignorato se <Riempimento rimozione> è impostato su <Nessuno>." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Mostra istruzioni" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Mostra o nasconde le **istruzioni d’uso** qui sotto." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nuova strada>\n" +
                    "1. Apri il pannello Strade (scegli una strada).\n" +
                    "2. In basso nel pannello dello strumento strada: usa le 3 icone EZ per Entrambi / Sinistra / Destra.\n" +
                    "   Clicca di nuovo il pulsante selezionato per Nessuno.\n" +
                    "3. Disegna come al solito.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = clic destro, <LMB> = clic sinistro\n" +
                    "-----------------------------------------\n\n" +
                    "<Strada esistente>\n" +
                    "1. Apri il pannello EZ Update: clicca <Ctrl+V> per attivare/disattivare il pannello\n" +
                    "   (<icona in alto a sinistra> fa la stessa cosa).\n" +
                    "2. Usa le 3 icone EZ per Entrambi / Sinistra / Destra.\n" +
                    "   Clicca di nuovo il pulsante per Nessuno.\n" +
                    "3. Passa sopra + visualizza l’anteprima di una strada.\n" +
                    "4. Anteprima rossa = celle da rimuovere.\n" +
                    "5. <RMB scorre>: Entrambi → Sinistra → Destra → Nessuno → ...\n" +
                    "6. <LMB una volta>: applica (blocca la scelta).\n" +
                    "7. <Tieni LMB + trascina> lungo molte sezioni di strada, rilascia per applicare.\n" +
                    "8. <Annulla:> sposta via il mouse e rilascia **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<PULSANTE OPZIONALE>\n" +
                    "• <Contorno> mostra le linee di elevazione del terreno." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo classico con clic destro" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF è consigliato**\n" +
                    "OFF significa che RMB scorre tutti e 4 i modi: **Entrambi → Sinistra → Destra → Nessuno → ...**\n\n" +
                    "Vantaggio disattivato: meno bisogno di riportare il mouse al pannello dello strumento.\n\n" +
                    "--------------------------------------\n" +
                    "Se Classico è ON: RMB alterna in due gruppi separati:\n" +
                    "Solo Sinistra ↔ Destra\n" +
                    "Solo Entrambi ↔ Nessuno"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Attiva/disattiva pannello aggiornamento Easy Zoning" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome visualizzato di questo mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versione" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versione attuale del mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Apri la pagina Paradox Mods dell’autore." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Unisciti al Discord del mod." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
