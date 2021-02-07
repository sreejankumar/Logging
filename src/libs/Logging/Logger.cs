using System;
using System.Runtime.CompilerServices;
using Logging.Interfaces;
using NLog;

namespace Logging
{
    public class Logger<T> : ILogger<T>
    {
        public Logger(IContextLogModel contextLogModel)
        {
            LogContext = contextLogModel;
        }

        public IContextLogModel LogContext { get; private set; }


        public void LogInfo(string info, [CallerMemberName] string callerMemberName = null)
        {
            InternalLog(LogLevel.Info,  callerMemberName,info, null);
        }

        public void LogDebug(string debug, [CallerMemberName] string callerMemberName = null)
        {
            InternalLog(LogLevel.Debug, callerMemberName,debug, null);
        }

        public void LogWarn(string warn, [CallerMemberName] string callerMemberName = null)
        {
            InternalLog(LogLevel.Warn, callerMemberName,warn, null);
        }

        public void LogError(string error, Exception ex, [CallerMemberName] string callerMemberName = null)
        {
            InternalLog(LogLevel.Error, callerMemberName, error, ex);
        }

        private void InternalLog(LogLevel level, string name, string message, Exception exception)
        {
            var logEvent = new LogEventInfo(level, $"{typeof(T)}.{name}", message);

            if (exception != null)
            {
                logEvent.Exception = exception;
            }

            logEvent.Properties["correlationId"] = LogContext.InternalCorrelationId;
            logEvent.Properties["context"] = LogContext;

            LogManager.GetCurrentClassLogger().Log(logEvent);
        }
    }
}
