// File: src/ARTZone/Settings/Setting.cs
// Options UI + keybinding definition (mouse). Toggles shown first, keybinding below.

namespace ARTZone
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine;              // Application.OpenURL

    [FileLocation("ModsSettings/ART-Zone/ART-Zone")]
    // Show toggles group first, keybinding group, then URL buttons
    [SettingsUITabOrder(ActionsTab, AboutTab)]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, InfoGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)]   // InfoGroup  header omitted on purpose.


    // Define the input action once. (Mouse action; Button type)
    [SettingsUIMouseAction(ARTZoneMod.kInvertZoningActionName,
                           ActionType.Button,
                           usages: new[] { "Zone Controller Tool" })]
    public sealed class Setting : ModSetting
    {
        // ---- Tabs ----
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Tab Sections ----
        public const string kSection = "Main";

        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";

        public Setting(IMod mod) : base(mod) { }


        // === External links ===
        private const string UrlParadoxMods = "TBD";
        private const string UrlDiscord = "https://discord.gg/HTav7ARPs2";

        // === Toggles (top group) ===
        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // === Keybinding (bottom group) ===
        // Default to RMB; user can unassign or rebind to other mouse buttons.
        [SettingsUIMouseBinding(BindingMouse.Right, ARTZoneMod.kInvertZoningActionName)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }


        // ---- About tab links: order below determines button order ----
        [SettingsUIButtonGroup("SocialLinks")]
        [SettingsUIButton]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool OpenParadoxModsButton
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlParadoxMods);
                }
                catch (Exception ex) { Mod.log.Warn($"Failed to open Paradox Mods: {ex.Message}"); }
            }
        }

        [SettingsUIButtonGroup("SocialLinks")]
        [SettingsUIButton]
        [SettingsUISection(AboutTab, InfoGroup)]
        public bool OpenDiscordButton
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlDiscord);
                }
                catch (Exception ex) { Mod.log.Warn($"Failed to open Discord: {ex.Message}"); }
            }
        }

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }
    }
}
