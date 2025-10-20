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

        // Action names (must match attributes in Setting.cs)
        public const string kInvertZoningActionName = "InvertZoning";
        public const string kToggleToolActionName = "ToggleZoneTool";

        // Settings instance (legacy alias + property)
        public static Setting m_Setting = null!;
        public static Setting? s_Settings
        {
            get; private set;
        }

        // Runtime handles to configured actions
        public static ProxyAction? m_InvertZoningAction
        {
            get; private set;
        }
        public static ProxyAction? m_ToggleToolAction
        {
            get; private set;
        }

        public static readonly ILog s_Log =
            LogManager.GetLogger("ART-Zone").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info("[ART] OnLoad");

            // Settings before locales (labels need a settings instance)
            var settings = new Setting(this);
            s_Settings = settings;
            m_Setting = settings;

            // Locales (add more later as you translate)
            AddLocale("en-US", new LocaleEN(settings));
            AddLocale("fr-FR", new LocaleFR(settings));
            AddLocale("de-DE", new LocaleDE(settings));
            AddLocale("es-ES", new LocaleES(settings));
            AddLocale("zh-HANS", new LocaleZH_CN(settings));
            AddLocale("zh-HANT", new LocaleZH_HANT(settings));

            // Load saved values, then show Options UI
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Create actions from the Setting.cs attributes and get live handles
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
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<ToolBootstrapSystem>(SystemUpdatePhase.UIUpdate);

            // Optional: locale change log (do NOT remove locales on dispose)
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
            var lm = gm?.localizationManager;
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

            // Keep Options UI consistent
            s_Settings?.RegisterInOptionsUI();
        }
    }
}
