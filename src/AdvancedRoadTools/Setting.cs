// Setting.cs
// Mod settings + fallback locale

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

    // NOTE: you said you want this exact location
    [FileLocation("ModsSettings/AdvancedRoadTools/AdvancedRoadTools")]
    [SettingsUIGroupOrder(kToggleGroup)]
    [SettingsUIShowGroupName(kToggleGroup)]
    [SettingsUIMouseAction(AdvancedRoadToolsMod.kInvertZoningActionName, ActionType.Button,
        usages: new string[] { "Zone Controller Tool" })] // C# 9 (no collection expressions)
    public class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kToggleGroup = "Zone Controller Tool";
        public const string kInvertZoningAction = "InvertZoning";

        public Setting(IMod mod) : base(mod) { }

        [SettingsUISection(kSection, kToggleGroup)]
        public bool RemoveZonedCells { get; set; } = true;

        [SettingsUISection(kSection, kToggleGroup)]
        // [SettingsUIDisableByCondition(typeof(Setting), nameof(IfRemoveZonedCells))]
        public bool RemoveOccupiedCells { get; set; } = true;

        [SettingsUIMouseBinding(BindingMouse.Right, kInvertZoningAction)]
        [SettingsUISection(kSection, kToggleGroup)]
        public ProxyBinding InvertZoning
        {
            get; set;
        }

        public override void SetDefaults()
        {
            // UI toggles
            RemoveOccupiedCells = true;
            RemoveZonedCells = true;
        }

        private bool IfRemoveZonedCells() => !RemoveZonedCells;
    }

    public class Locale : IDictionarySource
    {
        public readonly string LocaleID;
        private readonly Setting setting;

        // Initialize to avoid nullable warnings and simplify usage
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
            {
                // clone to be safe
                return Entries.ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            // Fallback built-in English strings
            return new Dictionary<string, string>
            {
                { setting.GetSettingsLocaleID(), "Advanced Road Tools" },
                { setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Zone Controller Tool Options" },

                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                {
                    setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)),
                    "Prevent zoned cells from being overriden during preview and set phase of Zone Controller Tool." +
                    "\nSet this to true if you're having problem with losing your zoning configuration when using the tool." +
                    "\nDefault: true"
                },

                { setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                {
                    setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)),
                    "Prevent occupied cells from being overriden during preview and set phase of Zone Controller Tool." +
                    "\nSet this to true if you're having problem with buildings becoming vacant and/or abandoned when using the tool." +
                    "\nDefault: true"
                },

                { setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)), "Inverts the current zoning configuration with a mouse action." },

                { "Assets.NAME[" + ZoningControllerToolSystem.ToolID + "]", "Zone Controller" },
                {
                    "Assets.DESCRIPTION[" + ZoningControllerToolSystem.ToolID + "]",
                    "Tool to control how the zoning of a road behaves.\nChoose between zoning on both sides, only on the left or right, or no zoning for that road.\nBy default, right-click inverts the zoning configuration."
                }
            };
        }

        public void Unload()
        {
        }
    }
}
