// File: src/Settings/LocaleJA.cs
// Japanese (ja-JP) strings for Options UI + palette text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

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
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "ART — 区画" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "情報" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "区画オプション" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "キー割り当て" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "既に区画指定されたセルを削除しない" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "プレビュー／適用中に、既に区画済みのセルを上書きしません。\n <有効化を推奨>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "占有中のセルを削除しない" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "プレビュー／適用中に、既に建物などで使用中のセルを上書きしません。\n <有効化を推奨>" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "パネルの表示 / 非表示" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "ART-Zone パネルを表示/非表示にします（デフォルト: Shift+Z）。" },

                // Binding title
                { m_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "ART-Zone パネルの表示 / 非表示" },

                // Palette text
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "ゾーン変更ツール" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "道路のゾーニングを変更します：両側、左側、右側、またはなし。右クリックで選択を切り替え、左クリックで確定します。" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "Mod 名" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "この Mod の表示名。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "現在の Mod バージョン。" },
#if DEBUG
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "詳細バージョン" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)),  "バージョン + ビルド情報" },
#endif
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Paradox Mods のページを開きます。" },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "Mod の Discord に参加します。" },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
