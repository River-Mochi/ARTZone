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
            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opciones de zonificación" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atajos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),     "Comportamiento heredado de la herramienta" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "No restablecer cuadrados ya zonificados" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "No restablece celdas ya zonificadas durante la vista previa/aplicación.\n\n" +
                    "**[ ✓ ] Se recomienda activarlo.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar que se eliminen edificios" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edificios = celdas ocupadas**. Evita que la vista previa/aplicación de nuevas zonas convierta edificios existentes en condenados.\n\n" +
                    "**[ ✓ ] Se recomienda activarlo.**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Alternar panel de actualización" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Muestra el panel de Easy Zoning (**Ctrl+V predeterminado**)."
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo RMB heredado" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Se recomienda desactivado.**\n" +
                    "Cuando está desactivado, RMB (clic derecho) puede recorrer los 4 modos:\n" +
                    "Ambos → Izquierda → Derecha → Ninguno → ...\n\n" +
                    "Ventaja: más rápido, menos necesidad de volver al panel con el ratón.\n\n" +

                    "**Activado:** RMB alterna en dos conjuntos separados:\n" +
                    "Izquierda ↔ Derecha\n" +
                    "Ambos ↔ Ninguno"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning – Alternar panel" },

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
