// File: src/Localization/LocaleKO.cs
// Purpose: Korean (ko-KR) strings for Options UI + Panel text.

namespace EasyZoning
{
    using Colossal;
    using System.Collections.Generic;

    public sealed class LocaleKO : IDictionarySource
    {
        private readonly Setting m_Settings;

        public LocaleKO(Setting setting)
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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "동작" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab),  "레거시" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "정보" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),         "구역 옵션" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "키 바인딩" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "호환성" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "UI" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "사용법" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Zone options
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "이미 지정된 구역 칸을 초기화하지 않음" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "미리보기/적용 중 이미 구역 지정된 셀을 초기화하지 않습니다.\n\n" +
                    "**[ ✓ ] 활성화 권장.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "건물이 제거되지 않도록 방지" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**건물 = 점유된 셀**. 새 구역 미리보기/적용으로 기존 건물이 철거 예정 상태가 되는 것을 막습니다.\n\n" +
                    "**[ ✓ ] 활성화 권장.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "업데이트 패널 토글" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning 패널을 표시합니다 (**기본값 Ctrl+V**)." },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 등고선 버튼" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 활성화**, 기존 도로용 Easy Zoning 패널에 등고선 버튼을 표시합니다.\n\n" +
                    "다른 모드가 이미 지형 등고선을 처리한다면 비활성화하십시오." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ 유리 패널 스타일" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 활성화**, 더 선명한 반투명 패널 스타일을 사용합니다.\n" +
                    "**[   ] 비활성화**, 더 어두운 바닐라 스타일 패널을 사용합니다.\n\n" +
                    "시각적 스타일만 바뀝니다. 블러는 사용하지 않습니다." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "설명 표시" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "아래의 **사용 설명**을 표시하거나 숨깁니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<새 도로>\n" +
                    "1. 도로 패널을 엽니다(도로 선택).\n" +
                    "2. 도로 도구 패널 아래쪽에서 3개의 구역 아이콘 중 하나를 선택합니다.\n" +
                    "3. 평소처럼 도로를 그립니다.\n\n" +
                    "-----------------------------------------\n" +
                    "  RMB = 오른쪽 클릭, LMB = 왼쪽 클릭\n" +
                    "-----------------------------------------\n\n" +
                    "<기존 도로>\n" +
                    "1. EZ 업데이트 패널을 엽니다: <Ctrl+V> 를 눌러 패널을 ON/OFF\n" +
                    "   (또는 <왼쪽 위 아이콘> 도 같은 기능).\n" +
                    "2. 아래 패널에서 구역 아이콘을 선택합니다.\n" +
                    "3. 도로 위에 마우스를 올려 미리보기 합니다.\n" +
                    "4. <RMB 순환>: 양쪽 → 왼쪽만 → 오른쪽만 → 없음 → ...\n" +
                    "5. <LMB 한 번>: 적용(확정).\n" +
                    "6. <LMB 누른 채 드래그>하여 여러 도로 구간에 적용한 뒤 놓습니다.\n" +
                    "7. <취소:> 마우스를 밖으로 옮기고 **LMB** 를 놓습니다.\n\n" +
                    "-------------------------------------------\n" +
                    "<선택 버튼>\n" +
                    "• <Contour> 는 지형의 등고선을 표시합니다." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "레거시 우클릭 순환" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF 권장**. RMB 로 4가지 모드를 모두 순환합니다:\n" +
                    "**양쪽 → 왼쪽만 → 오른쪽만 → 없음 → ...**\n\n" +
                    "장점: 마우스를 도구 패널로 다시 가져갈 필요가 줄어듭니다.\n\n" +
                    "--------------------------------------\n" +
                    "레거시가 ON 이면: RMB 는 두 개의 별도 세트만 전환합니다:\n" +
                    "왼쪽만 ↔ 오른쪽만\n" +
                    "양쪽 ↔ 없음" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 업데이트 패널 토글" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "모드 이름" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "이 모드의 표시 이름입니다." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "버전" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "현재 모드 버전입니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "제작자의 Paradox Mods 페이지를 엽니다." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "모드 Discord에 참가합니다." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
