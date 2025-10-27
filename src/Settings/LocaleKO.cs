// File: src/Settings/LocaleKO.cs
// Korean (ko-KR) strings for Options UI + Panel text.

namespace ARTZone.Settings
{
    using System.Collections.Generic;
    using ARTZone.Tools;
    using Colossal;

    public sealed class LocaleKO : IDictionarySource
    {
        private readonly Setting m_Settings;
        public LocaleKO(Setting setting) => m_Settings = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Settings title
                { m_Settings.GetSettingsLocaleID(), "ART — 존 설정" },

                // Tabs
                { m_Settings.GetOptionTabLocaleID(Setting.kActionsTab), "동작" },
                { m_Settings.GetOptionTabLocaleID(Setting.kAboutTab),   "정보" },

                // Groups
                { m_Settings.GetOptionGroupLocaleID(Setting.kToggleGroup),     "존 옵션" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "키 설정" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutInfoGroup),  "" },
                { m_Settings.GetOptionGroupLocaleID(Setting.kAboutLinksGroup), "" },

                // Toggles
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "이미 지정된 존 셀 삭제 안 함" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),  "미리보기/적용 중 이미 존이 지정된 셀을 덮어쓰지 않습니다.\n <사용 권장>" },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "이미 사용 중인 셀 삭제 안 함" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),  "미리보기/적용 중 건물이 있는 등 이미 점유된 셀을 덮어쓰지 않습니다.\n <사용 권장>" },

                // Keybind
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.ToggleZoneTool)), "패널 표시 / 숨기기" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.ToggleZoneTool)),  "ART-Zone 패널을 표시/숨깁니다 (기본 Shift+Z)." },

                // Binding title
                { m_Settings.GetBindingKeyLocaleID(ARTZoneMod.kToggleToolActionName), "ART-Zone 패널 표시 / 숨기기" },

                // Panel text
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "존 변경 도구" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]",
                  "도로의 존을 변경합니다: 양쪽, 왼쪽, 오른쪽 또는 없음. 마우스 오른쪽 버튼으로 선택을 전환하고 왼쪽 버튼으로 확정합니다." },

                // About tab
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.NameText)),    "모드 이름" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.NameText)),     "이 모드의 표시 이름." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "버전" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "현재 모드 버전." },

                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenMods)),    "Paradox Mods" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenMods)),     "Paradox Mods 페이지 열기." },
                { m_Settings.GetOptionLabelLocaleID(nameof(Setting.OpenDiscord)), "Discord" },
                { m_Settings.GetOptionDescLocaleID(nameof(Setting.OpenDiscord)),  "모드 Discord에 참가합니다." },
            };
            return d;
        }

        public void Unload()
        {
        }
    }
}
