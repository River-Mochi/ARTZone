// File: Utils/LocaleUtils.cs
// Purpose: Safe localization lookup + safe formatting helpers for Options UI strings.
// Notes:
// - Read-only: never mutates localization state.
// - Null-safe: returns fallback when LocalizationManager/dictionary is unavailable.
// - Culture-aware: numeric formatting uses current culture.

namespace CS2HonuShared
{
    using Colossal.Localization;   // LocalizationDictionary
    using Game.SceneFlow;          // GameManager
    using System;                  // Exception, FormatException
    using System.Globalization;    // CultureInfo

    public static class LocaleUtils
    {
        public static string Localize(string entryId, string fallback)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return fallback;
            }

            try
            {
                LocalizationDictionary? dict = GameManager.instance?.localizationManager?.activeDictionary;
                if (dict != null && dict.TryGetValue(entryId, out string value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            catch
            {
            }

            return fallback;
        }

        public static string SafeFormat(string entryId, string fallbackFormat, params object[] args)
        {
            string format = Localize(entryId, fallbackFormat);

            try
            {
                return string.Format(CultureInfo.CurrentCulture, format, args);
            }
            catch (FormatException)
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, fallbackFormat, args);
                }
                catch
                {
                    return fallbackFormat;
                }
            }
            catch
            {
                return fallbackFormat;
            }
        }

        public static string FormatN0(long value)
            => value.ToString("N0", CultureInfo.CurrentCulture);

        public static string FormatN0(double value)
            => Math.Round(value).ToString("N0", CultureInfo.CurrentCulture);
    }
}
