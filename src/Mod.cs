// File: src/Mod.cs
// Purpose: Mod entrypoint; settings + keybindings + tool registration (no Harmony).
// Change: DO NOT instantiate tools here; UISystem does it after load.

namespace AdvancedRoadTools
{
    using AdvancedRoadTools.Systems;
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
        public const string CouiRoot = "coui://" + ModID;

        public const string VersionShort = "1.0.0";
#if DEBUG
        public const string InformationalVersion = VersionShort + " (DEBUG)";
#else
        public const string InformationalVersion = VersionShort;
#endif

        public const string kInvertZoningActionName = "InvertZoning";
        public const string kToggleToolActionName = "ToggleZoneTool";

        public static Setting? s_Settings
        {
            get; private set;
        }
        public static ProxyAction? m_InvertZoningAction
        {
            get; private set;
        }
        public static ProxyAction? m_ToggleToolAction
        {
            get; private set;
        }

        public static readonly ILog s_Log = LogManager.GetLogger(ModID).SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info($"[ART] OnLoad v{InformationalVersion}");

            // Settings + locales
            var settings = new Setting(this);
            s_Settings = settings;
            TryAddLocale("en-US", new LocaleEN(settings));
            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Key bindings (ProxyAction)
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

            // Systems
            updateSystem.UpdateAt<ARTPaletteBootstrapSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<KeybindHotkeySystem>(SystemUpdatePhase.ToolUpdate);

            // Register tool definition ONLY (no instantiation here)
            ToolsHelper.Initialize(force: false);
            ToolsHelper.RegisterTool(
                new ToolDefinition(
                    typeof(ZoningControllerToolSystem),
                    ZoningControllerToolSystem.ToolID,
                    new ToolDefinition.UI("coui://ui-mods/images/grid-color.svg")   // tile for inside Road Services group
                )
            );

            // Keep strings in sync if player changes game language
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

            s_Settings?.UnregisterInOptionsUI();
            s_Settings = null;
        }

        private static void TryAddLocale(string id, IDictionarySource src)
        {
            var lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                s_Log.Warn($"[ART] No LocalizationManager; cannot add locale {id}");
                return;
            }
            lm.AddSource(id, src);
        }

        private static void OnLocaleChanged()
        {
            var id = GameManager.instance?.localizationManager?.activeLocaleId ?? "(unknown)";
            s_Log.Info("[ART] Active locale = " + id);
            s_Settings?.RegisterInOptionsUI();
        }
    }
}
