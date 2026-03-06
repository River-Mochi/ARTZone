// File: src/Localization/LocaleZH_HANT.cs
// Purpose: Traditional Chinese (zh-HANT) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocaleZH_HANT : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleZH_HANT(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "關於" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分區選項" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按鍵綁定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "舊版工具行為" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "不要重置已分區的格子" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "在預覽/套用時，不重置已經分區的格子。\n\n" +
                    "**[ ✓ ] 建議啟用。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止建築被移除" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建築 = 已佔用格子**。防止新分區的預覽/套用把現有建築變成「待拆除」。\n\n" +
                    "**[ ✓ ] 建議啟用。**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "切換更新面板" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "顯示 Easy Zoning 面板（**預設 Shift+V**）。"
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "舊版 RMB 循環" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**推薦 OFF。**\n" +
                    "當 OFF 時，RMB（右鍵）可循環全部 4 種模式：\n" +
                    "雙側 → 左側 → 右側 → 無 → ...\n\n" +
                    "優點：更快，減少回到面板切換圖示的次數。\n\n" +

                    "**ON：** RMB 在兩個集合裡切換：\n" +
                    "左側 ↔ 右側\n" +
                    "雙側 ↔ 無"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "切換 Easy Zoning 按鈕面板" },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "更改分區：雙側、左<->右，或無。\n" +
                    "左鍵確認選擇。按住左鍵並沿道路拖曳，可更新多個路段。" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模組名稱" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "此模組的顯示名稱。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "目前模組版本。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "開啟作者的 Paradox Mods 頁面。" },
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
