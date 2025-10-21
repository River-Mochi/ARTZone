// File: src/AdvancedRoadTools/Settings/Setting.cs
// Options UI + keybinding definitions using Colossal API ONLY.

namespace AdvancedRoadTools
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine; // Application.OpenURL

    // Where your settings asset lives (folder/name). Adjust if you prefer another path.
    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]

    // Tabs order
    [SettingsUITabOrder(kActionsTab, kAboutTab)]

    // Group order (we hide About group headers)
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)] // (no header for About groups)

    // Declare actions (names must match AdvancedRoadToolsMod constants)
    [SettingsUIMouseAction(AdvancedRoadToolsMod.kInvertZoningActionName, ActionType.Button, usages: new[] { "Game" })]
    [SettingsUIKeyboardAction(AdvancedRoadToolsMod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
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

        // Example toggles (keep if you actually use these in tool logic)
        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // Keybindings (Colossal API ONLY — no Unity.InputSystem.Key anywhere)

        // Mouse: Invert zoning while tool is active (default: RMB)
        [SettingsUIMouseBinding(BindingMouse.Right, AdvancedRoadToolsMod.kInvertZoningActionName)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        // Keyboard: Shift + Z toggles our tool
        [SettingsUIKeyboardBinding(BindingKeyboard.Z, AdvancedRoadToolsMod.kToggleToolActionName, shift: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // ====== ABOUT TAB ======

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string NameText => AdvancedRoadToolsMod.Name;

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string VersionText => AdvancedRoadToolsMod.VersionShort;

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
                catch (Exception ex) { AdvancedRoadToolsMod.s_Log.Warn($"[ART] Failed to open Paradox Mods: {ex.Message}"); }
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
                catch (Exception ex) { AdvancedRoadToolsMod.s_Log.Warn($"[ART] Failed to open Discord: {ex.Message}"); }
            }
        }

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }
    }
}
