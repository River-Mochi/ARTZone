// <copyright file="Mod.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// File: src/Mod.cs
// Purpose: Mod entrypoint; registers settings, localization, ECS systems, and keybindings.
// Notes:
//   • Locales install before Options UI so labels render correctly.
//   • Existing-roads tool uses the game tool actions:
//     - LMB = Apply
//     - RMB = Secondary Apply (4 cycles mode)
//     - ESC = explicit Keyboard cancel (because vanilla Cancel is often bound to RMB).
//   • Top-left button points at coui://ui-mods/images/* assets.

namespace EasyZoning
{
    using Colossal;                  // IDictionarySource (locale sources)
    using Colossal.IO.AssetDatabase; // AssetDatabase.LoadSettings
    using Colossal.Localization;     // LocalizationManager (locale sources)
    using Colossal.Logging;          // ILog, LogManager (mod log)
    using CS2HonuShared;             // LogUtils (safe logging + WarnOnce)
    using EasyZoning.Tools;          // ECS systems scheduled by UpdateSystem
    using Game;                      // UpdateSystem, SystemUpdatePhase
    using Game.Modding;              // IMod
    using Game.SceneFlow;            // GameManager (localization manager access)
    using System;                    // Exception, Func<T>
    using System.Reflection;         // Assembly (version number from csproj)

    public sealed class Mod : IMod
    {
        // ---- PUBLIC CONSTANTS / METADATA ----

        public const string ModName = "Easy Zoning";
        public const string ModID = "EasyZoning";
        public const string ModTag = "[EZ]";

        /// <summary>
        /// Version read from assembly (3-part), fallback to "1.0.0".
        /// </summary>
        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        // COUI base
        public const string UiCouiRoot = "coui://ui-mods";

        // Top-left floating action button
        public const string MainIconPath = UiCouiRoot + "/images/ico-zones-color02.svg";

        // Rebindable action ID exposed in Options UI
        public const string kToggleToolActionName = "ToggleZoneTool";   // default Ctrl+V

        // ---- BUILD TAG / LOGGER ----

#if DEBUG
        private const string BuildTag = "[DEBUG]";
#else
        private const string BuildTag = "[RELEASE]";
#endif

        // create Mod specific log file.
        public static readonly ILog s_Log =
            LogManager.GetLogger(ModID).SetShowsErrorsInUI(
#if DEBUG
                true
#else
                false
#endif
            );

        // ---- GLOBAL SETTINGS ----

        /// <summary>
        /// Global settings instance (Options UI).
        /// </summary>
        public static Setting? Settings
        {
            get; private set;
        }

        // ---- PRIVATE STATE ----

        private static bool s_BannerLogged;

        // ---- IMod IMPLEMENTATION ----

        public void OnLoad(UpdateSystem updateSystem)
        {
            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                LogSafe(( ) => $"{ModName} {ModTag} v{ModVersion} OnLoad {BuildTag}");
            }

            if (GameManager.instance == null)
            {
                WarnSafe(( ) => "GameManager.instance is null in Mod.OnLoad.");
                return;
            }

            Setting setting = new Setting(this);
            Settings = setting;

            // Localization sources
            AddLocaleSource("en-US", new LocaleEN(setting));
            AddLocaleSource("fr-FR", new LocaleFR(setting));
            AddLocaleSource("es-ES", new LocaleES(setting));
            AddLocaleSource("de-DE", new LocaleDE(setting));
            AddLocaleSource("it-IT", new LocaleIT(setting));
            AddLocaleSource("ja-JP", new LocaleJA(setting));
            AddLocaleSource("ko-KR", new LocaleKO(setting));
            AddLocaleSource("pl-PL", new LocalePL(setting));
            AddLocaleSource("pt-BR", new LocalePT_BR(setting));
            AddLocaleSource("zh-HANS", new LocaleZH_CN(setting));    // Simplified Chinese
            AddLocaleSource("zh-HANT", new LocaleZH_HANT(setting));  // Traditional Chinese

            // Settings + Options UI
            try
            {
                AssetDatabase.global.LoadSettings(ModID, setting, new Setting(this));
                setting.RegisterInOptionsUI();
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"Settings/UI init failed: {ex.GetType().Name}: {ex.Message}");
            }

            // Keybinding setup
            try
            {
                setting.RegisterKeyBindings();
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"Keybinding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // ECS systems
            try
            {
                // Tool logic: selection and apply (only runs when tool is active).
                updateSystem.UpdateAt<ZoningControllerToolSystem>(SystemUpdatePhase.ToolUpdate);

                // Visual highlight for blocks under the tool.
                updateSystem.UpdateAt<ToolHighlightSystem>(SystemUpdatePhase.ToolUpdate);

                // New-road sync: apply zoning mode to blocks as roads are built.
                updateSystem.UpdateAt<SyncNewRoadsSystem>(SystemUpdatePhase.Modification4);

                // Existing-block sync: apply tool depths to zone blocks on existing roads.
                updateSystem.UpdateAt<SyncBlockSystem>(SystemUpdatePhase.Modification4B);

                // Visual override for preview border and fill colors (remove cells only)
                updateSystem.UpdateAt<PreviewColorOverrideSystem>(SystemUpdatePhase.UIUpdate);

                // UI bridge: binds C# values to the React UI (runs every frame in UIUpdate).
                updateSystem.UpdateAt<ZoneControlBridgeUI>(SystemUpdatePhase.UIUpdate);

                // Hotkey system that listens to ToggleZoneTool (Ctrl+V).
                updateSystem.UpdateAt<KeybindHotkeySystem>(SystemUpdatePhase.ToolUpdate);
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"System scheduling/init failed: {ex.GetType().Name}: {ex.Message}");
            }

            // No active-dictionary event hook is needed here. All Easy Zoning locale
            // sources are registered once above, and the game rebuilds the active
            // dictionary from registered sources when the player changes language.
        }

        public void OnDispose( )
        {
            LogSafe(( ) => "OnDispose");

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }

        // ---- LOCALE HANDLING ----

        private static void AddLocaleSource(string localeId, IDictionarySource source)
        {
            if (string.IsNullOrEmpty(localeId))
                return;

            LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                WarnSafe(( ) => $"AddLocaleSource: No LocalizationManager; cannot add source for '{localeId}'.");
                return;
            }

            try
            {
                lm.AddSource(localeId, source);
            }
            catch (Exception ex)
            {
                WarnSafe(( ) => $"AddLocaleSource: AddSource for '{localeId}' failed: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // ---- LogUtils HELPERS ----

        public static void LogSafe(Func<string> messageFactory) =>
            LogUtils.TryLog(s_Log, Level.Info, messageFactory);

        public static void WarnSafe(Func<string> messageFactory) =>
            LogUtils.TryLog(s_Log, Level.Warn, messageFactory);

        public static void WarnOnce(string key, Func<string> messageFactory) =>
            LogUtils.WarnOnce(s_Log, key, messageFactory);
    }
}
