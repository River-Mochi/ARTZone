// File: src/Settings/Setting.cs
// Purpose: Options UI + one rebindable hotkey (Ctrl+Z).
// Notes:
//   - Only Ctrl+Z is declared here (Options → Keybindings).
//   - RMB is NOT declared here; the tool uses CS2's built-in Secondary Apply to cycle.

namespace EasyZoning
{
    using Colossal.IO.AssetDatabase; // FileLocation attribute (settings storage path)
    using Game.Input;                // ProxyBinding, BindingKeyboard, ActionType
    using Game.Modding;              // IMod, ModSetting base
    using Game.Settings;             // Settings UI attributes (tabs/groups/sections/buttons)
    using System;                    // Exception in URL open handlers
    using UnityEngine;               // Application.OpenURL

    // Persisted settings location
    [FileLocation("ModsSettings/EasyZoning/EasyZoning")]

    // Tabs & groups
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kLegacyGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup, kLegacyGroup)]

    // Declare ONLY the keyboard action (Ctrl+Z). RMB uses the game’s built-in Secondary Apply action.
    [SettingsUIKeyboardAction(Mod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Groups
        public const string kToggleGroup = "Zoning Tools";
        public const string kKeybindingGroup = "Key bindings";
        public const string kLegacyGroup = "Legacy Tool";
        public const string kAboutInfoGroup = "Info";
        public const string kAboutLinksGroup = "Links";

        public Setting(IMod mod) : base(mod)
        {
        }

        // --- Toggles ---

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveZonedCells
        {
            get; set;
        } = true;

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveOccupiedCells
        {
            get; set;
        } = true;

        // --- Key bindings (only Ctrl+Z exposed) ---

        [SettingsUIKeyboardBinding(BindingKeyboard.Z, Mod.kToggleToolActionName, ctrl: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // --- Legacy Tool behavior ---

        // RMB cycling (update-existing-roads tool):
        // Legacy
        // - OFF (default): full 4-cycle (Both → Left → Right → None → Both)
        // - ON: legacy 2-set toggle (Left ↔ Right, Both ↔ None)
        [SettingsUISection(kActionsTab, kLegacyGroup)]
        public bool LegacyRightClickCycle
        {
            get; set;
        } = false;

        // --- About (read-only) ---

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string NameText => "Easy Zoning";

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
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
            LegacyRightClickCycle = false;
        }
    }
}
