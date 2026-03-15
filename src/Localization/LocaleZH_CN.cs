// File: src/Localization/LocaleZH_CN.cs
// Purpose: Simplified Chinese (zh-HANS) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleZH_CN(Setting setting)
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
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "旧版" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "关于" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "分区选项" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "按键绑定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "兼容性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "界面" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "使用说明" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "不要重置已有分区格" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "在预览/应用时，不会重置已经划分分区的单元格。\n\n" +
                    "**[ ✓ ] 建议开启。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止建筑被移除" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建筑 = 已占用单元格**。防止预览/应用新区划时让现有建筑变成待拆除状态。\n\n" +
                    "**[ ✓ ] 建议开启。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "切换更新面板" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "显示 Easy Zoning 面板（**默认 Ctrl+V**）。" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 等高线按钮" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 已启用**，在 Easy Zoning 的现有道路面板中显示等高线按钮。\n\n" +
                    "如果其他模组已经处理地形等高线，可关闭此选项。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ 玻璃面板样式" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 已启用**，使用更清晰的半透明面板样式。\n" +
                    "**[   ] 已禁用**，使用更深色的原版风格面板。\n\n" +
                    "仅影响视觉样式。不使用模糊效果。" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "显示说明" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "显示或隐藏下面的**使用说明**。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<新建道路>\n" +
                    "1. 打开道路面板（选择一条道路）。\n" +
                    "2. 在道路工具面板底部选择 3 个分区图标之一。\n" +
                    "3. 像平常一样绘制道路。\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = 右键，LMB = 左键\n" +
                    "-----------------------------------------\n\n" +
                    "<现有道路>\n" +
                    "1. 打开 EZ Update 面板：点击 <Ctrl+V> 开启/关闭面板\n" +
                    "   （或使用<左上角图标>也可以）。\n" +
                    "2. 在底部面板中选择一个分区图标。\n" +
                    "3. 悬停到道路上进行预览。\n" +
                    "4. <RMB 循环切换>：双侧 → 左侧 → 右侧 → 无 → ...\n" +
                    "5. <单击 LMB>：应用（锁定设置）。\n" +
                    "6. <按住 LMB 并拖动>经过多个道路段，松开后应用。\n" +
                    "7. <取消：> 将鼠标移开并松开 **LMB**。\n\n" +
                    "-------------------------------------------\n" +
                    "<可选按钮>\n" +
                    "• <Contour> 显示地形等高线。" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "旧版右键循环" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**建议关闭**，这样 RMB 会循环全部 4 种模式：\n" +
                    "**双侧 → 左侧 → 右侧 → 无 → ...**\n\n" +
                    "优点：不需要频繁把鼠标移回工具面板。\n\n" +
                    "--------------------------------------\n" +
                    "如果旧版模式为 ON：RMB 只会在两组之间切换：\n" +
                    "左侧 ↔ 右侧\n" +
                    "双侧 ↔ 无" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "切换 Easy Zoning 更新面板" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模组名称" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "此模组的显示名称。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "当前模组版本。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "打开作者的 Paradox Mods 页面。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "加入模组 Discord。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
