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
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "このModについて" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "保護" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "キー割り当て" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "互換性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "表示" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "使い方" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● 建物の削除を防止" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建物 = 占有セル**。プレビュー/適用で建物が取り壊し対象になるのを防ぎます。\n\n" +
                    "**[ ✓ ] 有効推奨。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● すでに塗った/ゾーン済みのマスのリセットを防止" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "プレビュー/適用中に、すでにゾーン設定済みのセルをリセットしません。\n\n" +
                    "**[ ✓ ] 有効推奨。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "更新パネル On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning パネルを素早く表示するための**キー割り当て**\n" +
                    "**デフォルト Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 等高線ボタン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 有効**、Modの既存道路更新パネルに Contour 地形ボタンを表示します。\n\n" +
                    "● 小さめのパネルが好みの場合、または別Modが地形ラインを扱う場合は無効にしてください。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ ガラスパネル" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 有効**、パネルに透明感のあるスタイルを使います。\n" +
                    "**[   ] 無効**、グレーのパネルを使います。\n\n" +
                    "見た目だけの設定です。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "境界線の色：削除プレビュー" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "削除されるセルのプレビュー境界線の色。\n\n" +
                    "<オレンジ> = 明るく見やすい。\n" +
                    "<赤> = 赤い境界線のコントラストを強める。\n" +
                    "<バニラ赤> = ゲーム標準の見た目に合わせる。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "境界線の不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "削除プレビューの境界線の不透明度を調整します。\n\n" +
                    "<100%> はプレビュー通常の半透明を保ちます。\n" +
                    "<0%> は境界線を非表示にします。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "塗りつぶし色：削除プレビュー" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "削除可能なセルのプレビュー塗りつぶし色スタイル。\n\n" +
                    "<バニラ赤> = 現在のゲーム表示。\n" +
                    "<白> = すっきりしたコントラスト。\n" +
                    "<オレンジ> = オレンジの境界線に合わせる。\n" +
                    "<なし> = 境界線のみ、ミニマル" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "塗りつぶしの不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "削除可能セルのプレビュー塗りつぶし不透明度を調整します。\n\n" +
                    "<100%> はプレビュー通常の半透明を保ちます。\n" +
                    "<0%> は塗りつぶしを非表示にします。\n" +
                    "<削除塗りつぶし> が <なし> の場合は無視されます。" },

                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "オレンジ" },
                { "EasyZoning.Dropdown.Color.Red", "赤" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "バニラ赤" },
                { "EasyZoning.Dropdown.Color.White", "白" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "なし（枠線のみ）" },
                
                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "説明を表示" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "下の**使い方説明**を表示/非表示にします。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<新しい道路>\n" +
                    "1. 道路パネルを開きます（道路を選択）。\n" +
                    "2. 道路ツールパネル下部で、3つのEZアイコンを使い 両側 / 左 / 右 を選びます。\n" +
                    "   選択中のボタンをもう一度クリックすると なし になります。\n" +
                    "3. いつも通り描画します。\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = 右クリック、<LMB> = 左クリック\n" +
                    "-----------------------------------------\n\n" +
                    "<既存道路>\n" +
                    "1. EZ Update パネルを開く：<Ctrl+V> をクリックしてパネルを On/Off\n" +
                    "   （<左上アイコン> でも同じです）。\n" +
                    "2. 3つのEZアイコンで 両側 / 左 / 右 を選びます。\n" +
                    "   ボタンをもう一度クリックすると なし になります。\n" +
                    "3. 道路にカーソルを合わせてプレビューします。\n" +
                    "4. 赤いプレビュー = 削除されるセル。\n" +
                    "5. <RMBで切替>：両側 → 左 → 右 → なし → ...\n" +
                    "6. <LMB 1回>：適用します（確定）。\n" +
                    "7. <LMB長押し + ドラッグ> で複数の道路区間をなぞり、離すと適用。\n" +
                    "8. <キャンセル:> マウスを離れた場所へ動かして **LMB** を離します。\n\n" +
                    "-------------------------------------------\n" +
                    "<任意ボタン>\n" +
                    "• <等高線> は地形の高さラインを表示します。" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "レガシー右クリック切替" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF 推奨**\n" +
                    "OFFではRMBが4つのモードを順番に切り替えます：**両側 → 左 → 右 → なし → ...**\n\n" +
                    "無効時の利点：マウスをツールパネルに戻す手間が減ります。\n\n" +
                    "--------------------------------------\n" +
                    "レガシーがONの場合：RMBは2つの別グループ内で切り替えます：\n" +
                    "左 ↔ 右 のみ\n" +
                    "両側 ↔ なし のみ"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 更新パネルを切り替え" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod名" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "このModの表示名。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "現在のModバージョン。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "作者の Paradox Mods ページを開きます。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Modの Discord に参加します。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
