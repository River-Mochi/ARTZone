// <copyright file="LocaleZH_HANT.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

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
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "保護" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "按鍵綁定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "相容性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "視覺" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "使用說明" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● 防止移除建築" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建築 = 已占用格**。防止預覽/套用時把建築變成待拆除狀態。\n\n" +
                    "**[ ✓ ] 建議啟用。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● 防止重設已繪製/已分區的方格" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "預覽/套用時不會重設已經分區的格子。\n\n" +
                    "**[ ✓ ] 建議啟用。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "EZ 更新面板 On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "**按鍵綁定**，快速顯示 Easy Zoning 面板\n" +
                    "**預設 Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "等高線" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "顯示按鈕" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 啟用**，在既有道路更新面板中顯示等高線按鈕。\n\n" +
                    "● 如果想要較小面板，或其他模組已處理等高線，請關閉此項。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ 玻璃面板" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 啟用**，為面板使用清楚的半透明樣式。\n" +
                    "**[   ] 停用** = 灰色面板。\n\n" +
                    "<僅影響視覺樣式。>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "邊框顏色：移除預覽" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "將被移除格子的預覽邊框顏色。\n\n" +
                    "<橘色> = 更亮，也更容易看清。\n" +
                    "<紅色> = 更強的紅色邊框對比。\n" +
                    "<粉紅色> = 鮮明又有趣。\n" +
                    "<紫色> = 柔和但清楚可見。\n" +
                    "<原版紅色> = 符合遊戲預設外觀。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "邊框不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "調整移除預覽邊框的不透明度。\n\n" +
                    "<100%> 保持預覽的正常半透明效果。\n" +
                    "<0%> 隱藏邊框。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "填滿顏色：移除預覽" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "可移除格子預覽的填滿顏色樣式。\n\n" +
                    "<原版紅色> = 目前遊戲外觀。\n" +
                    "<白色> = 對比更清楚。\n" +
                    "<橘色> = 符合橘色邊框。\n" +
                    "<粉紅色> = 鮮明又有趣。\n" +
                    "<紫色> = 柔和但清楚可見。\n" +
                    "<無> = 只有邊框，極簡" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "填滿不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "調整可移除格子預覽的填滿不透明度。\n\n" +
                    "<100%> 保持預覽的正常半透明效果。\n" +
                    "<0%> 隱藏填滿。\n" +
                    "如果 <移除填滿> 設為 <無>，則會被忽略。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "高對比度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "預設內容：\n" +
                    "<玻璃面板 On>\n" +
                    "<橙色邊框>\n" +
                    "<100% 邊框不透明度>\n" +
                    "<無填滿。>" },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "遊戲顏色" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "使用紅色邊框和紅色填滿，以符合遊戲分區工具的預覽。" },
  
                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "橙色" },
                { "EasyZoning.Dropdown.Color.Red", "紅色" },
                { "EasyZoning.Dropdown.Color.Pink", "粉紅色" },
                { "EasyZoning.Dropdown.Color.Purple", "紫色" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "原版紅色" },
                { "EasyZoning.Dropdown.Color.White", "白色" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "無（僅邊框）" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "顯示說明" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "顯示或隱藏下方的**使用說明**。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<既有道路>\n" +
                    "1. 開啟 EZ Update 面板：點擊 <Ctrl+V> 開啟/關閉面板\n" +
                    "   （<左上角圖示> 也是相同功能）。\n" +
                    "2. 使用 3 個 EZ 圖示選擇 兩側 / 左側 / 右側。\n" +
                    "   再次點擊按鈕可切換為無。\n" +
                    "3. 將滑鼠移到道路上並預覽。\n" +
                    "4. 紅色預覽 = 將被移除的格子。\n" +
                    "5. <RMB 循環>：兩側 → 左側 → 右側 → 無 → ...\n" +
                    "6. <LMB 一次>：套用（鎖定設定）。\n" +
                    "7. <按住 LMB + 拖曳> 沿多個道路區段移動，放開後套用。\n" +
                    "8. <取消：> 將滑鼠移開並放開 **LMB**。\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = 右鍵，<LMB> = 左鍵\n" +
                    "-----------------------------------------\n\n" +
                    "<新道路>\n" +
                    "1. 開啟道路面板（選擇一條道路）。\n" +
                    "2. 在道路工具面板底部：使用 3 個 EZ 圖示選擇 兩側 / 左側 / 右側。\n" +
                    "   再次點擊已選按鈕可切換為無。\n" +
                    "3. 像平常一樣繪製。\n\n" +
                    "-------------------------------------------\n" +
                    "<地形按鈕>\n" +
                    "<◎ 等高線> 顯示地形高程線。"
                },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "舊版右鍵循環" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**不建議使用**\n" +
                    "OFF 表示使用新版方式：RMB 會循環全部 4 種模式：**兩側 → 左側 → 右側 → 無 → ...**\n\n" +
                    "優點：比較不需要把滑鼠移回工具面板。\n\n" +
                    "<-------------------------------------->\n" +
                    "如果舊版為 ON：RMB 會在兩個獨立群組中切換，且需要更多滑鼠移動：\n" +
                    "僅左側 ↔ 右側\n" +
                    "僅兩側 ↔ 無"
                },

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
