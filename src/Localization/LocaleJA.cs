// <copyright file="LocaleJA.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "レガシー" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "情報" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "保護" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "キー設定" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "互換性" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "表示" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "使い方" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● 建物の削除を防ぐ" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**建物 = 占有セル**。プレビュー/適用で建物が廃墟化しないようにします。\n\n" +
                    "**[ ✓ ] ON 推奨。**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● 既に塗った/ゾーン済みマスを守る" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "プレビュー/適用中に、既にゾーン済みのセルをリセットしません。\n\n" +
                    "**[ ✓ ] ON 推奨。**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "EZ更新パネル ON/OFF" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoningパネルをすぐ表示する**キー設定**\n" +
                    "**初期設定 Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "等高線" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "ボタンを表示" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] ON**で、既存道路更新パネルに等高線ボタンを表示します。\n\n" +
                    "● パネルを小さくしたい場合や、別MODで等高線を使う場合はOFFにしてください。" },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ ガラス風パネル" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] ON**で、見やすい半透明スタイルを使います。\n" +
                    "**[   ] OFF** = グレーパネル。\n\n" +
                    "<見た目だけの設定です。>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "枠線の色：削除プレビュー" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "削除されるセルのプレビュー枠線の色です。\n\n" +
                    "<オレンジ> = 明るくて見やすい。\n" +
                    "<赤> = 赤い枠線のコントラストを強める。\n" +
                    "<ピンク> = 明るくて楽しい色。\n" +
                    "<紫> = やわらかいけど見やすい色。\n" +
                    "<バニラ赤> = ゲーム標準の見た目に合わせる。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "枠線の不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "削除プレビュー枠線の不透明度を調整します。\n\n" +
                    "<100%> プレビュー本来の半透明を維持。\n" +
                    "<0%> 枠線を非表示。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "塗りつぶし色：削除プレビュー" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "削除可能セルのプレビュー塗りつぶし色です。\n\n" +
                    "<バニラ赤> = 現在のゲーム標準の見た目。\n" +
                    "<白> = すっきりしたコントラスト。\n" +
                    "<オレンジ> = オレンジ枠線に合わせる。\n" +
                    "<ピンク> = 明るくて楽しい色。\n" +
                    "<紫> = やわらかいけど見やすい色。\n" +
                    "<なし> = 枠線のみ、ミニマル" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "塗りつぶしの不透明度" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "削除可能セルのプレビュー塗りつぶし不透明度を調整します。\n\n" +
                    "<100%> プレビュー本来の半透明を維持。\n" +
                    "<0%> 塗りつぶしを非表示。\n" +
                    "<削除塗りつぶし> が <なし> の場合は無視されます。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "高コントラスト" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "プリセット内容:\n" +
                    "<ガラスパネル On>\n" +
                    "<オレンジ枠線>\n" +
                    "<枠線不透明度 100%>\n" +
                    "<塗りつぶしなし。>" },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "ゲームカラー" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "ゲームのゾーニングツールのプレビューに合わせて、赤い枠線と塗りつぶしを使います。" },
  
                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "オレンジ" },
                { "EasyZoning.Dropdown.Color.Red", "赤" },
                { "EasyZoning.Dropdown.Color.Pink", "ピンク" },
                { "EasyZoning.Dropdown.Color.Purple", "紫" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "バニラ赤" },
                { "EasyZoning.Dropdown.Color.White", "白" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "なし（枠線のみ）" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "使い方を表示" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "下の**使い方**を表示/非表示にします。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<既存道路>\n" +
                    "1. EZ更新パネルを開く: <Ctrl+V> でパネルON/OFF\n" +
                    "   （<左上アイコン> でも同じです）。\n" +
                    "2. 3つのEZアイコンで 両側 / 左 / 右 を選択。\n" +
                    "   同じボタンをもう一度クリックで なし。\n" +
                    "3. 道路にホバーしてプレビュー。\n" +
                    "4. 赤いプレビュー = 削除されるセル。\n" +
                    "5. <RMBで切替>: 両側 → 左 → 右 → なし → ...\n" +
                    "6. <LMB 1回>: 適用（確定）。\n" +
                    "7. <LMB長押し + ドラッグ>で複数道路を選び、離すと適用。\n" +
                    "8. <キャンセル:> マウスを外して **LMB** を離す。\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = 右クリック、<LMB> = 左クリック\n" +
                    "-----------------------------------------\n\n" +
                    "<新しい道路>\n" +
                    "1. 道路パネルを開く（道路を選ぶ）。\n" +
                    "2. 道路ツールパネル下部の3つのEZアイコンで 両側 / 左 / 右 を選択。\n" +
                    "   同じボタンをもう一度クリックで なし。\n" +
                    "3. 普通に道路を引く。\n\n" +
                    "-------------------------------------------\n" +
                    "<地形ボタン>\n" +
                    "<◎ 等高線> で地形の高さ線を表示します。"
                },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "旧式の右クリック切替" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**非推奨**\n" +
                    "OFFでは新方式: RMBで4モードを順に切替: **両側 → 左 → 右 → なし → ...**\n\n" +
                    "利点: マウスをツールパネルへ戻す回数が減ります。\n\n" +
                    "<-------------------------------------->\n" +
                    "レガシーONの場合: RMBは2つの組み合わせだけを切替。マウス移動が増えます:\n" +
                    "左 ↔ 右 のみ\n" +
                    "両側 ↔ なし のみ"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning更新パネル切替" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "MOD名" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "このMODの表示名。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "現在のMODバージョン。" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "作者のParadox Modsページを開きます。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "MODのDiscordに参加します。" },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
