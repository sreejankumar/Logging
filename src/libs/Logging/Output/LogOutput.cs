using System.Diagnostics;
using System.Reflection;
using Logging.Configuration;
using Logging.Constants;
using Logging.Models;
using Logging.Renderer;
using NLog;
using NLog.Config;

namespace Logging.Output
{
    public static partial class LogOutput
    {
        public static LoggingConfiguration RegisterLogTargets()
        {
            SetApplicationSourceName();

            var config = LogManager.Configuration ?? new LoggingConfiguration();

            ConsoleOutput(config);
            LogManager.Configuration = config;
            return config;
        }

        public static LoggingConfiguration RegisterLogTargets(NetworkLogConfiguration networkLogConfiguration)
        {
            SetApplicationSourceName();
            var config = LogManager.Configuration ?? new LoggingConfiguration();
            
            if (!networkLogConfiguration.DisableConsoleLogging)
            {
                ConsoleOutput(config);
            }
            NetworkOutput(config, networkLogConfiguration);
            LogManager.Configuration = config;
            return config;
        }
        
        public static LoggingConfiguration RegisterLogTargets(NetworkLogConfiguration networkLogConfiguration, EmailLogConfiguration emailLogConfiguration)
        {
            SetApplicationSourceName();
            var config = LogManager.Configuration ?? new LoggingConfiguration();
            
            if (!networkLogConfiguration.DisableConsoleLogging)
            {
                ConsoleOutput(config);
            }
            NetworkOutput(config, networkLogConfiguration);
            EmailOutput(config,emailLogConfiguration);
            LogManager.Configuration = config;
            return config;
        }

        private static void SetApplicationSourceName(string name = null)
        {
            GlobalDiagnosticsContext.Set(JsonDataRendererConstants.GdcSourceKey, name ?? Assembly.GetEntryAssembly()?.GetName().Name);
            GlobalDiagnosticsContext.Set(JsonDataRendererConstants.GdcProcessIdKey, Process.GetCurrentProcess().Id);
        }

        private static void SetRendererKeys()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(JsonDataRendererConstants.SequenceIdRendererKey, typeof(SequenceIdRenderer));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition(JsonDataRendererConstants.JsonDataRendererKey, typeof(JsonDataRenderer));
        }

    }
}