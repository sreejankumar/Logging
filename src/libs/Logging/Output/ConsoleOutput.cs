using Logging.Layout;
using NLog;
using NLog.Config;

namespace Logging.Output
{
    public static partial class LogOutput
    {
        private static void ConsoleOutput(LoggingConfiguration config)
        {
            SetRendererKeys();

            var target = new NLog.Targets.ConsoleTarget("console") {Layout = LogLayout.JsonLogLayout};
            config.AddTarget(target);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
        }
    }
}