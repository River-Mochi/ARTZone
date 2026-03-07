// File: src/Localization/LocaleDE.cs
// Purpose: German (de-DE) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocaleDE : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleDE(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Aktionen" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Info" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Zonen-Optionen" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Tastenbelegungen" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "Legacy-Werkzeugverhalten" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Bereits zonierte Felder nicht zurücksetzen" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Bereits zonierte Zellen werden während Vorschau/Anwenden nicht zurückgesetzt.\n\n" +
                    "**[ ✓ ] Aktiviert empfohlen.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Verhindern, dass Gebäude entfernt werden" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Gebäude = belegte Zellen**. Verhindert, dass Vorschau/Anwenden neuer Zonen bestehende Gebäude zu Abrisskandidaten macht.\n\n" +
                    "**[ ✓ ] Aktiviert empfohlen.**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Update-Panel umschalten" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy-Zoning-Panel anzeigen (**Standard Ctrl+V**)."
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Legacy-RMB-Zyklus" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF wird empfohlen.**\n" +
                    "Wenn OFF, kann RMB (Rechtsklick) alle 4 Modi durchlaufen:\n" +
                    "Beide → Links → Rechts → Keine → ...\n\n" +
                    "Vorteil: schneller, weniger Rückkehr zum Panel mit der Maus.\n\n" +

                    "**ON:** RMB toggelt in zwei getrennten Sets:\n" +
                    "Links ↔ Rechts\n" +
                    "Beide ↔ Keine"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy-Zoning-Button-Panel umschalten" },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "Zonierung ändern: beide Seiten, links<->rechts oder keine.\n" +
                    "Linksklick bestätigt die Auswahl. Linksklick halten + entlang einer Straße ziehen, um mehrere Segmente zu aktualisieren." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod-Name" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Anzeigename dieser Mod." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Aktuelle Mod-Version." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Paradox-Mods-Seite des Autors öffnen." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Dem Mod-Discord beitreten." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
