// File: src/Settings/LocaleES.cs
// Spanish (es-ES) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

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
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "ART — Zonas" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opciones de zonificación" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atajos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "No eliminar celdas ya zonificadas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "No sobrescribe celdas ya zonificadas durante la vista previa o la aplicación.\n <Se recomienda activarlo.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "No eliminar celdas ocupadas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "No sobrescribe celdas ya ocupadas durante la vista previa o la aplicación (por ejemplo, con edificios).\n <Se recomienda activarlo.>" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Mostrar / ocultar panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Muestra/oculta el panel de ART-Zone (por defecto Shift+Z)." },

                // Binding title
                { m_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "Mostrar / ocultar panel ART-Zone" },

                // Palette text
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Editor de zona" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Cambia la zonificación de las carreteras: ambos lados, izquierda, derecha o ninguno. Clic derecho alterna la opción. Clic izquierdo confirma." },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nombre del mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nombre mostrado de este mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versión" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versión actual del mod." },
#if DEBUG
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "Versión informativa" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "Versión + información de compilación" },
#endif
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Abrir la página en Paradox Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Unirse al Discord del mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
