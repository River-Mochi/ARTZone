using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Colossal.IO.AssetDatabase;
using Game.Zones;
using HarmonyLib;

namespace AdvancedRoadTools.Core;

public class AdvancedRoadToolsMod : IMod
{
    public static ILog log = LogManager.GetLogger($"{nameof(Core)}.{nameof(AdvancedRoadToolsMod)}")
        .SetShowsErrorsInUI(false);

    internal static Setting m_Setting;

    private Harmony _harmony;
    
    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info(nameof(OnLoad));

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            log.Info($"Current mod asset at {asset.path}");

        m_Setting = new Setting(this);
        m_Setting.RegisterInOptionsUI();
        GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

        AssetDatabase.global.LoadSettings(nameof(Core), m_Setting, new Setting(this));

        updateSystem.UpdateAt<InitializeAdvancedDataSystem>(SystemUpdatePhase.Modification4B);
        updateSystem.UpdateAfter<UpdateRoadTilingSystem>(SystemUpdatePhase.Modification4);
        updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
        updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
        
        _harmony = new Harmony("AdvancedRoadTools");
        _harmony.PatchAll();
    }


    public void OnDispose()
    {
        log.Info(nameof(OnDispose));
        if (m_Setting != null)
        {
            m_Setting.UnregisterInOptionsUI();
            m_Setting = null;
        }
    }
}

[HarmonyPatch]
public static class Patches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(ZoneUtils), nameof(ZoneUtils.GetCellWidth))]
    public static void ModifyCellWidth(ref int __result,float roadWidth)
    {
        AdvancedRoadToolsMod.log.Info($"[{nameof(ZoneUtils)}:{nameof(ZoneUtils.GetCellWidth)}] Called");
        //__result -= 10;
    }
}