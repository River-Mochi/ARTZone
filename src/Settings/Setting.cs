// File: src/Settings/Setting.cs
// Purpose: Options UI + one rebindable hotkey (default Ctrl+V).
// Notes:
//   - No UI strings live here. All text is in LocaleEN.cs for translation.
//   - Usage instructions are multiline row and text is localized.
//   - Usage row is hidden when ShowUsage is OFF.

namespace EasyZoning
{
    using Colossal.IO.AssetDatabase; // FileLocation attribute (settings storage path)
    using Game.Input;                // ProxyBinding, BindingKeyboard, ActionType
    using Game.Modding;              // IMod, ModSetting base
    using Game.Settings;             // Settings UI attributes
    using Game.UI.Localization;      // LocalizedString.Id for dropdown labels
    using Game.UI.Widgets;           // DropdownItem<T>
    using System;                    // Exception in URL open handlers
    using UnityEngine;               // Application.OpenURL

    [FileLocation("ModsSettings/EasyZoning/EasyZoning")]
    [SettingsUITabOrder(kActionsTab, kLegacyTab, kAboutTab)]
    [SettingsUIGroupOrder(
        kProtectGroup, kKeybindingGroup, kCompatibilityGroup, kUiGroup, kUsageGroup,
        kLegacyGroup,
        kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(
        kProtectGroup, kUiGroup, kUsageGroup)] // kLegacyGroup and other names omitted on purpose so omitted groups stay hidden in UI.
    [SettingsUIKeyboardAction(Mod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kLegacyTab = "Legacy";
        public const string kAboutTab = "About";

        // Groups
        public const string kProtectGroup = "Protections";
        public const string kKeybindingGroup = "Key bindings";
        public const string kCompatibilityGroup = "Compatibility";
        public const string kUiGroup = "Visuals";
        public const string kUsageGroup = "Usage";
        public const string kLegacyGroup = "Legacy Tool";
        public const string kAboutInfoGroup = "Info";
        public const string kAboutLinksGroup = "Links";

        private const string UsageIconPath = Mod.MainIconPath;

        public const string kRemovePreviewFillVanillaRed = "vanilla-red";
        public const string kRemovePreviewFillWhite = "white";
        public const string kRemovePreviewFillOrange = "orange";
        public const string kRemovePreviewFillNone = "none";
        public const string kRemovePreviewBorderOrange = "orange";
        public const string kRemovePreviewBorderRed = "red";
        public const string kRemovePreviewBorderVanillaRed = "vanilla-red";

        public Setting(IMod mod) : base(mod)
        {
        }

        // --- Zone Options ---

        [SettingsUISection(kActionsTab, kProtectGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        [SettingsUISection(kActionsTab, kProtectGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        // --- Key bindings ---

        [SettingsUIKeyboardBinding(BindingKeyboard.V, Mod.kToggleToolActionName, ctrl: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // --- Compatibility ---

        [SettingsUIMultilineText("Media/Tools/Snap Options/ContourLines.svg")]
        [SettingsUISection(kActionsTab, kCompatibilityGroup)]
        public string ContourIconText => string.Empty;

        // Default ON.
        [SettingsUISection(kActionsTab, kCompatibilityGroup)]
        public bool ShowContourButton { get; set; } = true;


        // --- UI ---

        // Default ON.
        [SettingsUISection(kActionsTab, kUiGroup)]
        public bool UseGlassPanel { get; set; } = true;

        [SettingsUISection(kActionsTab, kUiGroup)]
        [SettingsUIDropdown(typeof(Setting), nameof(GetRemovePreviewBorderStyleValues))]
        public string RemovePreviewBorderStyle { get; set; } = kRemovePreviewBorderOrange;

        // Compatibility shim for older locale keys / saved settings.
        // Intentionally ignored so updated installs default to the new orange-border mode.
        [SettingsUIHidden]
        public bool UseOrangeRemovePreviewEdge
        {
            get => RemovePreviewBorderStyle == kRemovePreviewBorderOrange;
            set { }
        }

        [SettingsUISlider(min = 0, max = 100, step = 5, scalarMultiplier = 1, unit = "percentage")]
        [SettingsUISection(kActionsTab, kUiGroup)]
        public int RemovePreviewEdgeOpacityPercent { get; set; } = 100;

        [SettingsUISection(kActionsTab, kUiGroup)]
        [SettingsUIDropdown(typeof(Setting), nameof(GetRemovePreviewFillStyleValues))]
        public string RemovePreviewFillStyle { get; set; } = kRemovePreviewFillNone;

        [SettingsUISlider(min = 0, max = 100, step = 5, scalarMultiplier = 1, unit = "percentage")]
        [SettingsUIDisableByCondition(typeof(Setting), nameof(IsRemovePreviewFillOpacityDisabled))]
        [SettingsUISection(kActionsTab, kUiGroup)]
        public int RemovePreviewFillOpacityPercent { get; set; } = 100;

        [SettingsUIButtonGroup(kUiGroup)]
        [SettingsUIButton]
        [SettingsUISection(kActionsTab, kUiGroup)]
        public bool ApplyHighContrastPreset
        {
            set
            {
                if (!value)
                {
                    return;
                }

                SetHighContrastPreset();
                ApplyAndSave();
            }
        }

        [SettingsUIButtonGroup(kUiGroup)]
        [SettingsUIButton]
        [SettingsUISection(kActionsTab, kUiGroup)]
        public bool ApplyGameColorPreset
        {
            set
            {
                if (!value)
                {
                    return;
                }

                SetGameColorPreset();
                ApplyAndSave();
            }
        }

        // --- Usage (Actions tab) ---

        // Default OFF.
        [SettingsUISection(kActionsTab, kUsageGroup)]
        public bool ShowUsage { get; set; } = false;

        [SettingsUIMultilineText(UsageIconPath)]
        [SettingsUIHideByCondition(typeof(Setting), nameof(HideUsageText))]
        [SettingsUISection(kActionsTab, kUsageGroup)]
        public string UsageText => string.Empty;

        private bool HideUsageText( ) => !ShowUsage;
        private bool IsRemovePreviewFillOpacityDisabled( ) => RemovePreviewFillStyle == kRemovePreviewFillNone;

        private void SetHighContrastPreset()
        {
            UseGlassPanel = true;
            RemovePreviewBorderStyle = kRemovePreviewBorderOrange;
            RemovePreviewEdgeOpacityPercent = 100;
            RemovePreviewFillStyle = kRemovePreviewFillNone;
            RemovePreviewFillOpacityPercent = 100;
        }

        private void SetGameColorPreset()
        {
            RemovePreviewBorderStyle = kRemovePreviewBorderVanillaRed;
            RemovePreviewEdgeOpacityPercent = 100;
            RemovePreviewFillStyle = kRemovePreviewFillVanillaRed;
            RemovePreviewFillOpacityPercent = 100;
        }

        public static DropdownItem<string>[] GetRemovePreviewBorderStyleValues( ) => new[]
        {
            new DropdownItem<string>
            {
                value = kRemovePreviewBorderOrange,
                displayName = LocalizedString.Id("EasyZoning.Dropdown.Color.Orange"),
            },
            new DropdownItem<string>
            {
                value = kRemovePreviewBorderRed,
                displayName = LocalizedString.Id("EasyZoning.Dropdown.Color.Red"),
            },
            new DropdownItem<string>
            {
                value = kRemovePreviewBorderVanillaRed,
                displayName = LocalizedString.Id("EasyZoning.Dropdown.Color.VanillaRed"),
            },
        };

        public static DropdownItem<string>[] GetRemovePreviewFillStyleValues( ) => new[]
        {
            new DropdownItem<string>
            {
                value = kRemovePreviewFillVanillaRed,
                displayName = LocalizedString.Id("EasyZoning.Dropdown.Color.VanillaRed"),
            },
            new DropdownItem<string>
            {
                value = kRemovePreviewFillWhite,
                displayName = LocalizedString.Id("EasyZoning.Dropdown.Color.White"),
            },
            new DropdownItem<string>
            {
                value = kRemovePreviewFillOrange,
                displayName = LocalizedString.Id("EasyZoning.Dropdown.Color.Orange"),
            },
            new DropdownItem<string>
            {
                value = kRemovePreviewFillNone,
                displayName = LocalizedString.Id("EasyZoning.Dropdown.Fill.NoneBorderOnly"),
            },
        };

        // --- Legacy (Legacy tab) ---

        // Default OFF.
        [SettingsUISection(kLegacyTab, kLegacyGroup)]
        public bool LegacyRightClickCycle { get; set; } = false;

        // --- About (read-only) ---

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string NameText => Mod.ModName;

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string VersionText =>
#if DEBUG
            Mod.ModVersion + " (DEBUG)";
#else
            Mod.ModVersion;
#endif

        private const string UrlParadox =
            "https://mods.paradoxplaza.com/authors/River-mochi/cities_skylines_2?games=cities_skylines_2&orderBy=desc&sortBy=best&time=alltime";
        private const string UrlDiscord = "https://discord.gg/HTav7ARPs2";

        [SettingsUIButtonGroup(kAboutLinksGroup)]
        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenParadox
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlParadox);
                }
                catch (Exception)
                {
                }
            }
        }

        [SettingsUIButtonGroup(kAboutLinksGroup)]
        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenDiscord
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlDiscord);
                }
                catch (Exception)
                {
                }
            }
        }

        public override void SetDefaults( )
        {
            RemoveOccupiedCells = true;
            RemoveZonedCells = true;
            ShowContourButton = true;
            SetHighContrastPreset();
            ShowUsage = false;
            LegacyRightClickCycle = false;
        }
    }
}
