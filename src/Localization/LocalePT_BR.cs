// File: src/Localization/LocalePT_BR.cs
// Purpose: Portuguese-BR (pt-BR) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocalePT_BR : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocalePT_BR(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Ações" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Sobre" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opções de zoneamento" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atalhos do teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "Comportamento legado da ferramenta" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Não redefinir quadrados já zoneados" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Não redefine células já zoneadas durante prévia/aplicar.\n\n" +
                    "**[ ✓ ] Ativado recomendado.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Impedir que edifícios sejam removidos" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edifícios = células ocupadas**. Impede que a prévia/aplicação de novas zonas transforme edifícios existentes em condenados.\n\n" +
                    "**[ ✓ ] Ativado recomendado.**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Alternar painel de atualização" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Mostrar o painel Easy Zoning (**padrão Ctrl+V**)."
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo RMB legado" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF é recomendado.**\n" +
                    "Quando OFF, RMB (clique direito) pode ciclar os 4 modos:\n" +
                    "Ambos → Esquerda → Direita → Nenhum → ...\n\n" +
                    "Vantagem: mais rápido, menos necessidade de voltar ao painel.\n\n" +

                    "**ON:** RMB alterna em dois conjuntos separados:\n" +
                    "Esquerda ↔ Direita\n" +
                    "Ambos ↔ Nenhum"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Alternar painel de botões do Easy Zoning" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome do mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome exibido deste mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versão" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versão atual do mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Abrir a página Paradox Mods do autor." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Entrar no Discord do mod." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
