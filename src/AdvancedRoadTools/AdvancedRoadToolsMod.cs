// AdvancedRoadToolsMod.cs
// Purpose: Mod entrypoint; settings + locales; keybinding hookup (polling); legacy m_Setting field kept.

#nullable enable

namespace AdvancedRoadTools
{
    using AdvancedRoadTools.Tools;
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Colossal.Serialization.Entities; // Purpose
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
            LogManager.GetLogger("AdvancedRoadTools").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info("[ART] OnLoad");

            // Settings instance
            Setting settings = new Setting(this);
            s_Settings = settings;
            m_Setting = settings; // keep legacy callers working

            // Load saved values (or defaults)
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));

            // Locales (engine manages lifetime)
            TryAddLocale("en-US", new LocaleEN(settings));

            // Show settings in Options UI
            settings.RegisterInOptionsUI();

            // REQUIRED with attribute-based bindings
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

            // Systems
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);

            // Optional: log active locale flips
            Colossal.Localization.LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
                lm.onActiveDictionaryChanged += OnLocaleChanged;
            }

            // Create tool prefabs once preloading starts (guard instance for CS8602)
            GameManager? gm = GameManager.instance;
            if (gm != null)
            {
                gm.onGamePreload -= CreateTools; // ensure no double subscription
                gm.onGamePreload += CreateTools;
            }
            else
            {
                s_Log.Warn("[ART] GameManager.instance is null during OnLoad; skipping onGamePreload hook.");
            }
        }

        public void OnDispose()
        {
            s_Log.Info("[ART] OnDispose");

            // Unhook only what we hooked; do NOT remove locales
            GameManager gm = GameManager.instance;
            if (gm != null)
            {
                gm.onGamePreload -= CreateTools;
                Colossal.Localization.LocalizationManager lm = gm.localizationManager;
                if (lm != null)
                    lm.onActiveDictionaryChanged -= OnLocaleChanged;
            }

            if (m_InvertZoningAction != null)
            {
                // No event subscriptions to remove (we're polling)
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
            string id = lm?.activeLocaleId ?? "(unknown)";
            s_Log.Info("[ART] Active locale = " + id);
        }

        private void CreateTools(Purpose purpose, GameMode mode)
        {
            try
            {
                ToolsHelper.InstantiateTools();
            }
            finally
            {
                GameManager gm = GameManager.instance;
                if (gm != null)
                    gm.onGamePreload -= CreateTools;
            }
        }
    }
}
