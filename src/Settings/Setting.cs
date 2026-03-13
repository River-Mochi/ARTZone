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
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kUsageGroup, kLegacyGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup, kUsageGroup)]  // kLegacyGroup name omitted on purpose to not display.
    [SettingsUIKeyboardAction(Mod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kLegacyTab = "Legacy";
        public const string kAboutTab = "About";

        // Groups
        public const string kToggleGroup = "Zoning Tools";
        public const string kKeybindingGroup = "Key bindings";
        public const string kUsageGroup = "Usage";
        public const string kLegacyGroup = "Legacy Tool";
        public const string kAboutInfoGroup = "Info";
        public const string kAboutLinksGroup = "Links";

        public Setting(IMod mod) : base(mod)
        {
        }

        // --- Zone Options ---

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // --- Key bindings ---

        [SettingsUIKeyboardBinding(BindingKeyboard.V, Mod.kToggleToolActionName, ctrl: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // --- USAGE (Actions tab) ---

        // Default OFF: show the instructions in options menu.
        [SettingsUISection(kActionsTab, kUsageGroup)]
        public bool ShowUsage { get; set; } = false;

        // Multiline instructions block (localized).
        [SettingsUIMultilineText]
        [SettingsUIHideByCondition(typeof(Setting), nameof(HideUsageText))]
        [SettingsUISection(kActionsTab, kUsageGroup)]
        public string UsageText => string.Empty;

        // Return true to hide UsageText.
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
                catch (Exception) { }
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
                catch (Exception) { }
            }
        }

        public override void SetDefaults( )
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;

            ShowUsage = false;
            LegacyRightClickCycle = false;
        }
    }
}
