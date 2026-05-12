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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "Akcje" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab), "Klasyczne" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab), "O modzie" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup), "Zabezpieczenia" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Skróty klawiszowe" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup), "Zgodność" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup), "Wygląd" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup), "INSTRUKCJA" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Zapobiegaj usuwaniu budynków" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Budynki = zajęte komórki**. Zapobiega temu, aby podgląd/zastosowanie nowych stref oznaczało istniejące budynki jako do wyburzenia.\n" +
                    "\n" +
                    "**[ ✓ ] Zalecane włączone.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Zapobiegaj resetowaniu już pomalowanych/wyznaczonych pól" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Nie resetuje już wyznaczonych komórek podczas podglądu/zastosowania.\n" +
                    "\n" +
                    "**[ ✓ ] Zalecane włączone.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Panel aktualizacji On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Skrót klawiszowy** do szybkiego pokazania panelu Easy Zoning\n" +
                    "**domyślnie Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Przycisk poziomic" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] włączone**, pokazuje przycisk terenu Contour w panelu aktualizacji istniejących dróg moda.\n" +
                    "\n" +
                    "● Wyłącz, jeśli wolisz mniejszy panel albo inny mod obsługuje linie terenu." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Szklany panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] włączone**, używa jasnego, półprzezroczystego stylu panelu.\n" +
                    "**[   ] wyłączone**, używa szarego panelu.\n" +
                    "\n" +
                    "Tylko styl wizualny." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Obramowanie usuwanych komórek" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Kolor obramowania w podglądzie komórek do usunięcia.\n" +
                    "\n" +
                    "<Pomarańczowy> = jaśniejszy i łatwiejszy do zauważenia.\n" +
                    "<Vanilla czerwony> = zgodny z domyślnym wyglądem gry." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Przezroczystość obramowania" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Dostosowuje przezroczystość obramowania podglądu usuwania.\n" +
                    "\n" +
                    "<100%> zachowuje normalną półprzezroczystość podglądu." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Wypełnienie usuwanych komórek" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Styl wypełnienia nakładki podglądu dla komórek do usunięcia.\n" +
                    "\n" +
                    "<Vanilla czerwony> = obecny wygląd gry.\n" +
                    "<Biały> = czystszy kontrast.\n" +
                    "<Pomarańczowy> = pasuje do pomarańczowego obramowania.\n" +
                    "<Brak> = samo obramowanie." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Przezroczystość wypełnienia" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Dostosowuje przezroczystość wypełnienia podglądu komórek możliwych do usunięcia.\n" +
                    "\n" +
                    "<100%> zachowuje normalną półprzezroczystość podglądu.\n" +
                    "Ignorowane, jeśli <Wypełnienie usuwania> ustawiono na <Brak>." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Pokaż instrukcje" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)), "Pokaż lub ukryj poniższe **instrukcje użycia**." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nowa droga>\n" +
                    "1. Otwórz panel Dróg (wybierz drogę).\n" +
                    "2. Na dole panelu narzędzia drogi użyj 3 ikon EZ dla Obie / Lewa / Prawa.\n" +
                    "   Kliknij wybrany przycisk ponownie, aby ustawić Brak.\n" +
                    "3. Rysuj jak zwykle.\n" +
                    "\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = prawy przycisk myszy, <LMB> = lewy przycisk myszy\n" +
                    "-----------------------------------------\n" +
                    "\n" +
                    "<Istniejąca droga>\n" +
                    "1. Otwórz panel EZ Update: kliknij <Ctrl+V>, aby włączyć/wyłączyć panel\n" +
                    "   (<ikona w lewym górnym rogu> robi to samo).\n" +
                    "2. Użyj 3 ikon EZ dla Obie / Lewa / Prawa.\n" +
                    "   Kliknij przycisk ponownie, aby ustawić Brak.\n" +
                    "3. Najedź na drogę i sprawdź podgląd.\n" +
                    "4. Czerwony podgląd = komórki do usunięcia.\n" +
                    "5. <RMB przełącza>: Obie → Lewa → Prawa → Brak → ...\n" +
                    "6. <LMB jeden raz>: zastosuj (blokuje wybór).\n" +
                    "7. <Przytrzymaj LMB + przeciągnij> wzdłuż wielu odcinków drogi, puść, aby zastosować.\n" +
                    "8. <Anuluj:> odsuń mysz i puść **LMB**.\n" +
                    "\n" +
                    "-------------------------------------------\n" +
                    "<PRZYCISK OPCJONALNY>\n" +
                    "• <Poziomice> pokazuje linie wysokości terenu." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Klasyczny cykl prawego kliknięcia" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF jest zalecane**\n" +
                    "Off oznacza, że RMB przełącza wszystkie 4 tryby: **Obie → Lewa → Prawa → Brak → ...**\n" +
                    "\n" +
                    "Zaleta wyłączenia: mniej potrzeby przesuwania myszy z powrotem do panelu narzędzi.\n" +
                    "\n" +
                    "--------------------------------------\n" +
                    "Jeśli Klasyczne jest ON: RMB przełącza dwa oddzielne zestawy:\n" +
                    "Tylko Lewa ↔ Prawa\n" +
                    "Tylko Obie ↔ Brak" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Przełącz panel aktualizacji Easy Zoning" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)), "Nazwa moda" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)), "Wyświetlana nazwa tego moda." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Wersja" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)), "Aktualna wersja moda." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)), "Otwórz stronę autora w Paradox Mods." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)), "Dołącz do Discorda moda." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
