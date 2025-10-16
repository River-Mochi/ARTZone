// AdvancedRoadToolsMod.cs
// Purpose: Mod entry point (Colossal-aligned). Registers settings, key bindings, locales; gets ProxyAction.

#nullable enable

namespace AdvancedRoadTools
{
    using Colossal;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Colossal.Serialization.Entities; // Purpose
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;
    using AdvancedRoadTools.Tools;

    public sealed class AdvancedRoadToolsMod : IMod
    {
        public const string ModID = "AdvancedRoadTools";

        // Action name must match Setting.cs attribute
        public const string kInvertZoningActionName = "InvertZoning";

        // Expose settings + action to the rest of the mod
        public static Setting? s_Settings
        {
            get; private set;
        }
        public static ProxyAction? m_InvertZoningAction
        {
            get; private set;
        }

        // Simple logger (quiet in UI)
        public static readonly ILog s_Log =
            LogManager.GetLogger("AdvancedRoadTools").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info("[ART] OnLoad");

            // Show where we’re loading from (helpful while deving)
            if (GameManager.instance?.modManager != null &&
                GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                s_Log.Info("[ART] Asset path: " + asset.path);
            }

            // Settings instance
            var settings = new Setting(this);
            s_Settings = settings;

            // Load saved values (or defaults)
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));

            // Locales (yours lives in LocaleEN.cs); add BEFORE registering Options if labels/descs are localized
            TryAddLocale("en-US", new LocaleEN(settings));

            // Show settings in Options UI
            settings.RegisterInOptionsUI();

            // REQUIRED with the attribute-based template: ensure actions exist before GetAction()
            try
            {
                settings.RegisterKeyBindings();

                // Acquire the runtime action handle (ProxyAction). This is what we poll each frame.
                m_InvertZoningAction = settings.GetAction(kInvertZoningActionName);
                if (m_InvertZoningAction != null)
                {
                    m_InvertZoningAction.shouldBeEnabled = true; // enable runtime polling
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
            var lm = GameManager.instance?.localizationManager;
            if (lm != null)
            {
                lm.onActiveDictionaryChanged -= OnLocaleChanged;
                lm.onActiveDictionaryChanged += OnLocaleChanged;
            }

            // Create tool prefabs once preloading starts
            GameManager.instance.onGamePreload += CreateTools;
        }

        public void OnDispose()
        {
            s_Log.Info("[ART] OnDispose");

            // Unhook only what we hook. Do NOT remove locales; the game manages them.
            if (GameManager.instance != null)
            {
                GameManager.instance.onGamePreload -= CreateTools;
                var lm = GameManager.instance.localizationManager;
                if (lm != null)
                {
                    lm.onActiveDictionaryChanged -= OnLocaleChanged;
                }
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
            }
        }

        private static void TryAddLocale(string localeId, IDictionarySource source)
        {
            var lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                s_Log.Warn("[ART] No LocalizationManager; cannot add locale " + localeId);
                return;
            }
            lm.AddSource(localeId, source);
        }

        private static void OnLocaleChanged()
        {
            var lm = GameManager.instance?.localizationManager;
            var id = lm?.activeLocaleId ?? "(unknown)";
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
                GameManager.instance.onGamePreload -= CreateTools;
            }
        }
    }
}
