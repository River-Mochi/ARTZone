// File: src/AdvancedRoadTools/AdvancedRoadToolsMod.cs
// Purpose: Mod entrypoint + zoning systems + C# locale registration.

namespace AdvancedRoadTools
{
    using AdvancedRoadTools.Tools;
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

        public static Setting m_Setting = null!;
        public const string kInvertZoningActionName = "InvertZoning";
        public static ProxyAction m_InvertZoningAction = default!;

        private LocaleEN _enLocale = null!;

        public static readonly ILog s_Log =
            LogManager.GetLogger("AdvancedRoadTools").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Debug($"{nameof(AdvancedRoadToolsMod)}.{nameof(OnLoad)}");

            // Settings
            m_Setting = new Setting(this);
            m_Setting.RegisterInOptionsUI();

            // Load saved settings (or defaults) first
            AssetDatabase.global.LoadSettings(ModID, m_Setting, new Setting(this));

            // Register C# locale (Options/Settings + tool strings)
            _enLocale = new LocaleEN(m_Setting);
            GameManager.instance.localizationManager.AddSource("en-US", _enLocale);

            // Keybind action proxy resolved from Settings
            m_InvertZoningAction = m_Setting.GetAction(kInvertZoningActionName);

            // Systems
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);

            GameManager.instance.localizationManager.onActiveDictionaryChanged +=
                () => s_Log.Info("Active locale: " + GameManager.instance.localizationManager.activeLocaleId);

            GameManager.instance.onGamePreload += CreateTools;
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

        public void OnDispose()
        {
            s_Log.Debug($"{nameof(AdvancedRoadToolsMod)}.{nameof(OnDispose)}");

            if (_enLocale != null)
            {
                GameManager.instance.localizationManager.RemoveSource(_enLocale);
                _enLocale = null!;
            }

            if (m_Setting != null)
            {
                m_Setting.UnregisterInOptionsUI();
                m_Setting = null!;
            }
        }
    }
}
