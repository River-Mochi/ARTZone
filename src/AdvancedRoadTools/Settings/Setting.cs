// File: src/Settings/Setting.cs
// Options UI + keybinding definition. Actions first, About second.
// About tab shows Mod Name + Version (read-only) and two side-by-side link buttons.

namespace ARTZone
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine; // Application.OpenURL

    [FileLocation("ModsSettings/ART-Zone/ART-Zone")]

    // Tabs order
    [SettingsUITabOrder(kActionsTab, kAboutTab)]

    // Group order (we hide the About group header)
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)] // (no header for About groups)

    // Declare actions (names match ARTZoneMod constants)
    [SettingsUIMouseAction(ARTZoneMod.kInvertZoningActionName, ActionType.Button, usages: new[] { "Game" })]
    [SettingsUIKeyboardAction(ARTZoneMod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Groups on Actions
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";

        // Groups on About (header hidden)
        public const string kAboutInfoGroup = "Info";
        public const string kAboutLinksGroup = "Links";

        public Setting(IMod mod) : base(mod) { }

        // ====== ACTIONS TAB ======

        // Toggles
        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // Keybindings
        [SettingsUIMouseBinding(BindingMouse.Right, ARTZoneMod.kInvertZoningActionName)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        // Shift + Z toggles the tool
        [SettingsUIKeyboardBinding(BindingKeyboard.Z, ARTZoneMod.kToggleToolActionName, shift: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // ====== ABOUT TAB ======

        // Read-only meta
        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string NameText => "ART-Zone";

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string VersionText => ARTZoneMod.VersionShort;

#if DEBUG
        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string InformationalVersionText => ARTZoneMod.InformationalVersion;
#endif

        // External links (side-by-side via the same UIButtonGroup)
        private const string UrlParadoxMods = "TBD";
        private const string UrlDiscord = "https://discord.gg/HTav7ARPs2";

        [SettingsUIButtonGroup(kAboutLinksGroup)]
        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenParadoxModsButton
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlParadoxMods);
                }
                catch (Exception ex) { ARTZoneMod.s_Log.Warn($"[ART] Failed to open Paradox Mods: {ex.Message}"); }
            }
        }

        [SettingsUIButtonGroup(kAboutLinksGroup)]
        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenDiscordButton
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlDiscord);
                }
                catch (Exception ex) { ARTZoneMod.s_Log.Warn($"[ART] Failed to open Discord: {ex.Message}"); }
            }
        }

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }
    }
}
