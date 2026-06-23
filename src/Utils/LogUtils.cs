// <copyright file="LogUtils.cs" company="River-Mochi">
// Copyright (c) 2026 River-Mochi. All rights reserved.
// Licensed under the MIT License. You may not use this file except in compliance with this License.
// See LICENSE file in the project root for full license information.
// This notice and the MIT License notice must be kept with
// all copies or substantial portions of this code.
// ================= </copyright> ======================

// Notes:
// - Routine logs append directly to the mod log file instead of routing through
//   Colossal's UI logger path, which can surface misleading popups.
// - WarnOnce prevents hot-path spam.

namespace CS2HonuShared
{
    using Colossal.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public static class LogUtils
    {
        private static readonly object s_WarnOnceLock = new object();
        private static readonly object s_FileWriteLock = new object();

        // Per-process key cache so hot-path warnings show once instead of spamming every update.
        private static readonly HashSet<string> s_WarnOnceKeys =
            new HashSet<string>(StringComparer.Ordinal);

        // Safety valve: if a caller uses too many dynamic keys, reset instead of growing forever.
        private const int MaxWarnOnceKeys = 2048;

        public static bool WarnOnce(ILog log, string key, Func<string> messageFactory, Exception? exception = null)
        {
            if (log == null || string.IsNullOrEmpty(key) || messageFactory == null)
            {
                return false;
            }

            if (!IsLevelEnabled(log, Level.Warn))
            {
                return false;
            }

            string fullKey = GetLogName(log) + "|" + key;

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

        public static void TryLog(ILog log, Level level, Func<string> messageFactory, Exception? exception = null)
        {
            if (log == null || messageFactory == null)
            {
                return;
            }

            if (!IsLevelEnabled(log, level))
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
                // Use the direct append path for routine mod logs.
                // This intentionally bypasses Colossal's logger write path because that path
                // can throw misleading UI popups when its own internal file stream fails.
                AppendDirect(log, level, message, level == Level.Error ? exception : null);
            }
            catch
            {
            }
        }

        private static void SafeLogNoException(ILog log, Level level, string message)
        {
            try
            {
                if (log != null && IsLevelEnabled(log, level))
                {
                    AppendDirect(log, level, message, null);
                }
            }
            catch
            {
            }
        }

        private static void AppendDirect(ILog log, Level level, string message, Exception? exception)
        {
            string logPath = GetLogPath(log);
            if (string.IsNullOrEmpty(logPath))
            {
                return;
            }

            lock (s_FileWriteLock)
            {
                // These are plain .NET file APIs on purpose:
                // - FileStream opens the mod log for append without going through CO's logger internals
                // - StreamWriter gives us a simple, stable text append path for everyday diagnostics
                // - FileShare.ReadWrite keeps the log viewable while the game is still running
                string? directory = Path.GetDirectoryName(logPath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using FileStream stream = new FileStream(
                    logPath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite);
                using StreamWriter writer = new StreamWriter(stream);

                writer.Write('[');
                writer.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"));
                writer.Write("] [");
                writer.Write(GetLevelName(level));
                writer.Write("]  ");
                writer.WriteLine(message ?? string.Empty);

                if (exception != null)
                {
                    writer.WriteLine(exception);
                }
            }
        }

        private static string GetLogPath(ILog log)
        {
            try
            {
                // Prefer the path Colossal already assigned to this custom logger if available.
                if (!string.IsNullOrEmpty(log.logPath))
                {
                    return log.logPath;
                }

                string logName = GetLogName(log);
                if (string.IsNullOrEmpty(logName))
                {
                    return string.Empty;
                }

                return Path.Combine(LogManager.kDefaultLogPath, logName + ".log");
            }
            catch
            {
                // Final fallback: derive the usual mod log file location from the mod id.
                return Path.Combine(LogManager.kDefaultLogPath, global::EasyZoning.Mod.ModID + ".log");
            }
        }

        private static string GetLogName(ILog log)
        {
            try
            {
                return string.IsNullOrEmpty(log.name) ? global::EasyZoning.Mod.ModID : log.name;
            }
            catch
            {
                return global::EasyZoning.Mod.ModID;
            }
        }

        private static bool IsLevelEnabled(ILog log, Level level)
        {
            try
            {
                return log.isLevelEnabled(level);
            }
            catch
            {
                // If Colossal logging state is mid-reload, prefer keeping direct-file logging alive.
                return true;
            }
        }

        private static string GetLevelName(Level level)
        {
            if (level == Level.Warn)
            {
                return "WARN";
            }

            if (level == Level.Error)
            {
                return "ERROR";
            }

            if (level == Level.Debug)
            {
                return "DEBUG";
            }

            if (level == Level.Trace)
            {
                return "TRACE";
            }

            if (level == Level.Verbose)
            {
                return "VERBOSE";
            }

            return "INFO";
        }
    }
}
