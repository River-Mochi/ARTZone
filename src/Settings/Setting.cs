// File: src/Settings/Setting.cs
// Options UI + keybinding definition. Actions tab first, About tab second.

namespace ARTZone
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine; // Application.OpenURL

    [FileLocation("ModsSettings/ART-Zone/ART-Zone")]

    // === Tab order: Actions first, About second ===
    [SettingsUITabOrder(kActionsTab, kAboutTab)]

    // === Group order within Actions tab ===
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)] // hide About group header

    // --- Declare actions (names must match ARTZoneMod constants) ---
    [SettingsUIMouseAction(ARTZoneMod.kInvertZoningActionName, ActionType.Button, usages: new[] { "Game" })]
    [SettingsUIKeyboardAction(ARTZoneMod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Sections (tabs have their own sections)
        public const string kSection = "Main";

        // Groups on Actions tab
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";

        // Group on About tab (header name hidden)
        public const string kAboutLinksGroup = "Links";

        public Setting(IMod mod) : base(mod) { }

        // ====== TOGGLES (Actions tab) ======
        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // ====== KEYBINDINGS (Actions tab) ======
        // RMB invert
        [SettingsUIMouseBinding(BindingMouse.Right, ARTZoneMod.kInvertZoningActionName)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        // Shift+Z toggles the tool (matches UI GameTopRight button behavior)
        [SettingsUIKeyboardBinding(BindingKeyboard.Z, ARTZoneMod.kToggleToolActionName, shift: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // ====== ABOUT tab: external links ======
        private const string UrlParadoxMods = "TBD";
        private const string UrlDiscord = "https://discord.gg/HTav7ARPs2";

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
