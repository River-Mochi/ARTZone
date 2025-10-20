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
        private readonly Setting s_Settings;
        public LocaleZH_HANT(Setting setting)
        {
            s_Settings = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART-Zone" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "關於" },

                // Groups (Actions tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分區控制工具選項" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按鍵綁定" },

                // Groups (About tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "連結" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "防止已分區格被移除" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "在預覽或應用階段時，不覆蓋已有的分區格。" },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止已建造格被移除" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "在預覽或應用階段時，不覆蓋已有建築格。" },

                // Keybind
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "反轉分區滑鼠按鈕" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "綁定一個滑鼠按鈕，用來反轉分區方向。" },
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "反轉分區" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "分區控制器" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "控制道路兩側的分區方式。\n可選擇兩側、僅左側、僅右側或無分區。\n預設右鍵可反轉當前設定。" },

                // About tab link buttons
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "在瀏覽器中開啟 Paradox Mods 網頁。" },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "在瀏覽器中開啟 ART-Zone 的 Discord。" },
            };
        }

        public void Unload()
        {
        }
    }
}
