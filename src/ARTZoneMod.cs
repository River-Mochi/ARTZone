// File: src/ARTZoneMod.cs
// Purpose: Mod entrypoint; locales + settings + keybindings + tool registration (no Harmony).
// Notes:
//   • Locales are installed BEFORE Options UI (so labels render correctly).
//   • RMB flip uses vanilla ToolBaseSystem.cancelAction → no custom binding here.
//   • Top-left button & Panel tile use the same icon path (single source of truth).

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

        // Single source of truth in C# for game icon path 
        public const string UiCouiRoot = "coui://ui-mods";      // webpack publicPath = coui://ui-mods/
        public const string MainIconPath = UiCouiRoot + "/images/ico-4square-color.svg"; // must match in artzone-tool-button.tsx

        public const string VersionShort = "1.0.1";

        // Rebindable action IDs exposed in Options UI
        public const string kToggleToolActionName = "ToggleZoneTool";   // Shift+Z

        public static Setting? Settings
        {
            get; private set;
        }
        public static ProxyAction? ToggleToolAction
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
            Settings = settings;

            // Register locales BEFORE register Options UI
            AddLocale("en-US", new LocaleEN(settings));
            AddLocale("fr-FR", new LocaleFR(settings));
            // AddLocale("de-DE", new LocaleDE(settings));
            AddLocale("es-ES", new LocaleES(settings));
            // AddLocale("it-IT", new LocaleIT(settings));
            AddLocale("ja-JP", new LocaleJA(settings));
            AddLocale("ko-KR", new LocaleKO(settings));
            // AddLocale("pl-PL", new LocalePL(settings));
            AddLocale("pt-BR", new LocalePT_BR(settings));
            AddLocale("zh-HANS", new LocaleZH_CN(settings));    // Simplified Chinese
            // AddLocale("zh-HANT", new LocaleZH_HANT(settings));

            // Load saved settings + register Options UI
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Key bindings (only Shift+Z)
            try
            {
                settings.RegisterKeyBindings();

                ToggleToolAction = settings.GetAction(kToggleToolActionName);
                if (ToggleToolAction != null)
                    ToggleToolAction.shouldBeEnabled = true;
            }
            catch (System.Exception ex)
            {
                s_Log.Warn($"[ART] Keybinding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // Systems
            updateSystem.UpdateAt<PanelBootStrapSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<KeybindHotkeySystem>(SystemUpdatePhase.ToolUpdate);

            // Tool registration (definition only; prefab created after game load)
            PanelBuilder.Initialize(force: false);
            PanelBuilder.RegisterTool(
                new ToolDefinition(
                    typeof(ZoningControllerToolSystem),
                    ZoningControllerToolSystem.ToolID,
                    new ToolDefinition.UI(MainIconPath) // Panel + top-left import use same asset name
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

            if (ToggleToolAction != null)
            {
                ToggleToolAction.shouldBeEnabled = false;
                ToggleToolAction = null;
            }

            Settings?.UnregisterInOptionsUI();
            Settings = null;
        }

        // ---- Locale helpers -----------------------------------------

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
                Settings?.RegisterInOptionsUI();
            }
            finally
            {
                s_ReapplyingLocale = false;
            }
        }
    }
}
