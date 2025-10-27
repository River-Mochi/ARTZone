// File: src/Settings/LocalePT_BR.cs
// Portuguese (pt-BR) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocalePT_BR : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocalePT_BR(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "ART — Zoneamento" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Ações" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Sobre" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opções de Zona" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Teclas de atalho" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Impedir que células zoneadas sejam removidas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Não substituir células já zoneadas durante a pré-visualização/aplicação.\n <Recomendado ativar.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Impedir que células ocupadas sejam removidas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Não substituir células já ocupadas durante a pré-visualização/aplicação (ex.: edifícios).\n <Recomendado ativar>" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Mostrar painel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Mostrar o botão do painel ART-Zone (padrão Shift+Z)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "Alternar painel do botão de zona" },

                // Palette (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Alterar Zoneamento" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Altere o zoneamento nas vias: ambos os lados, esquerdo, direito ou nenhum. Botão direito alterna a escolha. Botão esquerdo confirma." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nome do mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nome que aparece para este mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versão" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versão atual do mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Abrir a página do mod no Paradox Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Entrar no Discord do mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
