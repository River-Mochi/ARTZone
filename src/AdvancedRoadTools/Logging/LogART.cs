using Colossal.Logging;

namespace AdvancedRoadTools.Logging;

public static class LogART
{
    public static ILog logger = LogManager.GetLogger($"{nameof(AdvancedRoadTools)}")
        .SetShowsErrorsInUI(false);

    public static void Info(string content) => logger.Info(content);

    public static void Debug(string content) => logger.Debug(content);

    public static void Warn(string content) => logger.Warn(content);

    public static void Error(string content) => logger.Error(content);
}