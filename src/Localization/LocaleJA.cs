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
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab), "レガシー" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab), "情報" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup), "保護" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "キー割り当て" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup), "互換性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup), "表示" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup), "使い方" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● 建物の削除を防止" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建物 = 使用中セル**。新しいゾーンのプレビュー/適用で、既存の建物が取り壊し扱いになるのを防ぎます。\n" +
                    "\n" +
                    "**[ ✓ ] 有効推奨。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● すでに塗った/ゾーン設定済みのマスをリセットしない" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "プレビュー/適用中に、すでにゾーン設定済みのセルをリセットしません。\n" +
                    "\n" +
                    "**[ ✓ ] 有効推奨。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "更新パネル On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning パネルをすばやく表示するための**キー割り当て**\n" +
                    "**初期設定 Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 等高線ボタン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 有効**、既存道路更新パネルに地形の Contour ボタンを表示します。\n" +
                    "\n" +
                    "● 小さいパネルにしたい場合や、別のMODが地形ラインを扱う場合は無効にしてください。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ ガラスパネル" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 有効**、パネルに明るい半透明スタイルを使います。\n" +
                    "**[   ] 無効**、グレーのパネルを使います。\n" +
                    "\n" +
                    "見た目だけの変更です。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "削除セルの枠線" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "削除予定セルのプレビュー枠線の色です。\n" +
                    "\n" +
                    "<オレンジ> = 明るく見やすい色。\n" +
                    "<バニラ赤> = ゲーム標準の見た目に合わせます。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "枠線の不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "削除プレビューの枠線の不透明度を調整します。\n" +
                    "\n" +
                    "<100%> はプレビュー通常の半透明を維持します。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "削除セルの塗りつぶし" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "削除予定セルのプレビューオーバーレイの塗りつぶしスタイルです。\n" +
                    "\n" +
                    "<バニラ赤> = 現在のゲームの見た目。\n" +
                    "<白> = すっきりしたコントラスト。\n" +
                    "<オレンジ> = オレンジの枠線に合わせます。\n" +
                    "<なし> = 枠線のみ。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "塗りつぶしの不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "削除可能セルのプレビュー塗りつぶし不透明度を調整します。\n" +
                    "\n" +
                    "<100%> はプレビュー通常の半透明を維持します。\n" +
                    "<削除塗りつぶし> が <なし> の場合は無視されます。" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "説明を表示" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)), "下の**使い方説明**を表示または非表示にします。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<新しい道路>\n" +
                    "1. 道路パネルを開く（道路を選ぶ）。\n" +
                    "2. 道路ツールパネル下部の3つのEZアイコンで 両側 / 左 / 右 を選ぶ。\n" +
                    "   選択中のボタンをもう一度クリックすると なし。\n" +
                    "3. いつも通り道路を引く。\n" +
                    "\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = 右クリック、<LMB> = 左クリック\n" +
                    "-----------------------------------------\n" +
                    "\n" +
                    "<既存道路>\n" +
                    "1. EZ Update パネルを開く：<Ctrl+V> でパネルを On/Off\n" +
                    "   （<左上アイコン>でも同じ）。\n" +
                    "2. 3つのEZアイコンで 両側 / 左 / 右 を選ぶ。\n" +
                    "   ボタンをもう一度クリックすると なし。\n" +
                    "3. 道路にマウスを合わせてプレビュー。\n" +
                    "4. 赤いプレビュー = 削除されるセル。\n" +
                    "5. <RMBで切替>：両側 → 左 → 右 → なし → ...\n" +
                    "6. <LMB 1回>：適用（固定）。\n" +
                    "7. <LMB長押し + ドラッグ> 複数の道路区間に沿って動かし、離すと適用。\n" +
                    "8. <キャンセル:> マウスを離れた場所へ動かして **LMB** を離す。\n" +
                    "\n" +
                    "-------------------------------------------\n" +
                    "<任意ボタン>\n" +
                    "• <等高線> は地形の高度線を表示します。" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "レガシー右クリック切替" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF 推奨**\n" +
                    "Off の場合、RMB は4モードを切り替えます：**両側 → 左 → 右 → なし → ...**\n" +
                    "\n" +
                    "無効時の利点：マウスをツールパネルへ戻す回数が減ります。\n" +
                    "\n" +
                    "--------------------------------------\n" +
                    "レガシーが ON の場合：RMB は2つの別グループで切り替えます：\n" +
                    "左 ↔ 右 のみ\n" +
                    "両側 ↔ なし のみ" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 更新パネルを切り替え" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)), "MOD名" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)), "このMODの表示名。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)), "現在のMODバージョン。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)), "作者の Paradox Mods ページを開きます。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)), "MODのDiscordに参加します。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
