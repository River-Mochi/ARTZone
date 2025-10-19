// File: src/Settings/Setting.cs
// Options UI: Actions (toggles+keybind) + About (links)

namespace ARTZone
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine; // Application.OpenURL

    [FileLocation("ModsSettings/ART-Zone/ART-Zone")]    // save Options settings here
    // Show tabs in this order:
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    // Show groups (section groups) in this order where they appear:
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutLinksGroup)]
    // Show group headers for these:
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup, kAboutLinksGroup)]
    // Define our mouse action once
    [SettingsUIMouseAction(ARTZoneMod.kInvertZoningActionName, ActionType.Button, usages: new[] { "Zone Controller Tool" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Section id used on the Actions tab
        public const string kSection = "Main";

        // Group names
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";
        public const string kAboutLinksGroup = "Links";

        public Setting(IMod mod) : base(mod) { }

        // === External links (About tab) ===
        private const string UrlParadoxMods = "https://mods.paradoxplaza.com/"; // TODO: set your mod page
        private const string UrlDiscord = "https://discord.gg/HTav7ARPs2";

        // === Toggles (Actions tab) ===
        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // === Keybinding (Actions tab) ===
        // Default: RMB (user can change in Options)
        [SettingsUIMouseBinding(BindingMouse.Right, ARTZoneMod.kInvertZoningActionName)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        // === About tab buttons ===
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        [SettingsUIButtonGroup("Social")]
        [SettingsUIButton]
        public bool OpenParadoxModsButton
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlParadoxMods);
                }
                catch (Exception ex) { ARTZoneMod.s_Log.Warn($"Open Paradox Mods failed: {ex.Message}"); }
            }
        }

        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        [SettingsUIButtonGroup("Social")]
        [SettingsUIButton]
        public bool OpenDiscordButton
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlDiscord);
                }
                catch (Exception ex) { ARTZoneMod.s_Log.Warn($"Open Discord failed: {ex.Message}"); }
            }
        }

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }
    }
}
