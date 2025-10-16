// src/AdvancedRoadTools/AdvancedRoadToolsMod.cs
// Purpose: Mod entrypoint + phase-1 (zoning-only) bootstrap and robust locale loading.

namespace AdvancedRoadTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using AdvancedRoadTools.Tools;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Colossal.Serialization.Entities; // Purpose
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;

    public class AdvancedRoadToolsMod : IMod
    {
        public const string ModID = "AdvancedRoadTools";

        // Initialize with null! to satisfy CS8618; set for real in OnLoad.
        public static Setting m_Setting = null!;
        public const string kInvertZoningActionName = "InvertZoning";
        public static ProxyAction m_InvertZoningAction = default!;

        // One shared logger for the entire mod:
        public static readonly ILog s_Log =
            LogManager.GetLogger("AdvancedRoadTools").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Debug($"{nameof(AdvancedRoadToolsMod)}.{nameof(OnLoad)}");

            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();

            AddSources(); // robust locale loader (lang/*.json next to DLL)

            // Load saved settings (or defaults) first
            AssetDatabase.global.LoadSettings(ModID, m_Setting, new Setting(this));

            // keybinds: actions come from Settings
            m_InvertZoningAction = m_Setting.GetAction(kInvertZoningActionName);

            // Phase-1: zoning-only systems
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);

            GameManager.instance.localizationManager.onActiveDictionaryChanged +=
                () => s_Log.Info("Active locale: " + GameManager.instance.localizationManager.activeLocaleId);

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
            s_Log.Info("Loading locales…");

            string? modDir = null; // nullable to avoid CS8600
            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out ExecutableAsset? asset))
            {
                // e.g. …\Mods\AdvancedRoadTools\AdvancedRoadTools.dll
                modDir = Path.GetDirectoryName(asset.path);
            }

            if (string.IsNullOrEmpty(modDir))
            {
                s_Log.Warn("Mod asset path not found; registering built-in English only.");
                Locale fallback = new Locale("en-US", m_Setting);
                GameManager.instance.localizationManager.AddSource("en-US", fallback);
                return;
            }

            string langDir = Path.Combine(modDir, "lang");
            if (!Directory.Exists(langDir))
            {
                s_Log.Warn("No lang/ directory next to DLL; registering built-in English only.");
                Locale fallback = new Locale("en-US", m_Setting);
                GameManager.instance.localizationManager.AddSource("en-US", fallback);
                return;
            }

            foreach (string path in Directory.GetFiles(langDir, "*.json"))
            {
                try
                {
                    string id = Path.GetFileNameWithoutExtension(path);
                    string json = File.ReadAllText(path);

                    // Minimal parse of a flat { "key":"value", "n":123 } object.
                    Dictionary<string, string> dict = SimpleJsonDict.ParseFlatObject(json);

                    Locale src = new Locale(id, m_Setting) { Entries = dict };
                    GameManager.instance.localizationManager.AddSource(id, src);
                    s_Log.Info("\tLoaded locale " + id + " (" + dict.Count + " entries).");
                }
                catch (Exception ex)
                {
                    s_Log.Error("Failed loading locale from " + path + ": " + ex.Message);
                }
            }

            s_Log.Info("Finished loading locales");
        }

        public void OnDispose()
        {
            s_Log.Debug($"{nameof(AdvancedRoadToolsMod)}.{nameof(OnDispose)}");
            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null!; // keep static non-nullable, but allow GC here without CS8625
            }
        }

        /// <summary>
        /// Minimal flat JSON parser for {"key":"value","n":123,"b":true}. No arrays/nesting.
        /// Good enough for simple locale files without pulling Newtonsoft.
        /// </summary>
        private static class SimpleJsonDict
        {
            public static Dictionary<string, string> ParseFlatObject(string json)
            {
                Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
                if (string.IsNullOrWhiteSpace(json))
                    return result;

                string s = json.Trim();
                if (s.Length >= 2 && s[0] == '{' && s[s.Length - 1] == '}')
                    s = s.Substring(1, s.Length - 2);

                List<string> parts = SplitTopLevel(s);
                for (int i = 0; i < parts.Count; i++)
                {
                    string part = parts[i];
                    int idx = part.IndexOf(':');
                    if (idx <= 0)
                        continue;

                    string k = Unquote(part.Substring(0, idx).Trim());
                    string v = part.Substring(idx + 1).Trim();
                    result[k] = Unquote(v);
                }
                return result;
            }

            private static string Unquote(string s)
            {
                if (s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"')
                    s = s.Substring(1, s.Length - 2).Replace("\\\"", "\"").Replace("\\\\", "\\");
                return s; // numbers/bools pass through as text
            }

            private static List<string> SplitTopLevel(string s)
            {
                List<string> list = new List<string>();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                bool inString = false;
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == '"' && (i == 0 || s[i - 1] != '\\'))
                        inString = !inString;

                    if (c == ',' && !inString)
                    {
                        list.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                if (sb.Length > 0)
                    list.Add(sb.ToString());
                return list;
            }
        }
    }
}
