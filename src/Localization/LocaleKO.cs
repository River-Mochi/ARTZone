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
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup),         "보호" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup),     "키 설정" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup),  "호환성" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup),             "화면 표시" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup),          "사용법" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● 건물 제거 방지" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**건물 = 점유된 셀**. 미리보기/적용으로 건물이 폐건물 처리되는 것을 막습니다.\n\n" +
                    "**[ ✓ ] ON 권장.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● 이미 칠한/구역 지정된 칸 초기화 방지" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "미리보기/적용 중 이미 구역 지정된 셀을 초기화하지 않습니다.\n\n" +
                    "**[ ✓ ] ON 권장.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "EZ 업데이트 패널 ON/OFF" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning 패널을 빠르게 표시하는 **키 설정**\n" +
                    "**기본 Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ContourIconText)), "등고선" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ContourIconText)), "" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "버튼 표시" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] ON**, 기존 도로 업데이트 패널에 등고선 버튼을 표시합니다.\n\n" +
                    "● 더 작은 패널을 원하거나 다른 모드가 등고선을 처리하면 OFF로 두세요." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ 유리 패널" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] ON**, 더 선명한 반투명 패널 스타일을 사용합니다.\n" +
                    "**[   ] OFF** = 회색 패널.\n\n" +
                    "<시각 효과 전용.>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "테두리 색상: 제거 미리보기" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "제거될 셀 미리보기의 테두리 색상입니다.\n\n" +
                    "<주황> = 더 밝고 보기 쉬움.\n" +
                    "<빨강> = 더 강한 빨간 테두리 대비.\n" +
                    "<핑크> = 밝고 재미있는 색상.\n" +
                    "<보라> = 부드럽지만 잘 보이는 대비.\n" +
                    "<바닐라 빨강> = 게임 기본 모양과 맞춤." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "테두리 불투명도" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "제거 미리보기 테두리의 불투명도를 조절합니다.\n\n" +
                    "<100%> 미리보기의 기본 반투명 효과 유지.\n" +
                    "<0%> 테두리 숨김." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "채우기 색상: 제거 미리보기" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "제거 가능한 셀 미리보기의 채우기 색상입니다.\n\n" +
                    "<바닐라 빨강> = 현재 게임 기본 모양.\n" +
                    "<흰색> = 더 깔끔한 대비.\n" +
                    "<주황> = 주황 테두리와 맞춤.\n" +
                    "<핑크> = 밝고 재미있는 색상.\n" +
                    "<보라> = 부드럽지만 잘 보이는 대비.\n" +
                    "<없음> = 테두리만, 미니멀" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "채우기 불투명도" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "제거 가능한 셀 미리보기 채우기의 불투명도를 조절합니다.\n\n" +
                    "<100%> 미리보기의 기본 반투명 효과 유지.\n" +
                    "<0%> 채우기 숨김.\n" +
                    "<제거 채우기>가 <없음>이면 무시됩니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyHighContrastPreset)), "고대비" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyHighContrastPreset)),
                    "프리셋 내용:\n" +
                    "<유리 패널 ON>\n" +
                    "<주황 테두리>\n" +
                    "<테두리 불투명도 100%>\n" +
                    "<채우기 없음.>" },


                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ApplyGameColorPreset)), "게임 색상" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ApplyGameColorPreset)),
                    "게임의 구역 도구 미리보기와 맞도록 빨간 테두리+채우기를 사용합니다." },
  
                // Dropdown values
                { "EasyZoning.Dropdown.Color.Orange", "주황" },
                { "EasyZoning.Dropdown.Color.Red", "빨강" },
                { "EasyZoning.Dropdown.Color.Pink", "핑크" },
                { "EasyZoning.Dropdown.Color.Purple", "보라" },
                { "EasyZoning.Dropdown.Color.VanillaRed", "바닐라 빨강" },
                { "EasyZoning.Dropdown.Color.White", "흰색" },
                { "EasyZoning.Dropdown.Fill.NoneBorderOnly", "없음 (테두리만)" },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "사용법 표시" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)),
                    "아래 **사용법 안내**를 표시하거나 숨깁니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<기존 도로>\n" +
                    "1. EZ 업데이트 패널 열기: <Ctrl+V>를 눌러 패널 ON/OFF\n" +
                    "   (<왼쪽 위 아이콘>도 같은 기능).\n" +
                    "2. 3개의 EZ 아이콘으로 양쪽 / 왼쪽 / 오른쪽을 선택.\n" +
                    "   같은 버튼을 다시 누르면 없음.\n" +
                    "3. 도로에 마우스를 올려 미리보기.\n" +
                    "4. 빨간 미리보기 = 제거될 셀.\n" +
                    "5. <RMB 순환>: 양쪽 → 왼쪽 → 오른쪽 → 없음 → ...\n" +
                    "6. <LMB 한 번>: 적용(고정).\n" +
                    "7. <LMB 누른 채 드래그>로 여러 도로 구간을 지나간 뒤 놓으면 적용.\n" +
                    "8. <취소:> 마우스를 밖으로 옮기고 **LMB** 놓기.\n\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = 오른쪽 클릭, <LMB> = 왼쪽 클릭\n" +
                    "-----------------------------------------\n\n" +
                    "<새 도로>\n" +
                    "1. 도로 패널 열기(도로 선택).\n" +
                    "2. 도로 도구 패널 아래쪽에서 3개 EZ 아이콘으로 양쪽 / 왼쪽 / 오른쪽 선택.\n" +
                    "   같은 버튼을 다시 누르면 없음.\n" +
                    "3. 평소처럼 그리기.\n\n" +
                    "-------------------------------------------\n" +
                    "<지형 버튼>\n" +
                    "<◎ 등고선>은 지형 높이 선을 표시합니다."
                },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "레거시 오른쪽 클릭 순환" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**권장하지 않음**\n" +
                    "OFF는 최신 방식: RMB가 4개 모드를 순환: **양쪽 → 왼쪽 → 오른쪽 → 없음 → ...**\n\n" +
                    "장점: 마우스를 도구 패널로 다시 옮길 일이 줄어듭니다.\n\n" +
                    "<-------------------------------------->\n" +
                    "레거시가 ON이면 RMB가 두 묶음으로만 전환되어 마우스 이동이 더 필요합니다:\n" +
                    "왼쪽 ↔ 오른쪽만\n" +
                    "양쪽 ↔ 없음만"
                },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 업데이트 패널 전환" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "모드 이름" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "이 모드의 표시 이름." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "버전" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "현재 모드 버전." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "제작자의 Paradox Mods 페이지를 엽니다." },
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
