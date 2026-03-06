// File: src/Localization/LocalePL.cs
// Purpose: Polish (pl-PL) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocalePL : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocalePL(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Akcje" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "O modzie" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "Opcje strefowania" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Skróty klawiszowe" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "Zachowanie narzędzia (legacy)" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Nie resetuj już oznaczonych pól stref" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Nie resetuje już oznaczonych komórek stref podczas podglądu/zastosowania.\n\n" +
                    "**[ ✓ ] Włączone zalecane.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Zapobiegaj usuwaniu budynków" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Budynki = zajęte komórki**. Zapobiega temu, aby podgląd/zastosowanie nowych stref zmieniało istniejące budynki na przeznaczone do rozbiórki.\n\n" +
                    "**[ ✓ ] Włączone zalecane.**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Przełącz panel aktualizacji" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Pokaż panel Easy Zoning (**domyślnie Shift+V**)."
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Cykl RMB (legacy)" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Zalecane OFF.**\n" +
                    "Gdy OFF, RMB (prawy klik) może przełączać wszystkie 4 tryby:\n" +
                    "Obie → Lewa → Prawa → Brak → ...\n\n" +
                    "Zaleta: szybciej, rzadziej trzeba wracać do panelu.\n\n" +

                    "**ON:** RMB przełącza w dwóch osobnych zestawach:\n" +
                    "Lewa ↔ Prawa\n" +
                    "Obie ↔ Brak"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Przełącz panel przycisków Easy Zoning" },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "Zmień strefowanie: obie strony, lewa<->prawa albo brak.\n" +
                    "Lewy klik zatwierdza wybór. Przytrzymaj lewy klik + przeciągnij wzdłuż drogi, aby zaktualizować wiele segmentów." },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nazwa moda" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Wyświetlana nazwa tego moda." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Wersja" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Aktualna wersja moda." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "Otwórz stronę Paradox Mods autora." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Dołącz do Discorda moda." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
