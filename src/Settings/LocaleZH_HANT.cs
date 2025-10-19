// File: src/LocaleZH_HANT.cs
/// <summary>
/// Traditional Chinese locale (zh-HANT)
/// </summary>
namespace ARTZone
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleZH_HANT : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleZH_HANT(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { m_Setting.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "關於" },

                // Groups (Actions tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分區控制工具選項" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按鍵綁定" },

                // Groups (About tab)
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "連結" },

                // Toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "防止已分區格被移除" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "在預覽或應用階段時，不覆蓋已有的分區格。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止已建造格被移除" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "在預覽或應用階段時，不覆蓋已有建築格。" },

                // Keybind
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "反轉分區滑鼠按鈕" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "綁定一個滑鼠按鈕，用來反轉分區方向。" },
                { m_Setting.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "反轉分區" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "分區控制器" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "控制道路兩側的分區方式。\n可選擇兩側、僅左側、僅右側或無分區。\n預設右鍵可反轉當前設定。" },

                // About tab link buttons
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "在瀏覽器中開啟 Paradox Mods 網頁。" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "在瀏覽器中開啟 ART-Zone 的 Discord。" },
            };
        }

        public void Unload()
        {
        }
    }
}
