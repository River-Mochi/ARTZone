// File: Utils/LogUtils.cs
// Shared version 0.3.3
// Purpose:
// - WarnOnce: prevents repeated WARN spam in hot paths
// - TryLog: lazy message construction inside try/catch
// - Popup-safe: do NOT attach Exception objects at Warn level (can surface in-game popups)
// - Optional: attach Exception only at Error level

namespace CS2HonuShared
{
    using Colossal.Logging;
    using System;
    using System.Collections.Generic;

    public static class LogUtils
    {
        private static readonly object s_WarnOnceLock = new object();

        // Each mod is a separate assembly; one static set per mod.
        // Prefix keys with log.name to avoid collisions across loggers.
        private static readonly HashSet<string> s_WarnOnceKeys =
            new HashSet<string>(StringComparer.Ordinal);

        // Safety valve: don’t let a bad key strategy grow unbounded.
        private const int MaxWarnOnceKeys = 2048;

        public static bool WarnOnce(ILog log, string key, Func<string> messageFactory, Exception? exception = null)
        {
            if (log == null || string.IsNullOrEmpty(key) || messageFactory == null)
            {
                return false;
            }

            // Avoid locking if WARN is filtered out.
            if (!log.isLevelEnabled(Level.Warn))
            {
                return false;
            }

            string fullKey = log.name + "|" + key;

            lock (s_WarnOnceLock)
            {
                if (s_WarnOnceKeys.Count >= MaxWarnOnceKeys)
                {
                    s_WarnOnceKeys.Clear();
                }

                if (!s_WarnOnceKeys.Add(fullKey))
                {
                    return false;
                }
            }

            TryLog(log, Level.Warn, messageFactory, exception);
            return true;
        }

        /// <summary>
        /// Safe logging wrapper:
        /// - Only evaluates messageFactory if the level is enabled
        /// - Never throws outward (even if messageFactory or the logger throws)
        /// - Avoids attaching Exception objects except at Error (optional policy)
        /// </summary>
        public static void TryLog(ILog log, Level level, Func<string> messageFactory, Exception? exception = null)
        {
            if (log == null || messageFactory == null)
            {
                return;
            }

            if (!log.isLevelEnabled(level))
            {
                return;
            }

            string message;

            try
            {
                message = messageFactory() ?? string.Empty;
            }
            catch (Exception ex)
            {
                // Message factory failed; best effort log without attaching Exception (popup-safe).
                SafeLogNoException(log, Level.Warn, "Log message factory threw: " + ex.GetType().Name + ": " + ex.Message);
                return;
            }

            try
            {
                // Optional policy: only attach Exception at Error level.
                Exception? attach = (exception != null && level == Level.Error) ? exception : null;
                log.Log(level, message, attach ?? null!);
            }
            catch
            {
                // Logging must never throw back into gameplay/mod loading.
            }
        }

        private static void SafeLogNoException(ILog log, Level level, string message)
        {
            try
            {
                if (log != null && log.isLevelEnabled(level))
                {
                    log.Log(level, message, null!);
                }
            }
            catch
            {
            }
        }
    }
}
