using System;

namespace AdvancedRoadTools.Core.Logging;

public static class Log
{
    public static void Info(string content)
    {
        AdvancedRoadToolsMod.log.Info(content);
    }
    public static void Debug(string content)
    {
        AdvancedRoadToolsMod.log.Debug(content);
    }
    public static void Warn(string content)
    {
        AdvancedRoadToolsMod.log.Warn(content);
    }
    public static void Error(string content)
    {
        AdvancedRoadToolsMod.log.Error(content);
    }
}