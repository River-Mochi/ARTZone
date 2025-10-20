// File: src/ARTZoneMod.cs
// Purpose: Mod entrypoint; settings + locales; keybinding hookup.

namespace ARTZone
{
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;

    public sealed class ARTZoneMod : IMod
    {
        // ---- Meta ----
        public const string ModID = "ARTZone";
        public const string Name = "ART-Zone";
        public const string VersionShort = "1.0.0";
#if DEBUG
        public const string InformationalVersion = VersionShort + " (DEBUG)";
#else
        public const string InformationalVersion = VersionShort;
#endif

        // ---- Actions (must match Setting.cs attributes) ----
        public const string kInvertZoningActionName = "InvertZoning";
        public const string kToggleToolActionName = "ToggleZoneTool";

        // ---- Settings instance ----
        public static Setting? s_Settings
        {
            get; private set;
        }

        // ---- Runtime input actions ----
        public static ProxyAction? m_InvertZoningAction
        {
            get; private set;
        }
        public static ProxyAction? m_ToggleToolAction
        {
            get; private set;
        }

        // ---- Logging ----
        public static readonly ILog s_Log =
            LogManager.GetLogger("ART-Zone").SetShowsErrorsInUI(false);

        private static bool s_BannerLogged;

        public void OnLoad(UpdateSystem updateSystem)
        {
            if (!s_BannerLogged)
            {
                s_Log.Info($"{Name} v{VersionShort} - OnLoad");
                s_BannerLogged = true;
            }

            // Settings must exist before locales (so labels resolve)
            var settings = new Setting(this);
            s_Settings = settings;

            // Locales BEFORE Options UI registration
            AddLocale("en-US", new LocaleEN(settings));
            AddLocale("fr-FR", new LocaleFR(settings));
            AddLocale("de-DE", new LocaleDE(settings));
            AddLocale("es-ES", new LocaleES(settings));
            AddLocale("zh-HANS", new LocaleZH_CN(settings));   // Simplified Chinese
            AddLocale("zh-HANT", new LocaleZH_HANT(settings)); // Traditional Chinese

            // Load saved values (or defaults), then show Settings UI
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Key bindings (register + get runtime handles)
            try
            {
                settings.RegisterKeyBindings();

                m_InvertZoningAction = settings.GetAction(kInvertZoningActionName);
                if (m_InvertZoningAction != null)
                    m_InvertZoningAction.shouldBeEnabled = true;

                m_ToggleToolAction = settings.GetAction(kToggleToolActionName);
                if (m_ToggleToolAction != null)
                    m_ToggleToolAction.shouldBeEnabled = true;
            }
            catch (System.Exception ex)
            {
                s_Log.Warn($"[ART] Keybinding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // Systems (order matters)
            updateSystem.UpdateAt<Tools.ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            // (Removed: Tools.ToolHighlightSystem — not present in your project)
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<Tools.ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<Tools.ToolBootstrapSystem>(SystemUpdatePhase.UIUpdate);

            // Log locale changes (don’t remove sources on dispose)
            Colossal.Localization.LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
                lm.onActiveDictionaryChanged += OnLocaleChanged;
            }
        }

        public void OnDispose()
        {
            s_Log.Info($"{Name} - OnDispose");

            Colossal.Localization.LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm != null)
                lm.onActiveDictionaryChanged -= OnLocaleChanged;

            if (m_InvertZoningAction != null)
            {
                m_InvertZoningAction.shouldBeEnabled = false;
                m_InvertZoningAction = null;
            }

            if (m_ToggleToolAction != null)
            {
                m_ToggleToolAction.shouldBeEnabled = false;
                m_ToggleToolAction = null;
            }

            if (s_Settings != null)
            {
                s_Settings.UnregisterInOptionsUI();
                s_Settings = null;
            }
        }

        // ---- Helpers ----

        private static void AddLocale(string localeId, IDictionarySource source)
        {
            Colossal.Localization.LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                s_Log.Warn($"[ART] No LocalizationManager; cannot add locale {localeId}");
                return;
            }
            lm.AddSource(localeId, source);
        }

        private static void OnLocaleChanged()
        {
            var id = GameManager.instance?.localizationManager?.activeLocaleId ?? "(unknown)";
            s_Log.Info("[ART] Active locale = " + id);

            // Keep Options UI consistent after a locale flip
            s_Settings?.RegisterInOptionsUI();
        }
    }
}
