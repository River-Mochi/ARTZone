// File: src/ARTZoneMod.cs
// Purpose: Mod entrypoint; settings + locales; keybinding hookup (polling)

namespace ARTZone
{
    using ARTZone.Tools;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;

    public sealed class ARTZoneMod : IMod
    {
        public const string ModID = "ARTZone";

        // Action name used by Setting.cs attributes
        public const string kInvertZoningActionName = "InvertZoning";

        // Settings (legacy alias + modern property)
        public static Setting m_Setting = null!;
        public static Setting? s_Settings
        {
            get; private set;
        }

        // Runtime input action (polled in tool)
        public static ProxyAction? m_InvertZoningAction
        {
            get; private set;
        }

        public static readonly ILog s_Log =
            LogManager.GetLogger("ART-Zone").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info("[ART] OnLoad");

            // Settings must exist before locales (so labels/IDs resolve)
            var settings = new Setting(this);
            s_Settings = settings;
            m_Setting = settings;

            // Register locales BEFORE Options UI registration
            AddLocale("en-US", new LocaleEN(settings));
            AddLocale("fr-FR", new LocaleFR(settings));
            AddLocale("de-DE", new LocaleDE(settings));
            AddLocale("es-ES", new LocaleES(settings));
            AddLocale("zh-HANS", new LocaleZH_CN(settings));   // Simplified Chinese
            AddLocale("zh-HANT", new LocaleZH_HANT(settings)); // Traditional Chinese

            // Ready to turn on more later:
            // AddLocale("it-IT",  new LocaleIT(settings));
            // AddLocale("ja-JP",  new LocaleJA(settings));
            // AddLocale("ko-KR",  new LocaleKO(settings));
            // AddLocale("vi-VN",  new LocaleVI(settings));
            // AddLocale("pl-PL",  new LocalePL(settings));
            // AddLocale("pt-BR",  new LocalePT_BR(settings));

            // Load saved values (or defaults), then show Settings UI
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Key binding (safe-guarded)
            try
            {
                settings.RegisterKeyBindings();
                m_InvertZoningAction = settings.GetAction(kInvertZoningActionName);
                if (m_InvertZoningAction != null)
                    m_InvertZoningAction.shouldBeEnabled = true;
            }
            catch (System.Exception ex)
            {
                s_Log.Warn($"[ART] Key binding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // Register our systems (order matters)
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<ToolBootstrapSystem>(SystemUpdatePhase.UIUpdate);

            // Locale change logging
            var lm = GameManager.instance?.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
                lm.onActiveDictionaryChanged += OnLocaleChanged;
            }
        }

        public void OnDispose()
        {
            s_Log.Info("[ART] OnDispose");

            var gm = GameManager.instance;
            if (gm != null)
            {
                var lm = gm.localizationManager;
                if (lm != null)
                    lm.onActiveDictionaryChanged -= OnLocaleChanged;
            }

            if (m_InvertZoningAction != null)
            {
                m_InvertZoningAction.shouldBeEnabled = false;
                m_InvertZoningAction = null;
            }

            if (s_Settings != null)
            {
                s_Settings.UnregisterInOptionsUI();
                s_Settings = null;
                m_Setting = null!;
            }
        }

        // ----- Helpers -----

        private static void AddLocale(string localeId, IDictionarySource source)
        {
            var lm = GameManager.instance?.localizationManager;
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

            // Ensure settings labels rebuild if needed
            s_Settings?.RegisterInOptionsUI();
        }
    }
}
