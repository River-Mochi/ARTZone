// File: src/AdvancedRoadTools/AdvancedRoadToolsMod.cs
// Purpose: Mod entrypoint + phase-1 (zoning-only) bootstrap and robust locale loading.

namespace AdvancedRoadTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AdvancedRoadTools.Logging;
    using AdvancedRoadTools.Tools;
    using Colossal.IO.AssetDatabase;
    using Colossal.Serialization.Entities; // Purpose
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;
    using Newtonsoft.Json;

    public class AdvancedRoadToolsMod : IMod
    {
        public const string ModID = "AdvancedRoadTools";

        public static Setting m_Setting;
        public const string kInvertZoningActionName = "InvertZoning";
        public static ProxyAction m_InvertZoningAction;

        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Debug($"{nameof(AdvancedRoadToolsMod)}.{System.Reflection.MethodBase.GetCurrentMethod()?.Name}");

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();

            AddSources(); // robust locale loader (lang/*.json next to DLL)

            // NOTE: no custom RegisterKeyBindings helper is present; attributes handle bindings.
            m_InvertZoningAction = m_Setting.GetAction(kInvertZoningActionName);

            AssetDatabase.global.LoadSettings(nameof(AdvancedRoadTools), m_Setting, new Setting(this));

            // Phase-1: zoning-only systems
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);

            GameManager.instance.localizationManager.onActiveDictionaryChanged +=
                () => log.Info($"Active locale is now {GameManager.instance.localizationManager.activeLocaleId}");

            // Defer tool prefab creation until preload, then unsubscribe.
            GameManager.instance.onGamePreload += CreateTools;
        }

        private void CreateTools(Purpose purpose, GameMode mode)
        {
            try
            {
                ToolsHelper.InstantiateTools();
            }
            finally
            {
                GameManager.instance.onGamePreload -= CreateTools;
            }
        }

        private void AddSources()
        {
            log.Info("Loading locales…");

            string modDir = null;
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                // e.g. …\Mods\AdvancedRoadTools\AdvancedRoadTools.dll
                modDir = Path.GetDirectoryName(asset.path);
            }

            if (string.IsNullOrEmpty(modDir))
            {
                log.Warn("Mod asset path not found; registering built-in English only.");
                var fallback = new Locale("en-US", m_Setting);
                GameManager.instance.localizationManager.AddSource("en-US", fallback);
                return;
            }

            var langDir = Path.Combine(modDir, "lang");
            if (!Directory.Exists(langDir))
            {
                log.Warn("No lang/ directory next to DLL; registering built-in English only.");
                var fallback = new Locale("en-US", m_Setting);
                GameManager.instance.localizationManager.AddSource("en-US", fallback);
                return;
            }

            foreach (var path in Directory.GetFiles(langDir, "*.json"))
            {
                try
                {
                    var id = Path.GetFileNameWithoutExtension(path);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path))
                               ?? new Dictionary<string, string>();
                    var src = new Locale(id, m_Setting) { Entries = dict };
                    GameManager.instance.localizationManager.AddSource(id, src);
                    log.Info($"\tLoaded locale {id} ({dict.Count} entries).");
                }
                catch (Exception ex)
                {
                    log.Error($"Failed loading locale from {path}: {ex.Message}");
                }
            }

            log.Info("Finished loading locales");
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
}
