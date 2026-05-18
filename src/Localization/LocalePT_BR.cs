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
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Clássico" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Sobre" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Proteções" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Atalhos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilidade" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Visuais" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USO" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Impedir remoção de edifícios" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edifícios = células ocupadas**. Impede que prévia/aplicação transforme edifícios em condenados.\n\n" +
                    "**[ ✓ ] Recomendado ativado.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Impedir reset de quadrados já pintados/zoneados" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Não reseta células já zoneadas durante prévia/aplicação.\n\n" +
                    "**[ ✓ ] Recomendado ativado.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Painel de atualização On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Atalho de teclado** para mostrar rapidamente o painel Easy Zoning\n" +
                    "**padrão Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "Linhas de contorno" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "Mostrar botão" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] ativado**, mostra o botão de terreno Contour no painel de atualização de estradas existentes do mod.\n\n" +
                    "● Desative isto se preferir um painel menor ou se outro mod já controlar as linhas do terreno." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Painel de vidro" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] ativado**, usa um estilo translúcido claro para o painel.\n" +
                    "**[   ] desativado**, usa um painel cinza.\n\n" +
                    "Apenas estilo visual." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Cor da borda: remoções na prévia" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Cor da borda para a prévia das células que serão removidas.\n\n" +
                    "<Laranja> = mais brilhante e fácil de ver.\n" +
                    "<Vermelho> = contraste de borda vermelha mais forte.\n" +
                    "<Vermelho vanilla> = combina com o visual padrão do jogo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Opacidade da borda" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Ajusta a opacidade da borda da prévia de remoção.\n\n" +
                    "<100%> mantém a translucidez normal da prévia.\n" +
                    "<0%> oculta a borda." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Cor do preenchimento: remoções na prévia" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Estilo de cor do preenchimento para a prévia das células que podem ser removidas.\n\n" +
                    "<Vermelho vanilla> = visual atual do jogo.\n" +
                    "<Branco> = contraste mais limpo.\n" +
                    "<Laranja> = combina com a borda laranja.\n" +
                    "<Nenhum> = só borda, minimalista" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Opacidade do preenchimento" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Ajusta a opacidade do preenchimento para a prévia das células removíveis.\n\n" +
                    "<100%> mantém a translucidez normal da prévia.\n" +
                    "<0%> oculta o preenchimento.\n" +
                    "Ignorado se <Preenchimento de remoção> estiver definido como <Nenhum>." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "Alto contraste" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "Ativa painel de vidro, borda laranja, 100% de opacidade da borda e sem preenchimento." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "Cor do jogo" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "Usa borda e preenchimento vermelhos como a prévia da ferramenta de zoneamento do jogo." },

                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "Laranja" },
                { "EasyZoning.Dropdown.Color.Red", "Vermelho" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "Vermelho vanilla" },
                { "EasyZoning.Dropdown.Color.White", "Branco" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "Nenhum (só borda)" },
             
                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Mostrar instruções" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Mostra ou oculta as **instruções de uso** abaixo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nova estrada>\n" +
                    "1. Abra o painel Estradas (escolha uma estrada).\n" +
                    "2. Na parte inferior do painel da ferramenta de estrada: use os 3 ícones EZ para Ambos / Esquerda / Direita.\n" +
                    "   Clique novamente no botão selecionado para Nenhum.\n" +
                    "3. Desenhe como sempre.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = clique direito, <LMB> = clique esquerdo\n" +
                    "-----------------------------------------\n\n" +
                    "<Estrada existente>\n" +
                    "1. Abra o painel EZ Update: clique em <Ctrl+V> para ligar/desligar o painel\n" +
                    "   (<ícone no canto superior esquerdo> faz a mesma coisa).\n" +
                    "2. Use os 3 ícones EZ para Ambos / Esquerda / Direita.\n" +
                    "   Clique no botão novamente para Nenhum.\n" +
                    "3. Passe o mouse + veja a prévia de uma estrada.\n" +
                    "4. Prévia vermelha = células que serão removidas.\n" +
                    "5. <RMB alterna>: Ambos → Esquerda → Direita → Nenhum → ...\n" +
                    "6. <LMB uma vez>: aplica (fixa a escolha).\n" +
                    "7. <Segure LMB + arraste> por várias seções de estrada, solte para aplicar.\n" +
                    "8. <Cancelar:> mova o mouse para longe e solte **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<BOTÃO OPCIONAL>\n" +
                    "<◎ Contorno> mostra linhas de elevação do terreno." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo clássico com clique direito" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF é recomendado**\n" +
                    "OFF significa que RMB alterna todos os 4 modos: **Ambos → Esquerda → Direita → Nenhum → ...**\n\n" +
                    "Vantagem desativada: menos necessidade de mover o mouse de volta ao painel da ferramenta.\n\n" +
                    "--------------------------------------\n" +
                    "Se Clássico estiver ON: RMB alterna em dois grupos separados:\n" +
                    "Somente Esquerda ↔ Direita\n" +
                    "Somente Ambos ↔ Nenhum"
                },

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
