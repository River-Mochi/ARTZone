// File: src/Localization/LocalePT_BR.cs
// Purpose: Portuguese (pt-BR) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocalePT_BR : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocalePT_BR(Setting setting)
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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Ações" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Legacy" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Sobre" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Opções de zoneamento" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Atalhos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilidade" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Interface" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USO" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Não redefinir quadrados já zoneados" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Não redefine células já zoneadas durante prévia/aplicação.\n\n" +
                    "**[ ✓ ] Recomendado ativado.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar que edifícios sejam removidos" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edifícios = células ocupadas**. Impede que a prévia/aplicação de novas zonas transforme edifícios existentes em condenados.\n\n" +
                    "**[ ✓ ] Recomendado ativado.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Alternar painel de atualização" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Mostra o painel Easy Zoning (**padrão Ctrl+V**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Botão de contorno" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] ativado**, mostra o botão de Contorno no painel Easy Zoning para estradas existentes.\n\n" +
                    "Desative isto se outro mod já controlar as linhas de contorno do terreno." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Estilo de painel translúcido" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] ativado**, usa um estilo de painel translúcido mais claro.\n" +
                    "**[   ] desativado**, usa um painel mais escuro no estilo vanilla.\n\n" +
                    "Apenas estilo visual. Nenhum blur é usado." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseOrangeRemovePreviewEdge)), "◉ Orange remove-preview edge" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseOrangeRemovePreviewEdge)),
                    "**[ ✓ ] enabled**, use a brighter orange border for cells that will be removed.\n" +
                    "**[   ] disabled**, keep the vanilla red border.\n\n" +
                    "Only changes the remove-preview border. Fill stays vanilla for now." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Remove-preview edge opacity" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Adjusts only the orange remove-preview border opacity.\n\n" +
                    "Does not change normal zoning colors or the white add-preview cells." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Mostrar instruções" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Mostra ou oculta as **instruções de uso** abaixo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nova estrada>\n" +
                    "1. Abra o painel de Estradas (escolha uma estrada).\n" +
                    "2. Na parte inferior do painel da ferramenta de estrada: use os 3 ícones EZ para Ambos os lados / Esquerda / Direita.\n" +
                    "   Clique novamente no botão selecionado para Nenhum.\n" +
                    "3. Desenhe normalmente.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = clique direito, LMB = clique esquerdo\n" +
                    "-----------------------------------------\n\n" +
                    "<Estrada existente>\n" +
                    "1. Abra o painel EZ Update: clique <Ctrl+V> para ligar/desligar o painel\n" +
                    "   (ou o <ícone no canto superior esquerdo> faz o mesmo).\n" +
                    "2. Use os 3 ícones EZ para Ambos os lados / Esquerda / Direita.\n" +
                    "   Clique novamente no botão selecionado para Nenhum.\n" +
                    "3. Passe o mouse sobre uma estrada + veja a prévia.\n" +
                    "4. Prévia vermelha = células que serão removidas.\n" +
                    "5. <RMB alterna>: Ambos os lados → Esquerda → Direita → Nenhum → ...\n" +
                    "6. <LMB uma vez>: aplica (confirma).\n" +
                    "7. <Segure LMB + arraste> por vários trechos de estrada, solte para aplicar.\n" +
                    "8. <Cancelar:> mova o mouse para fora e solte **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<BOTÃO OPCIONAL>\n" +
                    "• <Contour> mostra linhas de elevação do terreno." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo legado com clique direito" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Recomendado OFF** para que RMB percorra os 4 modos:\n" +
                    "**Ambos os lados → Esquerda → Direita → Nenhum → ...**\n\n" +
                    "Vantagem: menos necessidade de mover o mouse de volta ao painel da ferramenta.\n\n" +
                    "--------------------------------------\n" +
                    "Se Legacy estiver ON: RMB alterna entre dois conjuntos separados:\n" +
                    "Esquerda ↔ Direita\n" +
                    "Ambos os lados ↔ Nenhum" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Alternar painel de atualização Easy Zoning" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome do mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome exibido deste mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versão" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versão atual do mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Abrir a página do autor no Paradox Mods." },
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
