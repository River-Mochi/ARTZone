using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvancedRoadTools.Logging;
using AdvancedRoadTools.Tools;
using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Environment;
using Colossal.Serialization.Entities;
using Game;
using Game.Input;
using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;
using Newtonsoft.Json;
using UnityEngine;

namespace AdvancedRoadTools;

public class AdvancedRoadToolsMod : IMod
{
    public const string ModID = "AdvancedRoadTools";


    public static Setting m_Setting;
    public const string kInvertZoningActionName = "InvertZoning";
    public static ProxyAction m_InvertZoningAction;

    public void OnLoad(UpdateSystem updateSystem)
    {
        log.Debug($"{nameof(AdvancedRoadToolsMod)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

        RegisterPrefab();

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            log.Info($"Current mod asset at {asset.path}");
        m_Setting = new Setting(this);
        m_Setting.RegisterInOptionsUI();
        
        AddSources();
        m_Setting.RegisterKeyBindings();

        m_InvertZoningAction = m_Setting.GetAction(kInvertZoningActionName);
        GameManager.instance.localizationManager.onActiveDictionaryChanged +=
            () => log.Info($"Active directory is now {GameManager.instance.localizationManager.activeLocaleId}");

        AssetDatabase.global.LoadSettings(nameof(AdvancedRoadTools), m_Setting, new Setting(this));


        updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
        updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);

        updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);

        updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);

        updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);

        GameManager.instance.onGamePreload += CreateTools;
    }

    private void AddSources()
    {
        log.Info($"Loading locales");
        var modPath = string.Empty;
        foreach (var path in Directory.GetDirectories(Path.Combine(EnvPath.kCacheDataPath, "Mods", "mods_subscribed")))
        {
            if (!path.Contains("102147")) continue;

            modPath = path;
            break;
        }

        if (string.IsNullOrEmpty(modPath))
        {
            log.Error($"Mod's folder couldn't be localized!");
        }
        var langPath = Path.Combine(modPath, "lang");

        if (!Directory.Exists(langPath))
        {
            log.Error($"lang folder not found under mod's directory.");
            return;
        }

        foreach (var path in Directory.GetFiles(langPath, "*.json"))
        {
            var localeID = Path.GetFileNameWithoutExtension(path);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));

            var locale = new Locale(localeID, m_Setting);
            locale.Entries = dict;

            GameManager.instance.localizationManager.AddSource(localeID, locale);
            log.Info($"\tLoaded locale {localeID}.json");
        }
        log.Info($"Finished loading locales");
    }

    private void CreateTools(Purpose purpose, GameMode mode)
    {
        ToolsHelper.InstantiateTools();
        GameManager.instance.onGamePreload -= CreateTools;
    }

    private void RegisterPrefab()
    {
        //World world = World;
        //PrefabSystem prefabSystem = world.GetOrCreateSystem<PrefabSystem>();
        var prefab = ScriptableObject.CreateInstance<ServicePrefab>();
        var uiObject = ScriptableObject.CreateInstance<UIObject>();


        //prefabSystem.AddComponentData(prefab, new UIObjectData{});
    }


    public void OnDispose()
    {
        log.Debug($"{nameof(AdvancedRoadToolsMod)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");
        if (m_Setting != null)
        {
            m_Setting.UnregisterInOptionsUI();
            m_Setting = null;
        }
    }
}