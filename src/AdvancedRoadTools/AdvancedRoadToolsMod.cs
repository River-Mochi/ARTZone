using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using Colossal.IO.AssetDatabase;
using Game.Prefabs;
using Unity.Entities;
using UnityEngine;

namespace AdvancedRoadTools.Core;

public class AdvancedRoadToolsMod : IMod
{
    public static ILog log = LogManager.GetLogger($"{nameof(Core)}.{nameof(AdvancedRoadToolsMod)}")
        .SetShowsErrorsInUI(false);

    internal static Setting m_Setting;
    
    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Info(nameof(OnLoad));

        RegisterPrefab();

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
    }

    private void RegisterPrefab()
    {
        //World world = World;
       // PrefabSystem prefabSystem = world.GetOrCreateSystem<PrefabSystem>();
        var prefab = ScriptableObject.CreateInstance<ServicePrefab>();
        
        var uiObject = ScriptableObject.CreateInstance<UIObject>();
        
        
        //prefabSystem.AddComponentData(prefab, new UIObjectData{});
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