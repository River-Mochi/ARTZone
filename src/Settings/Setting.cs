// Options UI + keybinding definition (CO API)
namespace AdvancedRoadTools
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine;

    // Location of settings to preserve for next reboot
    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]

    // Tabs & groups
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)]

    // Declare actions (names must match the constants in AdvancedRoadToolsMod)
    [SettingsUIMouseAction(AdvancedRoadToolsMod.kInvertZoningActionName, ActionType.Button, usages: new[] { "Game" })]
    [SettingsUIKeyboardAction(AdvancedRoadToolsMod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";

        public const string kAboutInfoGroup = "Info";
        public const string kAboutLinksGroup = "Links";

        public Setting(IMod mod) : base(mod) { }

        // Toggles
        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kActionsTab, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // RMB to invert
        [SettingsUIMouseBinding(BindingMouse.Right, AdvancedRoadToolsMod.kInvertZoningActionName)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        // Shift+Z to toggle the panel/tool
        [SettingsUIKeyboardBinding(BindingKeyboard.Z, AdvancedRoadToolsMod.kToggleToolActionName, shift: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // About (read-only)
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
