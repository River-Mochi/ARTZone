// Mod.cs
// Purpose: Mod entrypoint; settings + locales; keybinding hookup (polling)

namespace AdvancedRoadTools
{
    using AdvancedRoadTools.Tools;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;

    public sealed class AdvancedRoadToolsMod : IMod
    {
        public const string ModID = "AdvancedRoadTools";

        // Action name used by Setting.cs attributes
        public const string kInvertZoningActionName = "InvertZoning";

        // === Settings (keep legacy field for older code, plus a modern property) ===
        public static Setting m_Setting = null!;           // legacy alias for existing files
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

            // Settings instance
            var settings = new Setting(this);
            s_Settings = settings;
            m_Setting = settings; // keep legacy callers working

            // Load saved values (or defaults)
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));

            // Locales (engine manages lifetime)
            TryAddLocale("en-US", new LocaleEN(settings));

            // Show settings in Options UI
            settings.RegisterInOptionsUI();

            // Key binding
            try
            {
                settings.RegisterKeyBindings();

                // Acquire the runtime handle and enable it for gameplay
                m_InvertZoningAction = settings.GetAction(kInvertZoningActionName);
                if (m_InvertZoningAction != null)
                {
                    m_InvertZoningAction.shouldBeEnabled = true; // enable via default activator
                }
            }
            catch (System.Exception ex)
            {
                s_Log.Warn($"[ART] Key binding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // --- Register our systems in the update loop ---
            // (Put these exactly in this order)
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);

            // IMPORTANT: bootstrap AFTER vanilla prefabs/UI are around.
            // This polls for a Road Services anchor, then creates our palette prefab.
            updateSystem.UpdateAt<ToolBootstrapSystem>(SystemUpdatePhase.UIUpdate);

            // Optional: log active locale flips
            Colossal.Localization.LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
                lm.onActiveDictionaryChanged += OnLocaleChanged;
            }
        }

        public void OnDispose()
        {
            s_Log.Info("[ART] OnDispose");

            // Unhook only what we hooked; do NOT remove locales
            GameManager gm = GameManager.instance;
            if (gm != null)
            {
                Colossal.Localization.LocalizationManager lm = gm.localizationManager;
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
                m_Setting = null!; // legacy alias cleared
            }
        }

        private static void TryAddLocale(string localeId, IDictionarySource source)
        {
            Colossal.Localization.LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                s_Log.Warn("[ART] No LocalizationManager; cannot add locale " + localeId);
                return;
            }
            lm.AddSource(localeId, source);
        }

        private static void OnLocaleChanged()
        {
            Colossal.Localization.LocalizationManager? lm = GameManager.instance?.localizationManager;
            var id = lm?.activeLocaleId ?? "(unknown)";
            s_Log.Info("[ART] Active locale = " + id);
        }
    }
}
