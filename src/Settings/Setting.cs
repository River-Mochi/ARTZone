// File: src/Settings/Setting.cs
// Options UI + keybinding definition. RMB = invert zoning. Shift+Z = toggle tool on/off.

namespace ARTZone
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine; // Application.OpenURL

    [FileLocation("ModsSettings/ART-Zone/ART-Zone")]
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup, kAboutLinksGroup)]

    // Declare actions once
    [SettingsUIMouseAction(ARTZoneMod.kInvertZoningActionName, ActionType.Button, usages: new[] { "Zone Controller Tool" })]
    [SettingsUIKeyboardAction(ARTZoneMod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Groups
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";
        public const string kAboutLinksGroup = "Links";

        // Section name
        public const string kSection = "Main";

        public Setting(IMod mod) : base(mod) { }

        // --- Toggles ---
        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // --- Keybindings (RMB invert) ---
        [SettingsUIMouseBinding(BindingMouse.Right, ARTZoneMod.kInvertZoningActionName)]
        [SettingsUISection(kSection, kKeybindingGroup)]

        public ProxyBinding InvertZoning
        {
            get; set;
        }

        // Shift+Z toggle (open Zoning Side panel)
        [SettingsUIKeyboardBinding(BindingKeyboard.Z, ARTZoneMod.kToggleToolActionName, shift: true)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // --- About tab link buttons ---
        private const string UrlParadoxMods = "https://mods.paradoxplaza.com/";
        private const string UrlDiscord = "https://discord.gg/HTav7ARPs2";

        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenParadoxModsButton
        {
            set
            {
                TryOpen(UrlParadoxMods);
            }
        }

        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenDiscordButton
        {
            set
            {
                TryOpen(UrlDiscord);
            }
        }

        private static void TryOpen(string url)
        {
            try
            {
                Application.OpenURL(url);
            }
            catch (Exception ex) { ARTZoneMod.s_Log.Warn($"OpenURL failed: {ex.Message}"); }
        }

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }
    }
}
