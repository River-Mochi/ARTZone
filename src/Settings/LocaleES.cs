// File: src/LocaleES.cs
/// <summary>
/// Spanish locale (es-ES)
/// </summary>
namespace ARTZone
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleES : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleES(Setting setting)
        {
            s_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups (Actions tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opciones de la herramienta de zonificación" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atajos de teclado" },

                // Groups (About tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "Enlaces" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Evitar eliminar celdas ya zonificadas" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Evita reemplazar las celdas zonificadas existentes durante la vista previa o aplicación." },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar eliminar celdas ocupadas" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Evita reemplazar las celdas que tienen edificios durante la vista previa o aplicación." },

                // Keybind
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Botón del ratón para invertir zonificación" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Asigna un botón del ratón para invertir la dirección de la zonificación." },
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Invertir zonificación" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Controlador de zonificación" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Controla cómo se comporta la zonificación junto a una carretera.\nElige entre ambos lados, solo izquierda, solo derecha o ninguno.\nClic derecho para invertir la configuración." },

                // About tab link buttons
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Abrir la página de Paradox Mods en tu navegador." },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Abrir el Discord de ART-Zone en tu navegador." },
            };
        }

        public void Unload()
        {
        }
    }
}
