using Colossal.Logging;

namespace AdvancedRoadTools.Logging;

public static class LogART
{    
    public static ILog log = LogManager.GetLogger($"[{nameof(AdvancedRoadTools)}")
        .SetShowsErrorsInUI(false);
    public static void Info(string content)
    {
        LogART.Info(content + $" Info]");
    }
    public static void Debug(string content)
    {
        LogART.Debug(content + $" Debug]");
    }
    public static void Warn(string content)
    {
        LogART.Warn(content + $" Warn]");
    }
    public static void Error(string content)
    {
        LogART.Error(content + $" Error]");
    }
}