// File: src/Localization/LocaleZH_HANT.cs
// Purpose: Traditional Chinese (zh-Hant) strings for Options UI + Panel text.

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
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "不重設現有分區格" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "在預覽/套用時不重設已分區的格子。\n\n" +
                "**[ ✓ ] 建議開啟。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止移除建築" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建築 = 已佔用格**。避免在預覽/套用新分區時，把現有建築變成「待拆除」。\n\n" +
                "**[ ✓ ] 建議開啟。**" },

                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "面板開關" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "顯示 Easy Zoning 面板按鈕（預設 Ctrl+Z）。" },

                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "切換 Easy Zoning 按鈕面板" },

                // Legacy Panel (Road Services tile)
                //{ $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Easy Zoning" },
                //{ $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                //  "Choose zoning for roads: both, left, right, or none.\nRight-click flips; left-click applies." },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "更改分區：兩側、左<->右或無。\n" +
                    "左鍵套用。沿著道路拖曳可更新多個路段。" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模組名稱" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "此模組的顯示名稱。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "目前的模組版本。" },

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
