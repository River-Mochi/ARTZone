// File: src/AdvancedRoadTools/Setting.cs
// Options UI + keybinding definition (mouse). Toggles shown first, keybinding below.

#nullable enable
namespace AdvancedRoadTools
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;

    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]
    // Show toggles group first, then the keybinding group
    [SettingsUIGroupOrder(kToggleGroup, kKeybindingGroup)]
    [SettingsUIShowGroupName(kToggleGroup, kKeybindingGroup)]

    // Define the input action once. (Mouse action; Button type)
    [SettingsUIMouseAction(AdvancedRoadToolsMod.kInvertZoningActionName,
                           ActionType.Button,
                           usages: new[] { "Zone Controller Tool" })]
    public sealed class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kKeybindingGroup = "Key bindings";

        public Setting(IMod mod) : base(mod) { }

        // === Toggles (top group) ===
        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // === Keybinding (bottom group) ===
        // Default to RMB; user can unassign or rebind to other mouse buttons.
        [SettingsUIMouseBinding(BindingMouse.Right, AdvancedRoadToolsMod.kInvertZoningActionName)]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }
    }
}
