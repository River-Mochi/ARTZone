// File: src/ARTZoneMod.cs
// Purpose: Mod entrypoint; locales + settings + keybindings + tool registration (no Harmony).
// Notes:
//   • Locales are installed BEFORE Options UI (so labels render correctly).
//   • RMB flip uses vanilla ToolBaseSystem.cancelAction → no custom binding here.
//   • Top-left button & palette tile use the same icon path (single source of truth).

namespace ARTZone
{
    using System.Collections.Generic; // HashSet
    using ARTZone.Settings;
    using ARTZone.Systems;
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

        // Single source of truth for icon path (webpack publicPath = coui://ui-mods/)
        public const string UiCouiRoot = "coui://ui-mods";
        public const string MainIconPath = UiCouiRoot + "/images/ico-4square-color.svg";

        public const string VersionShort = "1.0.0";
#if DEBUG
        public const string InformationalVersion = VersionShort + " (DEBUG)";
#else
        public const string InformationalVersion = VersionShort;
#endif

        // Rebindable action IDs exposed in Options UI
        public const string kToggleToolActionName = "ToggleZoneTool"; // Shift+Z

        public static Setting? s_Settings
        {
            get; private set;
        }
        public static ProxyAction? m_ToggleToolAction
        {
            get; private set;
        }

        public static readonly ILog s_Log = LogManager.GetLogger(ModID).SetShowsErrorsInUI(false);

        // Locale management guard (avoid double-install on reload)
        private static readonly HashSet<string> s_InstalledLocales = new();
        private static bool s_ReapplyingLocale;

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info($"[ART] OnLoad v{InformationalVersion}");

            // Settings object first
            var settings = new Setting(this);
            s_Settings = settings;

            // --- Install locales BEFORE Options UI ---
            AddLocale("en-US", new LocaleEN(settings));
            AddLocale("zh-HANS", new LocaleZH_CN(settings)); // Simplified Chinese
            // Future locales: add files then uncomment
            // AddLocale("fr-FR",   new LocaleFR(settings));
            // AddLocale("es-ES",   new LocaleES(settings));

            // Load saved settings + register Options UI
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Key bindings (only Shift+Z)
            try
            {
                settings.RegisterKeyBindings();

                m_ToggleToolAction = settings.GetAction(kToggleToolActionName);
                if (m_ToggleToolAction != null)
                    m_ToggleToolAction.shouldBeEnabled = true;
            }
            catch (System.Exception ex)
            {
                s_Log.Warn($"[ART] Keybinding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // Systems
            updateSystem.UpdateAt<ARTPaletteBootstrapSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<Tools.ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<KeybindHotkeySystem>(SystemUpdatePhase.ToolUpdate);

            // Tool registration (definition only; prefab created after game load)
            ToolsHelper.Initialize(force: false);
            ToolsHelper.RegisterTool(
                new ToolDefinition(
                    typeof(Tools.ZoningControllerToolSystem),
                    Tools.ZoningControllerToolSystem.ToolID,
                    new ToolDefinition.UI(MainIconPath) // palette + top-left import use same asset name
                )
            );

            // Keep strings updated when game language changes
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

            var lm = GameManager.instance?.localizationManager;
            if (lm != null)
                lm.onActiveDictionaryChanged -= OnLocaleChanged;

            if (m_ToggleToolAction != null)
            {
                m_ToggleToolAction.shouldBeEnabled = false;
                m_ToggleToolAction = null;
            }

            s_Settings?.UnregisterInOptionsUI();
            s_Settings = null;
        }

        // ===== Locale helpers =====

        private static void AddLocale(string id, IDictionarySource src)
        {
            var lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                s_Log.Warn($"[ART] No LocalizationManager; cannot add locale {id}");
                return;
            }

            if (!s_InstalledLocales.Add(id))
                return;

            lm.AddSource(id, src);
            s_Log.Info($"[ART] Locale installed: {id}");
        }

        private static void OnLocaleChanged()
        {
            if (s_ReapplyingLocale)
                return;

            s_ReapplyingLocale = true;
            try
            {
                var id = GameManager.instance?.localizationManager?.activeLocaleId ?? "(unknown)";
                s_Log.Info("[ART] Active locale = " + id);
                s_Settings?.RegisterInOptionsUI();
            }
            finally
            {
                s_ReapplyingLocale = false;
            }
        }
    }
}
