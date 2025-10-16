// Setting.cs
// Purpose: Options UI + Input binding (RMB) using Colossal's keybinding template (ProxyBinding/ProxyAction)

#nullable enable

namespace AdvancedRoadTools
{
    using System.Collections.Generic;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Game.UI.Widgets;

    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]
    [SettingsUIGroupOrder(kKeybindingGroup, kToggleGroup)]
    [SettingsUIShowGroupName(kKeybindingGroup, kToggleGroup)]

    // Declare the action once (Button action) and expose it under our tool usage
    [SettingsUIMouseAction(AdvancedRoadToolsMod.kInvertZoningActionName,
        ActionType.Button,
        usages: new[] { Tools.ZoningControllerToolSystem.ToolID })]
    public sealed class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kKeybindingGroup = "KeyBinding";
        public const string kToggleGroup = "Zone Controller Tool";

        public Setting(IMod mod) : base(mod) { }

        // === Key binding (RMB default) ===
        // This is the UI row that shows current binding and provides Unassign/Reset UI affordances.
        // Binding defines the default (Right Mouse); the Options UI lets players clear/reset it.
        [SettingsUISection(kSection, kKeybindingGroup)]
        [SettingsUIMouseBinding(BindingMouse.Right, AdvancedRoadToolsMod.kInvertZoningActionName)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        // Optional: an explicit "Reset" row to put the binding back to RMB programmatically.
        [SettingsUIButton]
        [SettingsUISection(kSection, kKeybindingGroup)]
        public bool ResetInvertBinding
        {
            set
            {
                ResetKeyBindings(); // Colossal template API — resets all bindings for this setting class
                AdvancedRoadToolsMod.s_Log.Info("[ART] Reset key bindings to defaults (Invert → RMB).");
            }
        }

        // === Tool toggles ===
        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        public override void SetDefaults()
        {
            // Defaults for non-binding options (bindings use attribute defaults)
            RemoveZonedCells = true;
            RemoveOccupiedCells = true;
        }

        // Locale keys are provided by LocaleEN.cs
        // Example keys used there:
        // GetSettingsLocaleID()
        // GetOptionTabLocaleID(kSection)
        // GetOptionGroupLocaleID(kKeybindingGroup)
        // GetOptionGroupLocaleID(kToggleGroup)
        // GetOptionLabelLocaleID(nameof(InvertZoning))
        // GetOptionDescLocaleID(nameof(InvertZoning))
        // GetOptionLabelLocaleID(nameof(ResetInvertBinding))
        // GetOptionDescLocaleID(nameof(ResetInvertBinding))
        // GetOptionLabelLocaleID(nameof(RemoveZonedCells))
        // GetOptionDescLocaleID(nameof(RemoveZonedCells))
        // GetOptionLabelLocaleID(nameof(RemoveOccupiedCells))
        // GetOptionDescLocaleID(nameof(RemoveOccupiedCells))
        // GetBindingKeyLocaleID(AdvancedRoadToolsMod.kInvertZoningActionName)
    }
}
