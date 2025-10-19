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
        private readonly Setting m_Setting;
        public LocaleES(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { m_Setting.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups (Actions tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opciones de la herramienta de zonificación" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Atajos de teclado" },

                // Groups (About tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "Enlaces" },

                // Toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Evitar eliminar celdas ya zonificadas" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "Evita reemplazar las celdas zonificadas existentes durante la vista previa o aplicación." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar eliminar celdas ocupadas" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "Evita reemplazar las celdas que tienen edificios durante la vista previa o aplicación." },

                // Keybind
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Botón del ratón para invertir zonificación" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "Asigna un botón del ratón para invertir la dirección de la zonificación." },
                { m_Setting.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Invertir zonificación" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Controlador de zonificación" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Controla cómo se comporta la zonificación junto a una carretera.\nElige entre ambos lados, solo izquierda, solo derecha o ninguno.\nClic derecho para invertir la configuración." },

                // About tab link buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Abrir la página de Paradox Mods en tu navegador." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Abrir el Discord de ART-Zone en tu navegador." },
            };
        }

        public void Unload()
        {
        }
    }
}
