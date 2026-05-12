// File: src/Localization/LocaleZH_HANT.cs
// Purpose: Traditional Chinese (zh-HANT) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleZH_HANT : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleZH_HANT(Setting setting)
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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "舊版" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "關於" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "分區選項" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "按鍵綁定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "相容性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "介面" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "使用說明" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "不要重設現有分區格" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "在預覽/套用時，不會重設已經劃分分區的儲格。\n\n" +
                    "**[ ✓ ] 建議開啟。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止建築被移除" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建築 = 已佔用儲格**。防止預覽/套用新分區時讓現有建築變成待拆除狀態。\n\n" +
                    "**[ ✓ ] 建議開啟。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "切換更新面板" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "顯示 Easy Zoning 面板（**預設 Ctrl+V**）。" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 等高線按鈕" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 已啟用**，在 Easy Zoning 的現有道路面板中顯示等高線按鈕。\n\n" +
                    "如果其他模組已經處理地形等高線，可停用此選項。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ 玻璃面板樣式" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 已啟用**，使用更清晰的半透明面板樣式。\n" +
                    "**[   ] 已停用**，使用較深色的原版風格面板。\n\n" +
                    "僅影響視覺樣式。不使用模糊效果。" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "顯示說明" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "顯示或隱藏下方的**使用說明**。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<新建道路>\n" +
                    "1. 開啟道路面板（選擇一條道路）。\n" +
                    "2. 在道路工具面板底部，使用 3 個 EZ 圖示選擇雙側 / 左側 / 右側。\n" +
                    "   再次點擊目前選取的按鈕即可切換為無。\n" +
                    "3. 像平常一樣繪製道路。\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = 右鍵，LMB = 左鍵\n" +
                    "-----------------------------------------\n\n" +
                    "<現有道路>\n" +
                    "1. 開啟 EZ Update 面板：按 <Ctrl+V> 開啟/關閉面板\n" +
                    "   （或使用<左上角圖示>也可以）。\n" +
                    "2. 使用 3 個 EZ 圖示選擇雙側 / 左側 / 右側。\n" +
                    "   再次點擊目前選取的按鈕即可切換為無。\n" +
                    "3. 將滑鼠移到道路上進行預覽。\n" +
                    "4. 紅色預覽 = 即將移除的格子。\n" +
                    "5. <RMB 循環切換>：雙側 → 左側 → 右側 → 無 → ...\n" +
                    "6. <單擊 LMB>：套用（鎖定設定）。\n" +
                    "7. <按住 LMB 並拖曳>經過多個道路段，放開後套用。\n" +
                    "8. <取消：> 將滑鼠移開並放開 **LMB**。\n\n" +
                    "-------------------------------------------\n" +
                    "<可選按鈕>\n" +
                    "• <Contour> 顯示地形等高線。" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "舊版右鍵循環" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**建議關閉**，這樣 RMB 會循環全部 4 種模式：\n" +
                    "**雙側 → 左側 → 右側 → 無 → ...**\n\n" +
                    "優點：不需要頻繁把滑鼠移回工具面板。\n\n" +
                    "--------------------------------------\n" +
                    "如果舊版模式為 ON：RMB 只會在兩組之間切換：\n" +
                    "左側 ↔ 右側\n" +
                    "雙側 ↔ 無" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "切換 Easy Zoning 更新面板" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模組名稱" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "此模組的顯示名稱。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "目前模組版本。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "開啟作者的 Paradox Mods 頁面。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "加入模組 Discord。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
