// File: src/Settings/LocaleZH_CN.cs
// Simplified Chinese (zh-HANS) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly Setting s_Settings;
        public LocaleZH_CN(Setting setting) => s_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { s_Settings.GetSettingsLocaleID(), "ART — 分区" },

                // Tabs
                { s_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { s_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "关于" },

                // Groups
                { s_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分区控制器选项" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按键绑定" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { s_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "防止已分区网格被移除" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "在预览/应用时不覆盖已分区网格。" },

                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止已占用网格被移除" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "在预览/应用时不覆盖已占用网格。" },

                // Keybind (only one visible)
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "切换分区控制面板" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "显示/隐藏 ART 分区控制面板（默认 Shift+Z）。" },

                // Binding title in the keybinding dialog
                { s_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "切换分区控制面板" },

                // Palette (Road Services tile)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "分区控制器" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "更改分区：两侧、左侧、右侧或无。右键可反转选择。" },

                // About tab labels
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模组名称" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "此模组的显示名称。" },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "当前模组版本。" },
#if DEBUG
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "信息版本" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "版本 + 构建信息" },
#endif
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "打开 Paradox Mods 页面。" },
                { s_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { s_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "加入模组 Discord。" },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
