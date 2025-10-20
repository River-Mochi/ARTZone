// File: src/LocaleZH_CN.cs
/// <summary>
/// Simplified Chinese locale (zh-HANS)
/// </summary>
namespace ARTZone
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleZH_CN(Setting setting)
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
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "关于" },

                // Groups (Actions tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分区控制工具选项" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按键绑定" },

                // Groups (About tab)
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "链接" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "防止已分区格被移除" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "在预览或应用阶段时，不覆盖已有分区格。" },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止已建造格被移除" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "在预览或应用阶段时，不覆盖已有建筑格。" },

                // Keybind
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "反转分区鼠标按钮" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InvertZoning)),  "绑定一个鼠标按钮，用于反转当前分区方向。" },
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "反转分区" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "分区控制器" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "控制道路两侧的分区方式。\n可选择两侧、仅左侧、仅右侧或不分区。\n默认右键点击可反转当前设置。" },

                // About tab link buttons
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "在浏览器中打开 Paradox Mods 页面。" },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "在浏览器中打开 ART-Zone 的 Discord。" },
            };
        }

        public void Unload()
        {
        }
    }
}
