// File: src/AdvancedRoadTools/Setting.cs
// Mod settings + fallback locale (RMB keybind uses the keybinding widget to get Unassign/Reset)

namespace AdvancedRoadTools
{
    using System.Collections.Generic;
    using System.Linq;
    using AdvancedRoadTools.Tools;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using Game.UI.Widgets;

    // NOTE: per your preference
    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]
    [SettingsUIGroupOrder(kToggleGroup)]
    [SettingsUIShowGroupName(kToggleGroup)]
    // Registers the action in Options for the tool area (don’t remove)
    [SettingsUIMouseAction(AdvancedRoadToolsMod.kInvertZoningActionName, ActionType.Button,
        usages: new string[] { "Zone Controller Tool" })]
    public class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kInvertZoningAction = "InvertZoning";

        public Setting(IMod mod) : base(mod) { }

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveOccupiedCells { get; set; } = true;

        // IMPORTANT:
        // Use the *keybinding widget* so the ❌ Unassign and 🔄 Reset buttons appear.
        // Keep the action name aligned with your Mod.cs registration.
        // Default = RMB (“<Mouse>/rightButton”)
        [SettingsUISection(kSection, kToggleGroup)]
        [SettingsUIKeyBinding(
            actionName: kInvertZoningAction,
            defaultBinding: "<Mouse>/rightButton",
            allowUnassign: true,           // shows the ❌
            showReset: true                // shows the 🔄
        )]
        public InputBinding InvertZoning { get; set; } = InputBinding.From("<Mouse>/rightButton");

        public override void SetDefaults()
        {
            RemoveOccupiedCells = true;
            RemoveZonedCells = true;
            InvertZoning = InputBinding.From("<Mouse>/rightButton");
        }

        private bool IfRemoveZonedCells() => !RemoveZonedCells;
    }

    public class Locale : IDictionarySource
    {
        public readonly string LocaleID;
        private readonly Setting setting;

        public Dictionary<string, string> Entries = new Dictionary<string, string>();

        public Locale(string localeID, Setting setting)
        {
            LocaleID = localeID;
            this.setting = setting;
        }

        public override string ToString() =>
            "[ART.Locale] " + LocaleID + "; Entries: " + (Entries == null ? "null" : Entries.Count.ToString());

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            if (Entries != null && Entries.Count > 0)
                return Entries.ToDictionary(pair => pair.Key, pair => pair.Value);

            // Fallback English
            return new Dictionary<string, string>
            {
                { setting.GetSettingsLocaleID(), "Advanced Road Tools" },
                { setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "ZONE CONTROLLER TOOL OPTIONS" },

                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                {
                    setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Prevent zoned cells from being overridden during preview and set phases.\nDefault: true"
                },

                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                {
                    setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "Prevent occupied cells from being overridden during preview and set phases.\nDefault: true"
                },

                { setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)), "Click to invert the current zoning configuration (default: RMB). Use ❌ to unassign or 🔄 to reset." },

                { "Assets.NAME[" + ZoningControllerToolSystem.ToolID + "]", "Zone Controller" },
                {
                    "Assets.DESCRIPTION[" + ZoningControllerToolSystem.ToolID + "]",
                    "Control road zoning: both sides, left, right, or none. Default right-click inverts the zoning configuration."
                }
            };
        }

        public void Unload()
        {
        }
    }
}
