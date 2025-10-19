// File: src/ARTZone/LocaleEN.cs
// Built-in English strings for Options UI + tool palette text.

#nullable enable
namespace ARTZone
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting m_Setting)
        {
            this.m_Setting = m_Setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "ART-Zone" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Actions" },

                { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Zone Controller Tool Options" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kKeybindingGroup), "Key bindings" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being overridden during preview and apply phases." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being overridden during preview and apply phases." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.InvertZoning)), "Invert Zoning Mouse Button" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.InvertZoning)), "Bind a mouse button to invert zoning while the tool is active." },

                { m_Setting.GetBindingKeyLocaleID(ARTZoneMod.kInvertZoningActionName), "Invert Zoning" },

                // Palette/asset text
                { $"Assets.NAME[{Tools.ZoningControllerToolSystem.ToolID}]", "Zone Controller" },
                { $"Assets.DESCRIPTION[{Tools.ZoningControllerToolSystem.ToolID}]",
                  "Control how zoning behaves along a road.\nChoose zoning on both sides, only left or right, or none.\nRight-click (by default) inverts the configuration." },


                 // About tab fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NameText)), "Mod Name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.NameText)), "Display name of this mod." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)), "Current mod version." },

#if DEBUG
                // Only visible in DEBUG builds
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.InformationalVersionText)), "Informational Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.InformationalVersionText)), "Mod Version with Commit ID" },
#endif
                 // About tab links (external link buttons)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadoxModsButton)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadoxModsButton)),  "Paradox Mods website; opens in browser." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenDiscordButton)), "Discord" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenDiscordButton)),  "Discord chat for feedback on the mod; opens in browser." },



            };
        }

        public void Unload()
        {
        }
    }
}
