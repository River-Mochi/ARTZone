// File: src/Mod.cs
// Purpose: Mod entrypoint; registers settings, localization, ECS systems, keybindings, and locale-change hooks.
// Notes:
//   • Locales install before Options UI so labels render correctly.
//   • RMB flip stays vanilla (ToolBaseSystem.cancelAction) — no custom binding.
//   • Top-left button points at coui://ui-mods/images/* assets.

namespace EasyZoning
{
    using Colossal;                  // IDictionarySource (locale sources)
    using Colossal.IO.AssetDatabase; // AssetDatabase.LoadSettings
    using Colossal.Localization;     // LocalizationManager (locale sources + change hook)
    using Colossal.Logging;          // ILog, LogManager (mod log)
    using CS2HonuShared;             // LogUtils (safe logging + WarnOnce)
    using EasyZoning.Tools;          // ECS systems scheduled by UpdateSystem
    using Game;                      // UpdateSystem, SystemUpdatePhase
    using Game.Input;                // ProxyAction (Ctrl+Z action)
    using Game.Modding;              // IMod
    using Game.SceneFlow;            // GameManager (localization manager access)
    using System;                    // Exception, Func<T>, StringComparison
    using System.Reflection;         // Assembly (version number from csproj)


    public sealed class Mod : IMod
    {
        // ---- PUBLIC CONSTANTS / METADATA ----

        public const string ModName = "Easy Zoning";
        public const string ModID = "EasyZoning";
        public const string ModTag = "[EZ]";

        /// <summary>
        /// Version read from assembly (3-part), fallback to "1.0.0".
        /// </summary>
        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        // COUI base
        public const string UiCouiRoot = "coui://ui-mods";

        // Top-left floating action button (color)
        public const string MainIconPath = UiCouiRoot + "/images/ico-zones-color02.svg";

        // Rebindable action ID exposed in Options UI
        public const string kToggleToolActionName = "ToggleZoneTool";   // default CTRL+Z

        // ---- BUILD TAG / LOGGER ----

#if DEBUG
        private const string BuildTag = "[DEBUG]";
#else
        private const string BuildTag = "[RELEASE]";
#endif

        // create Mod specific log file.
        public static readonly ILog s_Log =
            LogManager.GetLogger(ModID).SetShowsErrorsInUI(
#if DEBUG
                true
#else
                false
#endif
            );

        // ---- GLOBAL SETTINGS / KEYBIND ----

        /// <summary>
        /// Global settings instance (Options UI).
        /// </summary>
        public static Setting? Settings
        {
            get; private set;
        }

        /// <summary>
        /// ProxyAction for the Easy Zoning tool toggle (default CTRL+Z).
        /// </summary>
        public static ProxyAction? ToggleToolAction
        {
            get; private set;
        }

        // ---- PRIVATE STATE ----

        private static bool s_BannerLogged;
        private static bool s_ReapplyingLocale;

#if DEBUG
        private static string? s_LastLocaleId;
#endif

        // ---- IMod IMPLEMENTATION ----

        public void OnLoad(UpdateSystem updateSystem)
        {
            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                LogSafe(( ) => $"{ModName} {ModTag} v{ModVersion} OnLoad {BuildTag}");
            }

            if (GameManager.instance == null)
            {
                WarnSafe(( ) => "GameManager.instance is null in Mod.OnLoad.");
                return;
            }

            Setting setting = new Setting(this);
            Settings = setting;

            // Localization sources
            AddLocaleSource("en-US", new LocaleEN(setting));
            AddLocaleSource("fr-FR", new LocaleFR(setting));
            AddLocaleSource("es-ES", new LocaleES(setting));
            AddLocaleSource("de-DE", new LocaleDE(setting));
            AddLocaleSource("it-IT", new LocaleIT(setting));
            AddLocaleSource("ja-JP", new LocaleJA(setting));
            AddLocaleSource("ko-KR", new LocaleKO(setting));
            AddLocaleSource("pl-PL", new LocalePL(setting));
            AddLocaleSource("pt-BR", new LocalePT_BR(setting));
            AddLocaleSource("zh-HANS", new LocaleZH_CN(setting));    // Simplified Chinese
            AddLocaleSource("zh-HANT", new LocaleZH_HANT(setting));  // Traditional Chinese

