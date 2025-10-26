// File: src/Settings/LocaleES.cs
// Spanish (es-ES) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleES : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleES(Setting setting) => s_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART — Zonas" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opciones de zonificación" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atajos de teclado" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "No eliminar celdas ya zonificadas" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "No sobrescribe celdas ya zonificadas durante la vista previa o la aplicación.\n <Se recomienda activarlo.>" },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "No eliminar celdas ocupadas" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "No sobrescribe celdas ya ocupadas durante la vista previa o la aplicación (por ejemplo, con edificios).\n <Se recomienda activarlo.>" },

                // Keybind
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Mostrar / ocultar panel" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "Muestra/oculta el panel de ART-Zone (por defecto Shift+Z)." },

                // Binding title
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "Mostrar / ocultar panel ART-Zone" },

                // Palette text
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Editor de zona" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "Cambia la zonificación de las carreteras: ambos lados, izquierda, derecha o ninguno. Clic derecho alterna la opción. Clic izquierdo confirma." },

                // About tab
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nombre del mod" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nombre mostrado de este mod." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versión" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versión actual del mod." },
#if DEBUG
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "Versión informativa" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "Versión + información de compilación" },
#endif
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Abrir la página en Paradox Mods." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Unirse al Discord del mod." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
