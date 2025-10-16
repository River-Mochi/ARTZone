// File: src/AdvancedRoadTools/Setting.cs
// Mod settings + keybinding (mouse). Groups ordered: Toggles first, Keybinding second.

#nullable enable

namespace AdvancedRoadTools
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;

    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)]

    // Define the action used by the binding row (button action on the mouse device)
    [SettingsUIMouseAction(AdvancedRoadToolsMod.kInvertZoningActionName, ActionType.Button,
        usages: new string[] { "Zone Controller Tool" })]
    public sealed class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Keybinding";

        public Setting(IMod mod) : base(mod) { }

        // ----- Toggles (shown first) -----
        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // ----- Mouse binding (shown second) -----
        // Default = RMB; UI lets user bind ANY mouse button (including LMB/MMB/Forward/Back).
        [SettingsUIMouseBinding(BindingMouse.Right, AdvancedRoadToolsMod.kInvertZoningActionName)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        public override void SetDefaults()
        {
            RemoveOccupiedCells = true;
            RemoveZonedCells = true;
            // Binding default handled by attribute; nothing to do here.
        }
    }
}
