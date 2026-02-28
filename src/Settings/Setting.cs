// File: src/Settings/Setting.cs
// Purpose: Options UI + One rebindable entry (Ctrl+Z).
// Note: RMB is *not* declared here; the Game’s own RMB/cancel bindings remain vanilla.
// Later in the tool, the RMB is read for preview flip.

namespace EasyZoning
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using System;
    using UnityEngine;

    // Persisted settings location (rebranded)
    [FileLocation("ModsSettings/EasyZoning/EasyZoning")]

    // Tabs & groups
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)]

    // Declare ONLY the keyboard action (Ctrl+Z). RMB is vanilla cancelAction.
    [SettingsUIKeyboardAction(Mod.kToggleToolActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Groups
        public const string kToggleGroup = "Zoning Tools";
        public const string kKeybindingGroup = "Key bindings";
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
        }
    }
}
