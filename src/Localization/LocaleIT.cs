// File: src/Localization/LocaleIT.cs
// Purpose: Italian (it-IT) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocaleIT : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleIT(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Azioni" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Informazioni" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opzioni di zonizzazione" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Scorciatoie da tastiera" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "Comportamento legacy dello strumento" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Non ripristinare i quadrati già zonizzati" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Non ripristina le celle già zonizzate durante anteprima/applicazione.\n\n" +
                    "**[ ✓ ] Attivato consigliato.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Impedisci la rimozione degli edifici" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edifici = celle occupate**. Impedisce che anteprima/applicazione di nuove zone trasformi edifici esistenti in condannati.\n\n" +
                    "**[ ✓ ] Attivato consigliato.**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Attiva/disattiva pannello di aggiornamento" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Mostra il pannello Easy Zoning (**Shift+V predefinito**)."
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo RMB legacy" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF consigliato.**\n" +
                    "Quando è OFF, RMB (clic destro) può ciclare tutti e 4 i modi:\n" +
                    "Entrambi → Sinistra → Destra → Nessuno → ...\n\n" +
                    "Vantaggio: più veloce, meno bisogno di tornare al pannello con il mouse.\n\n" +

                    "**ON:** RMB alterna in due set separati:\n" +
                    "Sinistra ↔ Destra\n" +
                    "Entrambi ↔ Nessuno"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Attiva/disattiva pannello pulsanti Easy Zoning" },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "Cambia zonizzazione: entrambi i lati, sinistra<->destra, o nessuno.\n" +
                    "Clic sinistro conferma la scelta. Tieni premuto clic sinistro + trascina lungo una strada per aggiornare più segmenti." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome visualizzato di questa mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versione" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versione corrente della mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Apri la pagina Paradox Mods dell’autore." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Unisciti al Discord della mod." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
