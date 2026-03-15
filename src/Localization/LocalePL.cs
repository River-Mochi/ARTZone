// File: src/Localization/LocalePL.cs
// Purpose: Polish (pl-PL) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocalePL : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocalePL(Setting setting)
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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Akcje" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Legacy" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "Informacje" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "Opcje stref" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Skróty klawiszowe" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Zgodność" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Interfejs" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "INSTRUKCJA" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Nie resetuj istniejących wyznaczonych pól stref" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Nie resetuje komórek już oznaczonych strefą podczas podglądu/stosowania.\n\n" +
                    "**[ ✓ ] Zalecane włączenie.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Zapobiegaj usuwaniu budynków" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Budynki = zajęte komórki**. Zapobiega temu, aby podgląd/stosowanie nowych stref zmieniało istniejące budynki w budynki przeznaczone do wyburzenia.\n\n" +
                    "**[ ✓ ] Zalecane włączenie.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Przełącz panel aktualizacji" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Pokazuje panel Easy Zoning (**domyślnie Ctrl+V**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Przycisk warstwic" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] włączone**, pokazuje przycisk Warstwice w panelu Easy Zoning dla istniejących dróg.\n\n" +
                    "Wyłącz to, jeśli inny mod już obsługuje linie warstwic terenu." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Styl szklanego panelu" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] włączone**, używa jaśniejszego półprzezroczystego stylu panelu.\n" +
                    "**[   ] wyłączone**, używa ciemniejszego panelu w stylu vanilla.\n\n" +
                    "Tylko styl wizualny. Nie używa rozmycia." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Pokaż instrukcje" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Pokazuje lub ukrywa **instrukcje użycia** poniżej." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nowa droga>\n" +
                    "1. Otwórz panel Dróg (wybierz drogę).\n" +
                    "2. Na dole panelu narzędzia dróg wybierz jedną z 3 ikon stref.\n" +
                    "3. Rysuj jak zwykle.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = prawy przycisk myszy, LMB = lewy przycisk myszy\n" +
                    "-----------------------------------------\n\n" +
                    "<Istniejąca droga>\n" +
                    "1. Otwórz panel EZ Update: kliknij <Ctrl+V>, aby włączyć/wyłączyć panel\n" +
                    "   (lub <ikona w lewym górnym rogu> robi to samo).\n" +
                    "2. Wybierz ikonę strefy z dolnego panelu.\n" +
                    "3. Najedź na drogę + zobacz podgląd.\n" +
                    "4. <RMB przełącza>: Obie strony → Lewa → Prawa → Brak → ...\n" +
                    "5. <Jedno kliknięcie LMB>: zastosowuje (zatwierdza).\n" +
                    "6. <Przytrzymaj LMB + przeciągnij> po wielu odcinkach drogi, puść, aby zastosować.\n" +
                    "7. <Anuluj:> odsuń mysz i puść **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<PRZYCISK OPCJONALNY>\n" +
                    "• <Contour> pokazuje linie wysokości terenu." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Legacy cykl prawym przyciskiem" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Zalecane OFF**, aby RMB przełączał wszystkie 4 tryby:\n" +
                    "**Obie strony → Lewa → Prawa → Brak → ...**\n\n" +
                    "Zaleta: mniejsza potrzeba przesuwania myszy z powrotem do panelu narzędzia.\n\n" +
                    "--------------------------------------\n" +
                    "Gdy Legacy jest ON: RMB przełącza dwie osobne pary:\n" +
                    "Lewa ↔ Prawa\n" +
                    "Obie strony ↔ Brak" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Przełącz panel aktualizacji Easy Zoning" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Nazwa moda" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "Wyświetlana nazwa tego moda." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Wersja" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Aktualna wersja moda." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Otwórz stronę autora w Paradox Mods." },
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
