// File: src/Localization/LocaleJA.cs
// Purpose: Japanese (ja-JP) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocaleJA : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleJA(Setting setting) => m_Settings = setting;

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
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "情報" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "ゾーン設定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "キー割り当て" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "旧ツール動作" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "既にゾーン指定されたマスをリセットしない" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "プレビュー/適用中に、既にゾーン指定されたセルをリセットしません。\n\n" +
                    "**[ ✓ ] 有効推奨。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "建物が削除されないようにする" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建物 = 占有セル**。新しいゾーンのプレビュー/適用で、既存の建物が立ち退き（取り壊し候補）にならないようにします。\n\n" +
                    "**[ ✓ ] 有効推奨。**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "更新パネルの切り替え" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning パネルを表示（**既定 Ctrl+Z**）。"
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "レガシー RMB サイクル" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF 推奨。**\n" +
                    "OFF の場合、RMB（右クリック）で4モードを循環できます：\n" +
                    "両側 → 左 → 右 → なし → ...\n\n" +
                    "利点：速く、パネルへ戻る回数が減ります。\n\n" +

                    "**ON:** RMB は2つのセットで切り替えます：\n" +
                    "左 ↔ 右\n" +
                    "両側 ↔ なし"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning ボタンパネルの切り替え" },

                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                    "ゾーンを変更：両側、左<->右、またはなし。\n" +
                    "左クリックで確定。左クリック長押し + 道路に沿ってドラッグで複数セグメントを更新します。" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod 名" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "このModの表示名。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "現在のModバージョン。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "作者の Paradox Mods ページを開きます。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Mod の Discord に参加します。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
