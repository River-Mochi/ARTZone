// File: src/Settings/Setting.cs
// Purpose: Options UI + one rebindable hotkey (default Ctrl+V).
// Notes:
//   - No UI strings live here. All text is in LocaleEN.cs for translation.
//   - Usage instructions are a multiline row whose text is localized.
//   - Usage row is hidden when ShowUsage is OFF.

namespace EasyZoning
{
    using Colossal.IO.AssetDatabase; // FileLocation attribute (settings storage path)
    using Game.Input;                // ProxyBinding, BindingKeyboard, ActionType
    using Game.Modding;              // IMod, ModSetting base
    using Game.Settings;             // Settings UI attributes
    using System;                    // Exception in URL open handlers
    using UnityEngine;               // Application.OpenURL

    [FileLocation("ModsSettings/EasyZoning/EasyZoning")]
    [SettingsUITabOrder(kActionsTab, kLegacyTab, kAboutTab)]
    [SettingsUIGroupOrder(
        kProtectGroup, kKeybindingGroup, kCompatibilityGroup, kUiGroup, kUsageGroup,
        kLegacyGroup,
        kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(
        kProtectGroup, kUiGroup, kUsageGroup)] // kLegacyGroup and other names omitted on purpose so they don't show in UI.
    [SettingsUIKeyboardAction(Mod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kLegacyTab = "Legacy";
        public const string kAboutTab = "About";

        // Groups
        public const string kProtectGroup = "Zoning Tools";
        public const string kKeybindingGroup = "Key bindings";
        public const string kCompatibilityGroup = "Compatibility";
        public const string kUiGroup = "Better UI";
        public const string kUsageGroup = "Usage";
        public const string kLegacyGroup = "Legacy Tool";
        public const string kAboutInfoGroup = "Info";
        public const string kAboutLinksGroup = "Links";

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

        // Default ON.
        [SettingsUISection(kActionsTab, kCompatibilityGroup)]
        public bool ShowContourButton { get; set; } = true;

        // --- UI ---

        // Default ON.
        [SettingsUISection(kActionsTab, kUiGroup)]
        public bool UseGlassPanel { get; set; } = true;

        // Default ON.
        [SettingsUISection(kActionsTab, kUiGroup)]
        public bool UseOrangeRemovePreviewEdge { get; set; } = true;

        [SettingsUISlider(min = 20, max = 100, step = 5, scalarMultiplier = 1, unit = "percentage")]
        [SettingsUISection(kActionsTab, kUiGroup)]
        public int RemovePreviewEdgeOpacityPercent { get; set; } = 100;

        // --- Usage (Actions tab) ---

        // Default OFF.
        [SettingsUISection(kActionsTab, kUsageGroup)]
        public bool ShowUsage { get; set; } = false;

        [SettingsUIMultilineText]
        [SettingsUIHideByCondition(typeof(Setting), nameof(HideUsageText))]
        [SettingsUISection(kActionsTab, kUsageGroup)]
        public string UsageText => string.Empty;

        private bool HideUsageText( ) => !ShowUsage;

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
            UseGlassPanel = true;
            UseOrangeRemovePreviewEdge = true;
            RemovePreviewEdgeOpacityPercent = 100;
            ShowUsage = false;
            LegacyRightClickCycle = false;
        }
    }
}
