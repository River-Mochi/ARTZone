// File: src/AdvancedRoadTools/Mod.cs
// Purpose: Mod entrypoint; settings + keybindings via Colossal API (no Unity.InputSystem)

namespace AdvancedRoadTools
{
    // Tool systems you already have:
    using AdvancedRoadTools.Tools;
    using Colossal.IO.AssetDatabase;
    using Colossal.Logging;
    using Game;
    using Game.Input;
    using Game.Modding;

    public sealed class AdvancedRoadToolsMod : IMod
    {
        // ---- Meta ----
        public const string ModID = "AdvancedRoadTools";
        public const string Name = "Advanced Road Tools";
        public const string VersionShort = "1.0.0";

        // ---- Actions (must match Setting.cs attributes) ----
        public const string kInvertZoningActionName = "InvertZoning";
        public const string kToggleToolActionName = "ToggleZoneTool";

        // ---- Settings instance ----
        // Legacy alias (other files reference this)
        public static Setting m_Setting = null!;
        // Modern property
        public static Setting? s_Settings
        {
            get; private set;
        }

        // ---- Runtime input actions (available to any system) ----
        public static ProxyAction? m_InvertZoningAction
        {
            get; private set;
        }
        public static ProxyAction? m_ToggleToolAction
        {
            get; private set;
        }

        // ---- Logging ----
        public static readonly ILog s_Log =
            LogManager.GetLogger("AdvancedRoadTools").SetShowsErrorsInUI(false);

        public void OnLoad(UpdateSystem updateSystem)
        {
            s_Log.Info($"{Name} v{VersionShort} - OnLoad");

            // Create settings and load saved values
            var settings = new Setting(this);
            s_Settings = settings;
            m_Setting = settings; // keep legacy callers working

            AssetDatabase.global.LoadSettings(ModID, settings, new Setting(this));

            // Register in Options UI + bind actions
            settings.RegisterInOptionsUI();

            try
            {
                settings.RegisterKeyBindings();

                // Get runtime action handles and enable them
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

            // --- Register systems in the update loop (keep your existing order) ---
            updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<SyncCreatedRoadsSystem>(SystemUpdatePhase.Modification4);
            updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);
            updateSystem.UpdateAt<ZoningControllerToolUISystem>(SystemUpdatePhase.UIUpdate);
            updateSystem.UpdateAt<ToolBootstrapSystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            s_Log.Info($"{Name} - OnDispose");

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
                m_Setting = null!; // legacy alias cleared
            }
        }
    }
}
