using System;
using System.Collections.Generic;
using System.Text;
using Logging.Constants;
using Logging.Interfaces;
using Logging.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using NLog;
using NLog.LayoutRenderers;
using NLog.Web.LayoutRenderers;

namespace Logging.Renderer
{
   /// <summary>
    /// This class generates a partial JSON string so that it can be used in a JSON layout template
    /// Ensure that the json field name and values are properly escaped to prevent breaking the original JSON line layout!
    /// 
    /// </summary>
    [LayoutRenderer(JsonDataRendererConstants.JsonDataRendererKey)]
    public class JsonDataRenderer : AspNetLayoutRendererBase
    {
        private const string Delimiter = ", ";

        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            DoAppend(builder, logEvent);
        }

        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            var payloadType = JsonDataRendererConstants.PayloadTypeText;
            var ignoreHttpContext = false;
            var eventId = 0;
            Dictionary<string, object> logContextDictionary = null;

            if (logEvent.Properties != null)
            {
                if (logEvent.Properties.ContainsKey(JsonDataRendererConstants.EventIdKey))
                {
                    if (logEvent.Properties[JsonDataRendererConstants.EventIdKey] is int id) eventId = id;
                }

                if (logEvent.Properties.ContainsKey(JsonDataRendererConstants.PayloadTypeKey))
                {
                    payloadType = logEvent.Properties[JsonDataRendererConstants.PayloadTypeKey] as string ??
                                  payloadType;
                }

                if (logEvent.Properties.ContainsKey(JsonDataRendererConstants.IgnoreHttpContextKey))
                {
                    ignoreHttpContext = (bool)logEvent.Properties[JsonDataRendererConstants.IgnoreHttpContextKey];
                }

                if (logEvent.Properties.ContainsKey(JsonDataRendererConstants.LogContextDictionaryKey))
                {
                    logContextDictionary =
                        logEvent.Properties[JsonDataRendererConstants.LogContextDictionaryKey] as
                            Dictionary<string, object>;
                }
            }

            AddTag(builder);

            AddField(builder, JsonDataRendererConstants.EventIdKey, eventId);
            builder.Append(Delimiter);

            var httpContext = HttpContextAccessor?.HttpContext;
            if (!ignoreHttpContext && httpContext != null)
            {
                AddRequestGuid(builder, httpContext);
                AddControllerActionInfo(builder, httpContext);
            }

            if (logContextDictionary != null && logContextDictionary.Count > 0)
            {
                AddAdditionalLogContext(builder, logContextDictionary);
            }

            if (logEvent.Properties != null)
            {
                if (logEvent.Properties.ContainsKey("correlationId"))
                    AddCorrelationId(builder, (Guid?)logEvent.Properties["correlationId"]);

                if (logEvent.Properties.ContainsKey("context"))
                {
                    AddServiceContextLogModel(builder, (IContextLogModel)logEvent.Properties["context"]);
                }
            }

            AddPayloadData(builder, logEvent, payloadType);

            if (logEvent.Exception != null)
            {
                builder.Append(Delimiter);
                AddException(builder, logEvent);
            }
        }

        private static void AddTag(StringBuilder builder)
        {
            if (GlobalDiagnosticsContext.Contains(JsonDataRendererConstants.GdcTag))
            {
                var tag = GlobalDiagnosticsContext.Get(JsonDataRendererConstants.GdcTag);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    AddField(builder, JsonDataRendererConstants.GdcTag, tag, true);
                    builder.Append(Delimiter);
                }
            }
        }

        private static void AddServiceContextLogModel(StringBuilder builder, IContextLogModel baseLogModel)
        {
            AddField(builder, JsonDataRendererConstants.ContextDataKey, JsonConvert.SerializeObject(baseLogModel));
            builder.Append(Delimiter);
        }

        private static void AddCorrelationId(StringBuilder builder, Guid? correlationId)
        {
            AddField(builder, JsonDataRendererConstants.InternalCorrelationIdKey, correlationId, true);
            builder.Append(Delimiter);
        }

        private static void AddPayloadData(StringBuilder builder, LogEventInfo logEvent, string payloadType)
        {
            AddField(builder, JsonDataRendererConstants.PayloadTypeKey, payloadType, true);
            builder.Append(", ");

            var payload = payloadType == JsonDataRendererConstants.PayloadTypeText ? JsonConvert.ToString(logEvent.FormattedMessage) : logEvent.FormattedMessage;
            AddField(builder, JsonDataRendererConstants.PayloadKey, payload);
        }

        private static void AddRequestGuid(StringBuilder builder, HttpContext httpContext)
        {
            httpContext.Items.TryGetValue(WebConstants.RequestGuid, out var guidString);

            if (Guid.TryParse(guidString as string, out var guid))
            {
                AddField(builder, JsonDataRendererConstants.RequestGuidKey, guid.ToString(), true);
                builder.Append(Delimiter);
            }
        }

        private static void AddControllerActionInfo(StringBuilder builder, HttpContext httpContext)
        {
            var actionName = httpContext.GetRouteValue("action")?.ToString();
            var controllerName = httpContext.GetRouteValue("controller")?.ToString();

            if (string.IsNullOrEmpty(actionName) && string.IsNullOrEmpty(controllerName)) return;

            AddField(builder, JsonDataRendererConstants.ActionNameKey, actionName, true);
            builder.Append(Delimiter);
            AddField(builder, JsonDataRendererConstants.ControllerNameKey, controllerName, true);
            builder.Append(Delimiter);
        }

        private static void AddAdditionalLogContext(StringBuilder builder, Dictionary<string, object> logContext)
        {
            if (logContext != null)
            {
                foreach (var kv in logContext)
                {
                    var key = kv.Key;
                    var val = kv.Value;
                    var isNotPrimitive = !val?.GetType().IsPrimitive ?? false;
                    if (isNotPrimitive || val == null)
                    {
                        var data = kv.Value as string ?? kv.Value?.ToString() ?? string.Empty;
                        val = JsonConvert.ToString(data);
                    }

                    AddField(builder, key, val);
                    builder.Append(Delimiter);
                }
            }
        }

        private static void AddException(StringBuilder builder, LogEventInfo logEvent)
        {
            var exData = new ExceptionData(logEvent.Exception);
            var json = JsonConvert.SerializeObject(exData, new JsonSerializerSettings() { Formatting = Formatting.None });

            AddField(builder, JsonDataRendererConstants.FullStackTraceKey, json);
        }

        private static void AddField(StringBuilder builder, string key, object value, bool quoteValue = false)
        {
            builder.Append("\"");
            builder.Append(key);
            builder.Append("\"");
            builder.Append(" : ");

            if (quoteValue)
            {
                builder.Append("\"");
            }

            if (value == null)
            {
                builder.Append("null");
            }
            else
            {
                builder.Append(value);
            }


            if (quoteValue)
            {
                builder.Append("\"");
            }
        }
    }
}