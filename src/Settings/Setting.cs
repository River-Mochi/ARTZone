// File: src/Settings/Setting.cs
// Purpose: Options UI + one rebindable hotkey (default Ctrl+V).
// Notes:
//   - Keyboard action declared once (ToggleZoneTool), game shows it in Options.
//   - Default binding defined by SettingsUIKeyboardBinding (Ctrl+V).
//   - RMB cycling is not declared here; it uses the game’s built-in SecondaryApply tool action.

namespace EasyZoning
{
    using Colossal.IO.AssetDatabase; // FileLocation attribute (settings storage path)
    using Game.Input;                // ProxyBinding, BindingKeyboard, ActionType
    using Game.Modding;              // IMod, ModSetting base
    using Game.Settings;             // Settings UI attributes (tabs/groups/sections/buttons)
    using System;                    // Exception in URL open handlers
    using UnityEngine;               // Application.OpenURL

    // Persist settings under: .../ModsSettings/EasyZoning/EasyZoning
    [FileLocation("ModsSettings/EasyZoning/EasyZoning")]

    // Options UI structure.
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup, kLegacyGroup, kAboutInfoGroup, kAboutLinksGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup, kLegacyGroup)]

    // Declares the rebindable action that appears in Options → Key bindings.
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

        // --- Key bindings ---
        // ProxyBinding holds the saved/rebound key, stored by the settings system.
        // Default is Ctrl+V, declared by SettingsUIKeyboardBinding.
        [SettingsUIKeyboardBinding(BindingKeyboard.V, Mod.kToggleToolActionName, ctrl: true)]
        [SettingsUISection(kActionsTab, kKeybindingGroup)]
        public ProxyBinding ToggleZoneTool
        {
            get; set;
        }

        // --- Legacy Tool behavior ---

        // RMB cycling (update-existing-roads tool):
        // - OFF (default): Both → Left → Right → None → ...
        // - ON: Left <-> Right OR Both <-> None
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
            // ToggleZoneTool default binding from SettingsUIKeyboardBinding (Ctrl+V).
        }
    }
}
