// File: src/Localization/LocaleES.cs
// Purpose: Spanish (es-ES) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocaleES : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleES(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opciones de zonificación" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atajos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "No restablecer los cuadros ya zonificados" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "No restablecer las celdas ya zonificadas durante la vista previa/aplicación.\n\n" +
                "**[ ✓ ] Recomendado activado.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar que se eliminen edificios" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edificios = celdas ocupadas**. Evita que la vista previa/aplicación de nuevas zonas convierta edificios existentes en condenados.\n\n" +
                "**[ ✓ ] Recomendado activado.**" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Alternar panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Mostrar el botón del panel de Easy Zoning (Ctrl+Z predeterminado)." },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Alternar panel del botón de Easy Zoning" },

                // Legacy Panel (Road Services tile)
                //{ $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                //{ $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                //  "Choose zoning for roads: both, left, right, or none.\nRight-click flips; left-click applies." },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "Cambiar la zonificación: ambos lados, izquierda<->derecha o ninguno.\n" +
                    "Clic izquierdo aplica. Arrastrar a lo largo de una carretera para actualizar varios segmentos." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nombre del mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nombre para mostrar de este mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versión" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versión actual del mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Abrir la página de Paradox Mods del autor." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Unirse al Discord del mod." },
            };
            return d;
        }

        public void Unload( )
        {
        }
    }
}
