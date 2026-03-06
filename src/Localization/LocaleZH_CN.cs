// File: src/Localization/LocaleZH_CN.cs
// Purpose: Simplified Chinese (zh-HANS) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

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
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "关于" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "分区选项" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "按键绑定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "旧版工具行为" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "不要重置已分区的格子" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "在预览/应用时，不重置已经分区的格子。\n\n" +
                    "**[ ✓ ] 建议启用。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "防止建筑被移除" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建筑 = 已占用格子**。防止新分区的预览/应用把现有建筑变成“待拆除”。\n\n" +
                    "**[ ✓ ] 建议启用。**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "切换更新面板" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "显示 Easy Zoning 面板（**默认 Shift+V**）。"
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "旧版 RMB 循环" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**推荐 OFF。**\n" +
                    "当 OFF 时，RMB（右键）可循环全部 4 种模式：\n" +
                    "双侧 → 左侧 → 右侧 → 无 → ...\n\n" +
                    "优点：更快，减少回到面板切换图标的次数。\n\n" +

                    "**ON：** RMB 在两个集合里切换：\n" +
                    "左侧 ↔ 右侧\n" +
                    "双侧 ↔ 无"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "切换 Easy Zoning 按钮面板" },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "更改分区：双侧、左<->右，或无。\n" +
                    "左键确认选择。按住左键并沿道路拖动，可更新多个路段。" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "模组名称" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "此模组的显示名称。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "当前模组版本。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "打开作者的 Paradox Mods 页面。" },
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
