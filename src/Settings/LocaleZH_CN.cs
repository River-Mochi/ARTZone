// File: src/Settings/LocaleZH_CN.cs
// Simplified Chinese (zh-HANS) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleZH_CN(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "ART — 分区" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "关于" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分区选项" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "快捷键" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "不要删除已分区的网格" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "在预览/应用时不要覆盖已经分区的网格。\n <建议启用>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "不要删除已占用的网格" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "在预览/应用时不要覆盖已经被建筑等占用的网格。\n <建议启用>" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "切换面板 显示/隐藏" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "显示或隐藏 ART-Zone 面板按钮（默认 Shift+Z）。" },

                // Binding title
                { m_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "切换 ART-Zone 面板" },

                // Palette text
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "分区更改工具" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "修改道路的分区：双侧、左侧、右侧或无。右键切换选择，左键确认。" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模组名称" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "该模组在游戏中显示的名称。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "当前模组版本。" },
#if DEBUG
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "详细版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "版本号 + 构建信息" },
#endif
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "打开 Paradox Mods 页面。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "加入模组的 Discord 服务器。" },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
