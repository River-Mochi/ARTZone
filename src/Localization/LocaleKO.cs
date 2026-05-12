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
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "작업" },
                { m_Settings.GetOptionTabLocaleID(Setting.kLegacyTab), "레거시" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab), "정보" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kProtectGroup), "보호" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "키 설정" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kCompatibilityGroup), "호환성" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUiGroup), "시각 효과" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kUsageGroup), "사용법" },

                // Legacy group header hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kLegacyGroup), "" },

                // About group headers hidden
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Protections
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "● 건물 제거 방지" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "**건물 = 점유된 셀**. 새 구역 미리보기/적용 때문에 기존 건물이 철거 대상으로 처리되는 것을 막습니다.\n" +
                    "\n" +
                    "**[ ✓ ] 켜기 권장.**" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "● 이미 칠했거나 구역 지정된 칸 초기화 방지" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "미리보기/적용 중 이미 구역 지정된 셀을 초기화하지 않습니다.\n" +
                    "\n" +
                    "**[ ✓ ] 켜기 권장.**" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "업데이트 패널 On/Off" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),
                    "Easy Zoning 패널을 빠르게 여는 **키 설정**\n" +
                    "**기본값 Ctrl+V**" },

                // Compatibility
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowContourButton)), "◉ 등고선 버튼" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowContourButton)),
                    "**[ ✓ ] 켜짐**, 모드의 기존 도로 업데이트 패널에 지형 Contour 버튼을 표시합니다.\n" +
                    "\n" +
                    "● 더 작은 패널을 원하거나 다른 모드가 지형 선을 처리한다면 끄세요." },

                // UI
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UseGlassPanel)), "◉ 글래스 패널" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UseGlassPanel)),
                    "**[ ✓ ] 켜짐**, 패널에 투명한 반투명 스타일을 사용합니다.\n" +
                    "**[   ] 꺼짐**, 회색 패널을 사용합니다.\n" +
                    "\n" +
                    "시각 스타일만 변경합니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewBorderStyle)), "제거 셀 테두리" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewBorderStyle)),
                    "제거될 셀 미리보기의 테두리 색입니다.\n" +
                    "\n" +
                    "<주황색> = 더 밝고 보기 쉽습니다.\n" +
                    "<바닐라 빨강> = 게임 기본 모습과 맞춥니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)), "테두리 불투명도" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewEdgeOpacityPercent)),
                    "제거 미리보기 테두리의 불투명도를 조정합니다.\n" +
                    "\n" +
                    "<100%>는 미리보기의 기본 반투명도를 유지합니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillStyle)), "제거 셀 채우기" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillStyle)),
                    "제거될 셀 미리보기 오버레이의 채우기 스타일입니다.\n" +
                    "\n" +
                    "<바닐라 빨강> = 현재 게임 모습.\n" +
                    "<흰색> = 더 깔끔한 대비.\n" +
                    "<주황색> = 주황색 테두리와 맞춤.\n" +
                    "<없음> = 테두리만 표시." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)), "채우기 불투명도" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemovePreviewFillOpacityPercent)),
                    "제거 가능한 셀 미리보기의 채우기 불투명도를 조정합니다.\n" +
                    "\n" +
                    "<100%>는 미리보기의 기본 반투명도를 유지합니다.\n" +
                    "<제거 채우기>가 <없음>이면 무시됩니다." },

                // Usage toggle + multiline block
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ShowUsage)), "설명 표시" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ShowUsage)), "아래 **사용 설명**을 표시하거나 숨깁니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.UsageText)),
                    "<새 도로>\n" +
                    "1. 도로 패널을 엽니다(도로 선택).\n" +
                    "2. 도로 도구 패널 아래쪽에서 EZ 아이콘 3개로 양쪽 / 왼쪽 / 오른쪽을 선택합니다.\n" +
                    "   선택된 버튼을 다시 클릭하면 없음으로 바뀝니다.\n" +
                    "3. 평소처럼 그립니다.\n" +
                    "\n" +
                    "-----------------------------------------\n" +
                    "  <RMB> = 오른쪽 클릭, <LMB> = 왼쪽 클릭\n" +
                    "-----------------------------------------\n" +
                    "\n" +
                    "<기존 도로>\n" +
                    "1. EZ Update 패널 열기: <Ctrl+V>를 눌러 패널 On/Off\n" +
                    "   (<왼쪽 위 아이콘>도 같은 기능).\n" +
                    "2. EZ 아이콘 3개로 양쪽 / 왼쪽 / 오른쪽을 선택합니다.\n" +
                    "   버튼을 다시 클릭하면 없음으로 바뀝니다.\n" +
                    "3. 도로 위에 마우스를 올려 미리보기.\n" +
                    "4. 빨간 미리보기 = 제거될 셀.\n" +
                    "5. <RMB 순환>: 양쪽 → 왼쪽 → 오른쪽 → 없음 → ...\n" +
                    "6. <LMB 한 번>: 적용(고정).\n" +
                    "7. <LMB 길게 누르기 + 드래그> 여러 도로 구간을 따라 이동한 뒤 놓으면 적용.\n" +
                    "8. <취소:> 마우스를 멀리 이동하고 **LMB**를 놓습니다.\n" +
                    "\n" +
                    "-------------------------------------------\n" +
                    "<선택 버튼>\n" +
                    "• <등고선>은 지형 고도선을 표시합니다." },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.UsageText)), "" },

                // Legacy
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.LegacyRightClickCycle)), "레거시 오른쪽 클릭 순환" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.LegacyRightClickCycle)),
                    "**OFF 권장**\n" +
                    "Off이면 RMB가 4가지 모드를 모두 순환합니다: **양쪽 → 왼쪽 → 오른쪽 → 없음 → ...**\n" +
                    "\n" +
                    "비활성 장점: 마우스를 도구 패널로 다시 옮길 일이 줄어듭니다.\n" +
                    "\n" +
                    "--------------------------------------\n" +
                    "레거시가 ON이면 RMB가 두 개의 별도 그룹에서만 전환됩니다:\n" +
                    "왼쪽 ↔ 오른쪽만\n" +
                    "양쪽 ↔ 없음만" },

                // Keybinding dialog title
                { m_Settings.GetBindingKeyLocaleID(Mod.kToggleToolActionName), "Easy Zoning 업데이트 패널 전환" },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)), "모드 이름" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)), "이 모드의 표시 이름입니다." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "버전" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)), "현재 모드 버전입니다." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenParadox)), "작성자의 Paradox Mods 페이지를 엽니다." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)), "모드 Discord에 참여합니다." },
            };

            return d;
        }

        public void Unload( )
        {
        }
    }
}
