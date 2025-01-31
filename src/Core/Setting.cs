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
[SettingsUIGroupOrder(kTillingGroup)]
[SettingsUIShowGroupName(kTillingGroup)]
public class Setting : ModSetting
{
    public const string kSection = "Main";

    public const string kTillingGroup = "Zone Controller Tool";

    public Setting(IMod mod) : base(mod)
    {
    }


    [SettingsUISlider(min = 1, max = 24, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
    [SettingsUISection(kSection, kTillingGroup)]
    public int MaxDepth { get; set; } = 6;

    public override void SetDefaults()
    {
        throw new System.NotImplementedException();
    }
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

            { m_Setting.GetOptionGroupLocaleID(Setting.kTillingGroup), "Zone Controller Tool" },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MaxDepth)), "Max Depth (in cells)" },

        };
    }

    public void Unload()
    {
    }
}