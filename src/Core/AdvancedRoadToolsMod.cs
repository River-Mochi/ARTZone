using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Colossal.IO.AssetDatabase;
using Game.UI.InGame;
using Game.Zones;
using HarmonyLib;
using Unity.Collections;
using Unity.Entities;

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

        
        updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
        updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
        updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
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
public class DevUIListComponents {

    [HarmonyPatch(typeof(DeveloperInfoUISystem), "OnCreate")]
    [HarmonyPostfix]
    public static void AddComponentsList(ref DeveloperInfoUISystem __instance, ref SelectedInfoUISystem ___m_InfoUISystem) {  
        var em = __instance.EntityManager;   
        var updateInfoMethod = (Entity entity, Entity prefab, InfoList info) => {
            info.label = "Instance ECS Components";
            NativeArray<ComponentType> arr = em.GetChunk(entity).Archetype.GetComponentTypes(Allocator.Temp);
            for (int i = 0; i < arr.Length; i++) {
                var ct = arr[i];
                info.Add(new InfoList.Item(ct.GetManagedType().FullName));
            }
        };    
        ___m_InfoUISystem.AddDeveloperInfo(new InfoList( 
            (entity1, entity2) => true, updateInfoMethod
        ));
    }
}