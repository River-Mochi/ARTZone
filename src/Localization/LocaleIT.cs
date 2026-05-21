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
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Tasti rapidi" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilità" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Aspetto" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USO" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Proteggi edifici dalla rimozione" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edifici = celle occupate**. Evita che anteprima/applica rendano gli edifici abbandonati.\n\n" +
                    "**[ ✓ ] Consigliato attivo.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Proteggi quadretti già dipinti/zonati" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Non resetta le celle già zonate durante anteprima/applica.\n\n" +
                    "**[ ✓ ] Consigliato attivo.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Pannello EZ On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Tasto rapido** per mostrare subito il pannello Easy Zoning\n" +
                    "**predefinito Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "Curve di livello" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "Mostra pulsante" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] attivo**, mostra il pulsante Curve di livello nel pannello per strade esistenti.\n\n" +
                    "● Disattiva se preferisci un pannello più piccolo o se un altro mod gestisce le curve di livello." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Pannello vetro" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] attivo**, usa uno stile traslucido più chiaro per il pannello.\n" +
                    "**[   ] disattivato** = pannello grigio.\n\n" +
                    "<Solo stile visivo.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Colore bordo: anteprima rimozioni" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Colore del bordo per l’anteprima delle celle da rimuovere.\n\n" +
                    "<Arancione> = più luminoso e facile da vedere.\n" +
                    "<Rosso> = contrasto rosso più forte.\n" +
                    "<Rosa> = colore vivace e divertente.\n" +
                    "<Viola> = contrasto morbido ma visibile.\n" +
                    "<Rosso vanilla> = come l’aspetto predefinito del gioco." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Opacità bordo" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Regola l’opacità del bordo dell’anteprima di rimozione.\n\n" +
                    "<100%> mantiene la normale trasparenza dell’anteprima.\n" +
                    "<0%> nasconde il bordo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Colore riempimento: anteprima rimozioni" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Stile del colore di riempimento per l’anteprima delle celle rimovibili.\n\n" +
                    "<Rosso vanilla> = aspetto attuale del gioco.\n" +
                    "<Bianco> = contrasto più pulito.\n" +
                    "<Arancione> = abbina il bordo arancione.\n" +
                    "<Rosa> = colore vivace e divertente.\n" +
                    "<Viola> = contrasto morbido ma visibile.\n" +
                    "<Nessuno> = solo bordo, minimalista" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Opacità riempimento" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Regola l’opacità del riempimento per l’anteprima delle celle rimovibili.\n\n" +
                    "<100%> mantiene la normale trasparenza dell’anteprima.\n" +
                    "<0%> nasconde il riempimento.\n" +
                    "Ignorato se <Riempimento rimozione> è impostato su <Nessuno>." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "Alto contrasto" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "Sets\n" +
                    "<Pannello vetro On>\n" +
                    "<Bordo arancione>\n" +
                    "<100% opacità bordo>\n" +
                    "<Nessun riempimento.>" },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "Colore gioco" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "Usa bordo e riempimento rossi come l’anteprima dello strumento zone del gioco." },
  
                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "Arancione" },
                { "EasyZoning.Dropdown.Color.Red", "Rosso" },
                { "EasyZoning.Dropdown.Color.Pink", "Rosa" },
                { "EasyZoning.Dropdown.Color.Purple", "Viola" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "Rosso vanilla" },
                { "EasyZoning.Dropdown.Color.White", "Bianco" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "Nessuno (solo bordo)" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Mostra istruzioni" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Mostra o nasconde le **istruzioni d’uso** qui sotto." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Strade esistenti>\n" +
                    "1. Apri il pannello EZ Update: clicca <Ctrl+V> per attivare/disattivare il pannello\n" +
                    "   (<icona in alto a sinistra> fa lo stesso).\n" +
                    "2. Usa le 3 icone EZ per Entrambi / Sinistra / Destra.\n" +
                    "   Clicca di nuovo il pulsante per Nessuno.\n" +
                    "3. Passa sopra una strada + anteprima.\n" +
                    "4. Anteprima rossa = celle da rimuovere.\n" +
                    "5. <RMB cicla>: Entrambi → Sinistra → Destra → Nessuno → ...\n" +
                    "6. <LMB una volta>: applica (blocca la scelta).\n" +
                    "7. <Tieni LMB + trascina> lungo più sezioni stradali, rilascia per applicare.\n" +
                    "8. <Annulla:> sposta via il mouse e rilascia **LMB**.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = clic destro, <LMB> = clic sinistro\n" +
                    "-----------------------------------------\n\n" +
                    "<Nuova strada>\n" +
                    "1. Apri il pannello Strade (scegli una strada).\n" +
                    "2. In basso nel pannello dello strumento strada: usa le 3 icone EZ per Entrambi / Sinistra / Destra.\n" +
                    "   Clicca di nuovo il pulsante per Nessuno.\n" +
                    "3. Disegna come al solito.\n\n" +
                    "-------------------------------------------\n" +
                    "<Pulsante terreno>\n" +
                    "<◎ Curve di livello> mostra le linee di elevazione del terreno."
                },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo classico col clic destro" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Non consigliato**\n" +
                    "OFF usa il metodo moderno: RMB cicla tutti e 4 i modi: **Entrambi → Sinistra → Destra → Nessuno → ...**\n\n" +
                    "Vantaggio: meno bisogno di riportare il mouse al pannello strumenti.\n\n" +
                    "<-------------------------------------->\n" +
                    "Se Classico è ON: RMB alterna in due gruppi separati e richiede più movimenti del mouse:\n" +
                    "Solo Sinistra ↔ Destra\n" +
                    "Solo Entrambi ↔ Nessuno"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Attiva/disattiva pannello Easy Zoning Update" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome visualizzato di questo mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versione" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versione attuale del mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Apri la pagina Paradox Mods dell’autore." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Entra nel Discord del mod." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
