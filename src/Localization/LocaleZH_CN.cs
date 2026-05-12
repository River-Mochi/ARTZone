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
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab), "旧版" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab), "关于" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup), "保护" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按键绑定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup), "兼容性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup), "视觉" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup), "使用说明" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● 防止移除建筑" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建筑 = 已占用单元格**。防止预览/应用新区划时让现有建筑变成待拆除状态。\n" +
                    "\n" +
                    "**[ ✓ ] 建议启用。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● 防止重置已涂色/已分区的方格" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "预览/应用时不会重置已经分区的单元格。\n" +
                    "\n" +
                    "**[ ✓ ] 建议启用。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "更新面板开/关" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "用于快速显示 Easy Zoning 面板的**按键绑定**\n" +
                    "**默认 Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 等高线按钮" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 启用**，在模组的现有道路更新面板中显示地形等高线按钮。\n" +
                    "\n" +
                    "● 如果想要更小的面板，或其他模组已处理地形线，请禁用此项。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ 玻璃面板" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 启用**，面板使用清晰的半透明样式。\n" +
                    "**[   ] 禁用**，使用灰色面板。\n" +
                    "\n" +
                    "仅改变视觉样式。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "移除单元格边框" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "将被移除的单元格预览边框颜色。\n" +
                    "\n" +
                    "<橙色> = 更亮，更容易看清。\n" +
                    "<原版红色> = 匹配游戏默认外观。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "边框不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "调整移除预览边框的不透明度。\n" +
                    "\n" +
                    "<100%> 保持预览的正常半透明效果。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "移除单元格填充" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "将被移除的单元格预览覆盖层填充样式。\n" +
                    "\n" +
                    "<原版红色> = 当前游戏外观。\n" +
                    "<白色> = 更清爽的对比度。\n" +
                    "<橙色> = 匹配橙色边框。\n" +
                    "<无> = 仅显示边框。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "填充不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "调整可移除单元格预览的填充不透明度。\n" +
                    "\n" +
                    "<100%> 保持预览的正常半透明效果。\n" +
                    "如果 <移除填充> 设置为 <无>，则忽略此项。" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "显示说明" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)), "显示或隐藏下面的**使用说明**。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<新建道路>\n" +
                    "1. 打开道路面板（选择一条道路）。\n" +
                    "2. 在道路工具面板底部，使用 3 个 EZ 图标选择双侧 / 左侧 / 右侧。\n" +
                    "   再次点击已选按钮可切换为无。\n" +
                    "3. 像平常一样绘制。\n" +
                    "\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = 右键，<LMB> = 左键\n" +
                    "-----------------------------------------\n" +
                    "\n" +
                    "<现有道路>\n" +
                    "1. 打开 EZ Update 面板：点击 <Ctrl+V> 开启/关闭面板\n" +
                    "   （<左上角图标>也有相同功能）。\n" +
                    "2. 使用 3 个 EZ 图标选择双侧 / 左侧 / 右侧。\n" +
                    "   再次点击按钮可切换为无。\n" +
                    "3. 悬停并预览道路。\n" +
                    "4. 红色预览 = 将被移除的单元格。\n" +
                    "5. <RMB 循环>：双侧 → 左侧 → 右侧 → 无 → ...\n" +
                    "6. <LMB 一次>：应用（锁定）。\n" +
                    "7. <按住 LMB + 拖动> 沿多个道路路段移动，松开后应用。\n" +
                    "8. <取消：> 移开鼠标并松开 **LMB**。\n" +
                    "\n" +
                    "-------------------------------------------\n" +
                    "<可选按钮>\n" +
                    "• <等高线> 显示地形高度线。" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "旧版右键循环" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**建议 OFF**\n" +
                    "Off 表示 RMB 会循环全部 4 个模式：**双侧 → 左侧 → 右侧 → 无 → ...**\n" +
                    "\n" +
                    "禁用优势：较少需要把鼠标移回工具面板。\n" +
                    "\n" +
                    "--------------------------------------\n" +
                    "如果旧版为 ON：RMB 会在两个独立组合中切换：\n" +
                    "仅左侧 ↔ 右侧\n" +
                    "仅双侧 ↔ 无" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "切换 Easy Zoning 更新面板" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)), "模组名称" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)), "此模组的显示名称。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)), "当前模组版本。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)), "打开作者的 Paradox Mods 页面。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)), "加入模组 Discord。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