            // Settings + Options UI
            try
            {
                AssetDatabase.global.LoadSettings(ModID, setting, new Setting(this));
                setting.RegisterInOptionsUI();
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"Settings/UI init failed: {ex.GetType().Name}: {ex.Message}");
            }

            // Keybinding setup
            try
            {
                setting.RegisterKeyBindings();

                ToggleToolAction = setting.GetAction(kToggleToolActionName);
                if (ToggleToolAction != null)
                {
                    ToggleToolAction.shouldBeEnabled = true;
                }
                else
                {
                    WarnSafe(( ) => $"Keybinding action '{kToggleToolActionName}' not found.");
                }
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"Keybinding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // ECS systems
            try
            {
                // Tool logic: selection and apply.
                updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);

                // Visual highlight for blocks under the tool.
                updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);

                // New-road sync: apply zoning mode to blocks as roads are built.
                updateSystem.UpdateAt<SyncNewRoadsSystem>(SystemUpdatePhase.Modification4);

                // Existing-block sync: apply tool depths to zone blocks on existing roads.
                updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);

                // Cohtml UI bridge for the EZ panel.
                updateSystem.UpdateAt<ZoneControlBridgeUI>(SystemUpdatePhase.UIUpdate);

                // Hotkey system that listens to ToggleZoneTool (CTRL+Z).
                updateSystem.UpdateAt<KeybindHotkeySystem>(SystemUpdatePhase.ToolUpdate);
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"System scheduling/init failed: {ex.GetType().Name}: {ex.Message}");
            }

            // Locale-change hook: re-register Options UI when active dictionary changes.
            LocalizationManager? lm = GameManager.instance.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
                lm.onActiveDictionaryChanged += OnLocaleChanged;
            }
        }

        public void OnDispose( )
        {
            LogSafe(( ) => "OnDispose");

            LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
            }

            if (ToggleToolAction != null)
            {
                ToggleToolAction.shouldBeEnabled = false;
                ToggleToolAction = null;
            }

            if (Settings != null)
            {
                try
                {
                    Settings.UnregisterInOptionsUI();
                }
                catch (Exception ex)
                {
                    WarnSafe(( ) => $"UnregisterInOptionsUI failed: {ex.GetType().Name}: {ex.Message}");
                }

                Settings = null;
            }
        }

        // ---- LOCALE HANDLING ----

        private static void OnLocaleChanged( )
        {
            if (s_ReapplyingLocale)
                return;

            s_ReapplyingLocale = true;
            try
            {
                LocalizationManager? lm = GameManager.instance?.localizationManager;
                string id = lm?.activeLocaleId ?? "(unknown)";

#if DEBUG
                if (!string.Equals(id, s_LastLocaleId, StringComparison.Ordinal))
                {
                    LogSafe(( ) => "[EZ] Active locale = " + id);
                    s_LastLocaleId = id;
                }
#endif
                // Re-register Options UI so labels pull from the current dictionary.
                Settings?.RegisterInOptionsUI();
            }
            finally
            {
                s_ReapplyingLocale = false;
            }
        }

        private static void AddLocaleSource(string localeId, IDictionarySource source)
        {
            if (string.IsNullOrEmpty(localeId))
                return;

            LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                WarnSafe(( ) => $"AddLocaleSource: No LocalizationManager; cannot add source for '{localeId}'.");
                return;
            }

            try
            {
                lm.AddSource(localeId, source);
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"AddLocaleSource: AddSource for '{localeId}' failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // ---- LogUtils HELPERS ----

        public static void LogSafe(Func<string> messageFactory) =>
            LogUtils.TryLog(s_Log, Level.Info, messageFactory);

        public static void WarnSafe(Func<string> messageFactory) =>
            LogUtils.TryLog(s_Log, Level.Warn, messageFactory);

        public static void WarnOnce(string key, Func<string> messageFactory) =>
            LogUtils.WarnOnce(s_Log, key, messageFactory);
    }
}
