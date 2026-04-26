// File: src/Localization/LocaleJA.cs
// Purpose: Japanese (ja-JP) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleJA : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleJA(Setting setting)
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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "アクション" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "レガシー" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "概要" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "ゾーン設定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "キー割り当て" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "互換性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "UI" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "使い方" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "既存のゾーン済みマスをリセットしない" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "プレビュー/適用時に、すでにゾーン指定されたセルをリセットしません。\n\n" +
                    "**[ ✓ ] 有効を推奨。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "建物が削除されるのを防ぐ" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建物 = 占有セル**。新しいゾーンのプレビュー/適用によって既存の建物が取り壊し扱いになるのを防ぎます。\n\n" +
                    "**[ ✓ ] 有効を推奨。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "更新パネルの切り替え" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning パネルを表示します（**デフォルト Ctrl+V**）。" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 等高線ボタン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 有効**で、既存道路用 Easy Zoning パネルに等高線ボタンを表示します。\n\n" +
                    "他の MOD がすでに地形の等高線を処理している場合は無効化してください。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ ガラスパネルスタイル" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 有効**で、より見やすい半透明パネルを使用します。\n" +
                    "**[   ] 無効**で、より暗いバニラ風パネルを使用します。\n\n" +
                    "見た目だけの設定です。ぼかしは使用しません。" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "使い方を表示" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "以下の**使い方説明**を表示または非表示にします。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<新しい道路>\n" +
                    "1. 道路パネルを開く（道路を選ぶ）。\n" +
                    "2. 道路ツールパネル下部で、EZ の3つのボタンから「両側 / 左のみ / 右のみ」を使う。\n" +
                    "   選択中のボタンをもう一度押すと「なし」。\n" +
                    "3. いつも通りに道路を引く。\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = 右クリック、LMB = 左クリック\n" +
                    "-----------------------------------------\n\n" +
                    "<既存の道路>\n" +
                    "1. EZ 更新パネルを開く：<Ctrl+V> を押してパネルを ON/OFF\n" +
                    "   （または <左上アイコン> でも同じ）。\n" +
                    "2. EZ の3つのボタンから「両側 / 左のみ / 右のみ」を使う。\n" +
                    "   選択中のボタンをもう一度押すと「なし」。\n" +
                    "3. 道路にカーソルを合わせてプレビューする。\n" +
                    "4. <RMB で切り替え>：両側 → 左のみ → 右のみ → なし → ...\n" +
                    "5. <LMB を1回>：適用（確定）。\n" +
                    "6. <LMB を押したままドラッグ>して複数の道路区間に適用し、離して確定。\n" +
                    "7. <キャンセル：> マウスを外して **LMB** を離す。\n\n" +
                    "-------------------------------------------\n" +
                    "<オプションボタン>\n" +
                    "• <Contour> は地形の等高線を表示します。" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "レガシー右クリック切り替え" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF 推奨**。RMB で4つすべてのモードを切り替えます：\n" +
                    "**両側 → 左のみ → 右のみ → なし → ...**\n\n" +
                    "利点：ツールパネルまでマウスを戻す回数が減ります。\n\n" +
                    "--------------------------------------\n" +
                    "レガシーが ON の場合：RMB は2つの別グループだけを切り替えます：\n" +
                    "左のみ ↔ 右のみ\n" +
                    "両側 ↔ なし" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 更新パネルの切り替え" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "MOD 名" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "この MOD の表示名です。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "現在の MOD バージョンです。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "作者の Paradox Mods ページを開きます。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "MOD の Discord に参加します。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
