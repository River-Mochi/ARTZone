// File: src/Localization/LocaleES.cs
// Purpose: Spanish (es-ES) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleES : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleES(Setting setting)
        {
            m_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Clásico" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "Opciones de zonificación" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Atajos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilidad" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Interfaz" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USO" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "No restablecer casillas ya zonificadas" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "No restablece las celdas ya zonificadas durante la vista previa/aplicación.\n\n" +
                    "**[ ✓ ] Activado recomendado.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Evitar que se eliminen edificios" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Los edificios = celdas ocupadas**. Evita que la vista previa/aplicación de nuevas zonas convierta edificios existentes en edificios condenados.\n\n" +
                    "**[ ✓ ] Activado recomendado.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Mostrar/Ocultar panel de actualización" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Mostrar el panel Easy Zoning (**Ctrl+V por defecto**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Botón de contorno" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] activado**, muestra el botón Contorno en el panel Easy Zoning para carreteras existentes.\n\n" +
                    "Desactívalo si otro mod ya maneja las líneas de contorno del terreno." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Estilo de panel translúcido" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] activado**, usa un panel translúcido más claro.\n" +
                    "**[   ] desactivado**, usa un panel más oscuro de estilo vanilla.\n\n" +
                    "Solo estilo visual. No se usa desenfoque." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Mostrar instrucciones" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Mostrar u ocultar las **instrucciones de uso** de abajo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Carretera nueva>\n" +
                    "1. Abrir el panel de carreteras (elige una carretera).\n" +
                    "2. En la parte inferior del panel de la herramienta de carretera: elegir uno de los 3 iconos de zona.\n" +
                    "3. Dibujar como siempre.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = clic derecho, LMB = clic izquierdo\n" +
                    "-----------------------------------------\n\n" +
                    "<Carretera existente>\n" +
                    "1. Abrir el panel EZ de actualización: pulsar <Ctrl+V> para mostrar/ocultar el panel\n" +
                    "   (o <el icono superior izquierdo> hace lo mismo).\n" +
                    "2. Seleccionar un icono de zona en el panel inferior.\n" +
                    "3. Pasar el cursor + previsualizar una carretera.\n" +
                    "4. <RMB recorre>: Ambos lados → Izquierda → Derecha → Ninguno → ...\n" +
                    "5. <LMB una vez>: aplica (lo fija).\n" +
                    "6. <Mantener LMB + arrastrar> por varios segmentos de carretera y soltar para aplicar.\n" +
                    "7. <Cancelar:> alejar el ratón y soltar **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<BOTÓN OPCIONAL>\n" +
                    "• <Contorno> muestra líneas de elevación del terreno." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo clásico con clic derecho" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Recomendado OFF** para que RMB recorra los 4 modos:\n" +
                    "**Ambos lados → Izquierda → Derecha → Ninguno → ...**\n\n" +
                    "Ventaja: menos necesidad de mover el ratón de vuelta al panel de herramientas.\n\n" +
                    "--------------------------------------\n" +
                    "Si el modo clásico está ON: RMB alterna entre dos grupos separados:\n" +
                    "Izquierda ↔ Derecha\n" +
                    "Ambos lados ↔ Ninguno" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Mostrar/Ocultar panel Easy Zoning de actualización" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nombre del mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nombre visible de este mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versión" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versión actual del mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Abrir la página Paradox Mods del autor." },
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
