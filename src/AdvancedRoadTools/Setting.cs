using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System.Collections.Generic;
using Game.Input;

namespace AdvancedRoadTools.Core;

[FileLocation(nameof(Core))]
[SettingsUIGroupOrder(kToggleGroup)]
[SettingsUIShowGroupName(kToggleGroup)]
public class Setting : ModSetting
{
    public const string kSection = "Main";

    public const string kToggleGroup = "Zone Controller Tool";

    public Setting(IMod mod) : base(mod)
    {
    }

    [SettingsUISection(kSection, kToggleGroup)]
    public bool RemoveZonedCells { get; set; } = true;

    [SettingsUISection(kSection, kToggleGroup)]
    [SettingsUIDisableByCondition(typeof(Setting), nameof(IfRemoveZonedCells))]
    public bool RemoveOccupiedCells { get; set; } = true;

    public override void SetDefaults()
    {
        throw new System.NotImplementedException();
    }

    private bool IfRemoveZonedCells() => !RemoveZonedCells;
}

public class LocaleEN : IDictionarySource
{
    private readonly Setting m_Setting;

    public LocaleEN(Setting setting)
    {
        m_Setting = setting;
    }

    public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors,
        Dictionary<string, int> indexCounts)
    {
        return new Dictionary<string, string>
        {
            { m_Setting.GetSettingsLocaleID(), "Advanced Road Tools" },
            { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

            { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Zone Controller Tool" },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveZonedCells)), "Prevent zoned cells from being removed" },
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.RemoveOccupiedCells)), "Prevent occupied cells from being removed" },

        };
    }

    public void Unload()
    {
    }
}