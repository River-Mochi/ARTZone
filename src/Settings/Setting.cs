// File: src/Settings/Setting.cs
// Purpose: Options UI + keybinding definition (CO API).
// Shows ONE rebindable entry in Actions → Key bindings:
//   • Toggle Zone Control panel → default Shift+Z
//
// RMB (right-click) keybind is NOT exposed here on purpose — the tool uses the
// vanilla ToolBaseSystem 'cancelAction' for flipping, so RMB remains the
// intuitive default and cannot be broken by user remapping inside this mod.

namespace AdvancedRoadTools
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine;

    // Persisted settings location
    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]

    // Tabs & groups
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)]

    // Declare ONLY the keyboard action (Shift+Z). RMB is handled by vanilla cancelAction.
    [SettingsUIKeyboardAction(AdvancedRoadToolsMod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Groups
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";
        public const string kAboutInfoGroup = "Info";
        public const string kAboutLinksGroup = "Links";

        public Setting(IMod mod) : base(mod) { }

        // --- Toggles ---

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // --- Key bindings (only Shift+Z is exposed) ---

        [SettingsUIKeyboardBinding(BindingKeyboard.Z, AdvancedRoadToolsMod.kToggleToolActionName, shift: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // --- About (read-only labels/buttons) ---

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string NameText => "Advanced Road Tools — Zone Controller";

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string VersionText => AdvancedRoadToolsMod.VersionShort;

#if DEBUG
        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string InformationalVersionText => AdvancedRoadToolsMod.InformationalVersion;
#endif

        private const string UrlMods = "TBD";
        private const string UrlDiscord = "https://discord.gg/HTav7ARPs2";

        [SettingsUIButtonGroup(kAboutLinksGroup)]
        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenMods
        {
            set
            {
                try
                {
                    Application.OpenURL(UrlMods);
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

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }
    }
}
