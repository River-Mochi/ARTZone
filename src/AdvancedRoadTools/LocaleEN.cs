// File: src/AdvancedRoadTools/LocaleEN.cs
// C# locale for Options panel + tool strings

namespace AdvancedRoadTools
{
    using System.Collections.Generic;
    using System.Linq;
    using AdvancedRoadTools.Tools;
    using Colossal;
    using Game.Settings;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting _setting;

        public LocaleEN(Setting setting) => _setting = setting;

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                { _setting.GetSettingsLocaleID(), "Advanced Road Tools" },
                { _setting.GetOptionTabLocaleID(Setting.kSection), "Main" },
                { _setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "ZONE CONTROLLER TOOL OPTIONS" },

                { _setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { _setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)), "Protect existing zone cells during preview and apply. Default: On" },

                { _setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { _setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)), "Protect occupied cells to avoid unintended vacancy/abandonment. Default: On" },

                { _setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { _setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)), "Click to invert the current zoning configuration. Default: RMB. Use ❌ to unassign or 🔄 to reset." },

                // Tool palette strings (safe to keep on C# side)
                { $"Assets.NAME[{ZoningControllerToolSystem.ToolID}]", "Zone Controller" },
                { $"Assets.DESCRIPTION[{ZoningControllerToolSystem.ToolID}]", "Control road zoning: both sides, left, right, or none. Right-click toggles by default." },
            };

            return d.ToDictionary(p => p.Key, p => p.Value);
        }

        public void Unload()
        {
        }

        public override string ToString() => "LocaleEN (AdvancedRoadTools)";
    }
}
