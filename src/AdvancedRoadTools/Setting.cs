// File: src/AdvancedRoadTools/Setting.cs
// Purpose: Mod settings (keybind uses keybinding widget with Unassign/Reset)

namespace AdvancedRoadTools
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Game.UI.Widgets;

    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]
    [SettingsUIGroupOrder(kToggleGroup)]
    [SettingsUIShowGroupName(kToggleGroup)]
    // Register the input action under our tool usage so it appears in Options
    [SettingsUIMouseAction(
        AdvancedRoadToolsMod.kInvertZoningActionName,
        ActionType.Button,
        usages: new[] { Tools.ZoningControllerToolSystem.ToolID })]
    public sealed class Setting : ModSetting
    {
        // Tabs/Groups
        public const string kSection = "Main";
        public const string kToggleGroup = "Zone Controller Tool";

        public Setting(IMod mod) : base(mod) { }

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // IMPORTANT: Use the *keybinding widget* (InputBinding + SettingsUIKeyBinding)
        // to get the ❌ Unassign and 🔄 Reset buttons. Default is RMB.
        [SettingsUISection(kSection, kToggleGroup)]
        [SettingsUIKeyBinding(
            actionName: AdvancedRoadToolsMod.kInvertZoningActionName,
            defaultBinding: "<Mouse>/rightButton",
            allowUnassign: true,
            showReset: true)]
        public InputBinding InvertZoning { get; set; } = InputBinding.From("<Mouse>/rightButton");

        public override void SetDefaults()
        {
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
            InvertZoning = InputBinding.From("<Mouse>/rightButton");
        }
    }
}
