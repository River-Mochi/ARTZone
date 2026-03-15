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
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Legacy" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Info" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "Opzioni di zonizzazione" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Tasti rapidi" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilità" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Interfaccia" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USO" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Non reimpostare le caselle già zonizzate" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Non reimposta le celle già zonizzate durante anteprima/applicazione.\n\n" +
                    "**[ ✓ ] Attivato consigliato.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Impedisci la rimozione degli edifici" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edifici = celle occupate**. Impedisce che anteprima/applicazione di nuove zone trasformi edifici esistenti in edifici condannati.\n\n" +
                    "**[ ✓ ] Attivato consigliato.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Attiva/disattiva pannello di aggiornamento" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Mostra il pannello Easy Zoning (**Ctrl+V predefinito**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Pulsante contorni" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] attivato**, mostra il pulsante Contorni nel pannello Easy Zoning per strade esistenti.\n\n" +
                    "Disattivare se un altro mod gestisce già le linee di contorno del terreno." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Stile pannello trasparente" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] attivato**, usa uno stile pannello traslucido più chiaro.\n" +
                    "**[   ] disattivato**, usa un pannello più scuro in stile vanilla.\n\n" +
                    "Solo stile visivo. Nessun effetto blur viene usato." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Mostra istruzioni" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Mostra o nasconde le **istruzioni d'uso** qui sotto." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nuova strada>\n" +
                    "1. Aprire il pannello Strade (scegliere una strada).\n" +
                    "2. In fondo al pannello dello strumento strada: scegliere una delle 3 icone zona.\n" +
                    "3. Disegnare normalmente.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = clic destro, LMB = clic sinistro\n" +
                    "-----------------------------------------\n\n" +
                    "<Strada esistente>\n" +
                    "1. Aprire il pannello EZ Update: fare clic su <Ctrl+V> per attivare/disattivare il pannello\n" +
                    "   (oppure <l'icona in alto a sinistra> fa la stessa cosa).\n" +
                    "2. Selezionare un'icona zona dal pannello in basso.\n" +
                    "3. Passare sopra una strada + vedere l'anteprima.\n" +
                    "4. <RMB cambia modalità>: Entrambi → Sinistra → Destra → Nessuno → ...\n" +
                    "5. <LMB una volta>: applica (blocca la scelta).\n" +
                    "6. <Tenere premuto LMB + trascinare> lungo più segmenti di strada, poi rilasciare per applicare.\n" +
                    "7. <Annulla:> spostare il mouse via e rilasciare **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<PULSANTE OPZIONALE>\n" +
                    "• <Contorni> mostra le linee di elevazione del terreno." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo legacy con clic destro" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF consigliato** così RMB scorre tutti e 4 i modi:\n" +
                    "**Entrambi → Sinistra → Destra → Nessuno → ...**\n\n" +
                    "Vantaggio: meno bisogno di riportare il mouse al pannello strumento.\n\n" +
                    "--------------------------------------\n" +
                    "Se Legacy è ON: RMB alterna tra due gruppi separati:\n" +
                    "Sinistra ↔ Destra\n" +
                    "Entrambi ↔ Nessuno" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Attiva/disattiva pannello Easy Zoning Update" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome visualizzato di questo mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versione" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versione attuale del mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Apri la pagina Paradox Mods dell'autore." },
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
