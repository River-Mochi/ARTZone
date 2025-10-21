// File: src/Mod.cs
// Purpose: Mod entrypoint; settings + keybindings via Colossal API (no Unity.InputSystem)

namespace AdvancedRoadTools
{
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
        public const string CouiRoot = "coui://" + ModID; // resolves to coui://AdvancedRoadTools (or ARTZone if change ModID)

        public const string VersionShort = "1.0.0";
#if DEBUG
        public const string InformationalVersion = VersionShort + " (DEBUG)";
#else
        public const string InformationalVersion = VersionShort;
#endif



        // Action names used by Setting attributes
        public const string kInvertZoningActionName = "InvertZoning";
        public const string kToggleToolActionName = "ToggleZoneTool";

        public static Setting? s_Settings
        {
            get; private set;
        }

        // Runtime input handles CO ProxyAction
        public static ProxyAction? m_InvertZoningAction
        {
            get; private set;
        }
        public static ProxyAction? m_ToggleToolAction
        {
            get; private set;
        }

        public static readonly ILog s_Log =
            LogManager.GetLogger("AdvancedRoadTools").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info($"[ART] OnLoad v{VersionShort}");

            // Settings FIRST, then locales, then load, then register UI
            var settings = new Setting(this);
            s_Settings = settings;

            // Locales BEFORE Options UI
            TryAddLocale("en-US", new LocaleEN(settings));

            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));
            settings.RegisterInOptionsUI();

            // Key binding registration (creates actions) + get runtime handles
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

            // Register systems (order matters)
            updateSystem.UpdateAt<Tools.ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<Tools.ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<Tools.ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<Tools.ToolBootstrapSystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<Tools.KeybindHotkeySystem>(SystemUpdatePhase.ToolUpdate); // listens for Shift+Z, RMB

            // Road Services palette tile registration
            Tools.ToolsHelper.Initialize();
            Tools.ToolsHelper.RegisterTool(
                new Tools.ToolDefinition(
                    typeof(Tools.ZoningControllerToolSystem),
                    Tools.ZoningControllerToolSystem.ToolID,
                    new Tools.ToolDefinition.UI($"{CouiRoot}/images/ToolsIcon.png")
                )
            );
            // ToolBootstrapSystem will keep trying until Road Services is ready

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
            s_Settings?.RegisterInOptionsUI(); // keep labels refreshed
        }
    }
}
