using System;
using System.Collections.Generic;
using System.Linq;
using Logging.Constants;
using Logging.Middleware;
using Newtonsoft.Json;
using NLog;

namespace Logging.Utility
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerSettings JsonSerializerSettingsNoFormatting =
            new JsonSerializerSettings() {Formatting = Formatting.None};

        public static void LogJson<T>(Logger logger, int eventId, T payload, NLog.LogLevel logLevel = null,
            Dictionary<string, object> logContextDictionary = null, Exception exception = null,
            bool ignoreHttpContext = false) where T : class
        {
            if (logger == null) return;
            if (!logger.IsDebugEnabled) return;

            try
            {
                var json = JsonConvert.SerializeObject(payload, JsonSerializerSettingsNoFormatting);

                LogJson(logger, eventId, json, logLevel, logContextDictionary, exception, ignoreHttpContext);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, nameof(RequestResponseLoggingMiddleware));
            }
        }
        
        private static void LogJson(Logger logger, int eventId, string payload, NLog.LogLevel logLevel = null,
            Dictionary<string, object> logContextDictionary = null, Exception exception = null,
            bool ignoreHttpContext = false)
        {
            if (logger == null) return;
            if (!logger.IsDebugEnabled) return;

            LogJson(logger, eventId, payload, logLevel, logContextDictionary, ignoreHttpContext, exception,
                JsonDataRendererConstants.PayloadTypeText);
        }
        
        private static void LogJson(Logger logger, int eventId, string payload, NLog.LogLevel logLevel,
            Dictionary<string, object> logContextDictionary, bool ignoreHttpContext, Exception exception,
            string payloadType)
        {
            if (logger == null) return;

            var logEntry = new LogEventInfo(logLevel ?? NLog.LogLevel.Debug, logger.Name, payload);
            logEntry.Properties.Add(JsonDataRendererConstants.EventIdKey, eventId);
            logEntry.Properties.Add(JsonDataRendererConstants.PayloadTypeKey, payloadType);
            logEntry.Exception = exception;

            if (logContextDictionary != null)
            {
                logEntry.Properties.Add(JsonDataRendererConstants.LogContextDictionaryKey, logContextDictionary);

                foreach (var (key, value) in logContextDictionary.Where(x => !logEntry.Properties.ContainsKey(x.Key)))
                {
                    logEntry.Properties.Add(key, value);
                }
            }

            if (ignoreHttpContext)
            {
                logEntry.Properties.Add(JsonDataRendererConstants.IgnoreHttpContextKey, true);
            }

            logger.Log(logEntry);
        }
    }
}