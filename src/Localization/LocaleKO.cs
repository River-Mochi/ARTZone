// File: src/Localization/LocaleKO.cs
// Purpose: Korean (ko-KR) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using EasyZoning.Tools;
    using System.Collections.Generic;

    public sealed class LocaleKO : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleKO(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            Dictionary<string, string> d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Settings.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "작업" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "정보" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "존 옵션" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "키 바인딩" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup),   "레거시 도구 동작" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "이미 존 지정된 칸을 리셋하지 않기" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "미리보기/적용 중에 이미 존 지정된 셀을 리셋하지 않습니다.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "건물이 제거되지 않도록 방지" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**건물 = 점유 셀**. 새 존의 미리보기/적용이 기존 건물을 철거(퇴거) 상태로 만들지 못하게 합니다.\n\n" +
                    "**[ ✓ ] Enabled recommended.**" },


                // Keybind (only one visible)
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "업데이트 패널 토글" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning 패널 표시 (**기본 Ctrl+V**)."
                },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "레거시 RMB 사이클" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF 권장.**\n" +
                    "OFF일 때, RMB(오른쪽 클릭)로 4가지 모드를 모두 순환합니다:\n" +
                    "양쪽 → 왼쪽 → 오른쪽 → 없음 → ...\n\n" +
                    "장점: 더 빠르고, 패널로 돌아갈 필요가 줄어듭니다.\n\n" +

                    "**ON:** RMB가 두 세트로만 토글됩니다:\n" +
                    "왼쪽 ↔ 오른쪽\n" +
                    "양쪽 ↔ 없음"
                },


                // Binding title in the keybinding dialog
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 버튼 패널 토글" },

                // About tab labels
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "모드 이름" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "이 모드의 표시 이름." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "버전" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "현재 모드 버전." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),     "제작자의 Paradox Mods 페이지를 엽니다." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "모드 Discord에 참여합니다." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
