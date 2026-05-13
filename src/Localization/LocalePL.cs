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
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "Starszy tryb" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "O modzie" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "Ochrona" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "Skróty klawiszowe" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "Zgodność" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "Wygląd" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "UŻYCIE" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● Chroń budynki przed usunięciem" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**Budynki = zajęte komórki**. Zapobiega temu, aby podgląd/zastosowanie oznaczało budynki do rozbiórki.\n\n" +
                    "**[ ✓ ] Zalecane włączenie.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● Chroń już pomalowane/zonowane kwadraty przed resetem" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Nie resetuje już zonowanych komórek podczas podglądu/zastosowania.\n\n" +
                    "**[ ✓ ] Zalecane włączenie.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "Panel aktualizacji On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**Skrót klawiszowy** do szybkiego pokazania panelu Easy Zoning\n" +
                    "**domyślnie Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ Przycisk poziomic" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] włączone**, pokazuje przycisk terenu Contour w panelu aktualizacji istniejących dróg moda.\n\n" +
                    "● Wyłącz to, jeśli wolisz mniejszy panel albo inny mod obsługuje linie terenu." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ Szklany panel" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] włączone**, używa jasnego półprzezroczystego stylu panelu.\n" +
                    "**[   ] wyłączone**, używa szarego panelu.\n\n" +
                    "Tylko styl wizualny." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "Kolor obramowania: podgląd usuwania" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "Kolor obramowania dla podglądu komórek do usunięcia.\n\n" +
                    "<Pomarańczowy> = jaśniejszy i łatwiejszy do zobaczenia.\n" +
                    "<Czerwony> = mocniejszy kontrast czerwonego obramowania.\n" +
                    "<Czerwony vanilla> = zgodny z domyślnym wyglądem gry." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "Przezroczystość obramowania" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "Dostosowuje przezroczystość obramowania podglądu usuwania.\n\n" +
                    "<100%> zachowuje normalną półprzezroczystość podglądu.\n" +
                    "<0%> ukrywa obramowanie." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "Kolor wypełnienia: podgląd usuwania" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "Styl koloru wypełnienia dla podglądu komórek, które można usunąć.\n\n" +
                    "<Czerwony vanilla> = obecny wygląd gry.\n" +
                    "<Biały> = czystszy kontrast.\n" +
                    "<Pomarańczowy> = pasuje do pomarańczowego obramowania.\n" +
                    "<Brak> = tylko obramowanie, minimalistycznie" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "Przezroczystość wypełnienia" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "Dostosowuje przezroczystość wypełnienia podglądu usuwalnych komórek.\n\n" +
                    "<100%> zachowuje normalną półprzezroczystość podglądu.\n" +
                    "<0%> ukrywa wypełnienie.\n" +
                    "Ignorowane, jeśli <Wypełnienie usuwania> ustawiono na <Brak>." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "Pokaż instrukcje" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "Pokazuje lub ukrywa poniższe **instrukcje użycia**." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<Nowa droga>\n" +
                    "1. Otwórz panel Dróg (wybierz drogę).\n" +
                    "2. Na dole panelu narzędzia drogi użyj 3 ikon EZ dla Obie / Lewa / Prawa.\n" +
                    "   Kliknij ponownie wybrany przycisk, aby ustawić Brak.\n" +
                    "3. Rysuj jak zwykle.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = prawy klik, <LMB> = lewy klik\n" +
                    "-----------------------------------------\n\n" +
                    "<Istniejąca droga>\n" +
                    "1. Otwórz panel EZ Update: kliknij <Ctrl+V>, aby włączyć/wyłączyć panel\n" +
                    "   (<ikona w lewym górnym rogu> robi to samo).\n" +
                    "2. Użyj 3 ikon EZ dla Obie / Lewa / Prawa.\n" +
                    "   Kliknij ponownie przycisk, aby ustawić Brak.\n" +
                    "3. Najedź kursorem + podejrzyj drogę.\n" +
                    "4. Czerwony podgląd = komórki do usunięcia.\n" +
                    "5. <RMB przełącza>: Obie → Lewa → Prawa → Brak → ...\n" +
                    "6. <LMB raz>: stosuje (blokuje wybór).\n" +
                    "7. <Przytrzymaj LMB + przeciągnij> wzdłuż wielu odcinków drogi, puść, aby zastosować.\n" +
                    "8. <Anuluj:> odsuń mysz i puść **LMB**.\n\n" +
                    "-------------------------------------------\n" +
                    "<PRZYCISK OPCJONALNY>\n" +
                    "• <Poziomice> pokazuje linie wysokości terenu." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "Starszy cykl prawym kliknięciem" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**Zalecane OFF**\n" +
                    "OFF oznacza, że RMB przełącza wszystkie 4 tryby: **Obie → Lewa → Prawa → Brak → ...**\n\n" +
                    "Zaleta wyłączenia: mniej potrzeby wracania myszą do panelu narzędzia.\n\n" +
                    "--------------------------------------\n" +
                    "Jeśli Starszy tryb jest ON: RMB przełącza w dwóch osobnych grupach:\n" +
                    "Tylko Lewa ↔ Prawa\n" +
                    "Tylko Obie ↔ Brak"
                },

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
