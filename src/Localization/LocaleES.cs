// <copyright file="LocaleES.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Acciones" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Clásico" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Acerca de" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Protecciones" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Atajos de teclado" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Compatibilidad" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Visuales" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "USO" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Evitar eliminación de edificios" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Edificios = celdas ocupadas**. Evita que la vista previa/aplicación deje edificios condenados.\n\n" +
                    "**[ ✓ ] Recomendado activado.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Evitar reinicio de cuadrados ya pintados/zonificados" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "No reinicia celdas ya zonificadas durante la vista previa/aplicación.\n\n" +
                    "**[ ✓ ] Recomendado activado.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Panel EZ On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Atajo de teclado** para mostrar rápido el panel Easy Zoning\n" +
                    "**predeterminado Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "Líneas de contorno" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "Mostrar botón" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] activado**, muestra el botón de líneas de contorno en el panel de actualización de carreteras existentes.\n\n" +
                    "● Desactívalo si prefieres un panel más pequeño o si otro mod gestiona las líneas de contorno." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Panel de cristal" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] activado**, usa un estilo translúcido más claro para el panel.\n" +
                    "**[   ] desactivado** = panel gris.\n\n" +
                    "<Solo estilo visual.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Color de borde: eliminaciones en vista previa" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Color del borde para la vista previa de celdas que se eliminarán.\n\n" +
                    "<Naranja> = más brillante y fácil de ver.\n" +
                    "<Rojo> = contraste de borde rojo más fuerte.\n" +
                    "<Rosa> = color vivo y divertido.\n" +
                    "<Morado> = contraste suave pero visible.\n" +
                    "<Rojo vanilla> = coincide con el aspecto predeterminado del juego." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Opacidad del borde" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Ajusta la opacidad del borde de la vista previa de eliminación.\n\n" +
                    "<100%> mantiene la translucidez normal de la vista previa.\n" +
                    "<0%> oculta el borde." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Color de relleno: eliminaciones en vista previa" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Estilo de color de relleno para la vista previa de celdas que pueden eliminarse.\n\n" +
                    "<Rojo vanilla> = aspecto actual del juego.\n" +
                    "<Blanco> = contraste más limpio.\n" +
                    "<Naranja> = combina con el borde naranja.\n" +
                    "<Rosa> = color vivo y divertido.\n" +
                    "<Morado> = contraste suave pero visible.\n" +
                    "<Ninguno> = solo borde, minimalista" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Opacidad del relleno" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Ajusta la opacidad del relleno para la vista previa de celdas eliminables.\n\n" +
                    "<100%> mantiene la translucidez normal de la vista previa.\n" +
                    "<0%> oculta el relleno.\n" +
                    "Se ignora si <Relleno de eliminación> está en <Ninguno>." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "Alto contraste" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "Preajuste para:\n" +
                    "<Panel de cristal On>\n" +
                    "<Borde naranja>\n" +
                    "<100% opacidad del borde>\n" +
                    "<Sin relleno.>" },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "Color del juego" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "Usa borde y relleno rojos para coincidir con la vista previa de zonificación del juego." },
  
                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "Naranja" },
                { "EasyZoning.Dropdown.Color.Red", "Rojo" },
                { "EasyZoning.Dropdown.Color.Pink", "Rosa" },
                { "EasyZoning.Dropdown.Color.Purple", "Morado" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "Rojo vanilla" },
                { "EasyZoning.Dropdown.Color.White", "Blanco" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "Ninguno (solo borde)" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Mostrar instrucciones" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Muestra u oculta las **instrucciones de uso** de abajo." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Carreteras existentes>\n" +
                    "1. Abre el panel EZ Update: haz clic en <Ctrl+V> para activar/desactivar el panel\n" +
                    "   (<icono superior izquierdo> hace lo mismo).\n" +
                    "2. Usa los 3 iconos EZ para Ambos / Izquierda / Derecha.\n" +
                    "   Haz clic otra vez en el botón para Ninguno.\n" +
                    "3. Pasa el cursor y previsualiza una carretera.\n" +
                    "4. Vista previa roja = celdas que se eliminarán.\n" +
                    "5. <RMB alterna>: Ambos → Izquierda → Derecha → Ninguno → ...\n" +
                    "6. <LMB una vez>: aplica (lo fija).\n" +
                    "7. <Mantener LMB + arrastrar> por varias secciones de carretera, soltar para aplicar.\n" +
                    "8. <Cancelar:> aleja el mouse y suelta **LMB**.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = clic derecho, <LMB> = clic izquierdo\n" +
                    "-----------------------------------------\n\n" +
                    "<Carretera nueva>\n" +
                    "1. Abre el panel Carreteras (elige una carretera).\n" +
                    "2. Abajo del panel de herramienta de carretera: usa los 3 iconos EZ para Ambos / Izquierda / Derecha.\n" +
                    "   Haz clic de nuevo en el botón seleccionado para Ninguno.\n" +
                    "3. Dibuja como siempre.\n\n" +
                    "-------------------------------------------\n" +
                    "<Botón de terreno>\n" +
                    "<◎ Líneas de contorno> muestra líneas de elevación del terreno."
                },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Ciclo clásico con clic derecho" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**No recomendado**\n" +
                    "OFF significa usar el método moderno: RMB recorre los 4 modos: **Ambos → Izquierda → Derecha → Ninguno → ...**\n\n" +
                    "Ventaja: menos necesidad de mover el mouse de vuelta al panel de herramienta.\n\n" +
                    "<-------------------------------------->\n" +
                    "Si Clásico está ON: RMB alterna en dos grupos separados y requiere más movimientos del mouse:\n" +
                    "Izquierda ↔ Derecha solamente\n" +
                    "Ambos ↔ Ninguno solamente"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Alternar panel de actualización de Easy Zoning" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nombre del mod" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Nombre mostrado de este mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versión" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versión actual del mod." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Abre la página de Paradox Mods del autor." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Únete al Discord del mod." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
