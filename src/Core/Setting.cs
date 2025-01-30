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
[SettingsUIGroupOrder(kSizeGroup, kTillingGroup, kOffsetGroup)]
[SettingsUIShowGroupName(kSizeGroup, kTillingGroup, kOffsetGroup)]
public class Setting : ModSetting
{
    public const string kSection = "Main";

    public const string kSizeGroup = "Size";
    public const string kTillingGroup = "Tilling Options";
    public const string kOffsetGroup = "Tilling Offset";

    public Setting(IMod mod) : base(mod)
    {
    }

    [SettingsUISection(kSection, kSizeGroup)]
    public bool SizeXActive { get; set; }
    [SettingsUISlider(min = 0, max = 6, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
    [SettingsUISection(kSection, kSizeGroup)]
    public int SizeX { get; set; }
    [SettingsUISection(kSection, kSizeGroup)]
    
    public bool SizeYActive { get; set; }
    [SettingsUISlider(min = 0, max = 6, step = 1, scalarMultiplier = 1, unit = Unit.kInteger)]
    [SettingsUISection(kSection, kSizeGroup)]
    public int SizeY { get; set; }

    [SettingsUISection(kSection, kTillingGroup)]
    public TillingModes TillingMode { get; set; } = TillingModes.Both;

    [SettingsUISlider(min = -10, max = 10, step = .5f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
    [SettingsUISection(kSection, kOffsetGroup)]
    public float OffsetX { get; set; }
    [SettingsUISlider(min = -10, max = 10, step = .5f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
    [SettingsUISection(kSection, kOffsetGroup)]
    public float OffsetY { get; set; }    
    [SettingsUISlider(min = -10, max = 10, step = .5f, scalarMultiplier = 1, unit = Unit.kFloatSingleFraction)]
    [SettingsUISection(kSection, kOffsetGroup)]
    public float OffsetZ { get; set; }
    public enum TillingModes
    {
        Both,
        Left,
        Right,
        None
    }

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

            { m_Setting.GetOptionGroupLocaleID(Setting.kSizeGroup), "Size" },
            { m_Setting.GetOptionGroupLocaleID(Setting.kTillingGroup), "Tilling Options" },
            { m_Setting.GetOptionGroupLocaleID(Setting.kOffsetGroup), "Tilling Offset" },

            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SizeXActive)), "Should modify width cell count?" },
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SizeX)), "Cell count X" },
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SizeYActive)), "Should modify depth cell count?" },
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.SizeY)), "Cell count Y" },
            
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TillingMode)), "Default Tilling Mode" },
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OffsetX)), "Offset X" },
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OffsetY)), "Offset Y" },
            { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OffsetZ)), "Offset Z" },
        };
    }

    public void Unload()
    {
    }
}