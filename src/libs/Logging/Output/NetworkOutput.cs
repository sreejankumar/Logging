using System;
using System.Text;
using System.Text.RegularExpressions;
using Logging.Configuration;
using Logging.Constants;
using Logging.Extensions;
using Logging.Layout;
using Logging.Models;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;

namespace Logging.Output
{
    public static partial class LogOutput
    {
        private static void NetworkOutput(LoggingConfiguration config, NetworkLogConfiguration appConfig)
        {
            if (!appConfig.NetworkLogAddress.HasValue() || !appConfig.NetworkLogSource.HasValue() ||
                !appConfig.NetworkLogTag.HasValue())
            {
                throw new Exception($"NetworkLogAddress: {appConfig.NetworkLogAddress} or " +
                                    $"NetworkLogSource : {appConfig.NetworkLogSource} or" +
                                    $"NetworkLogTag : {appConfig.NetworkLogTag}.");
            }

            SetApplicationSourceName(appConfig.NetworkLogSource);

            if (appConfig.NetworkLogTag.HasValue())
            {
                var tag = appConfig.NetworkLogTag;
                const string pattern = "^[a-zA-Z0-9-_.]+$";
                var regex = new Regex(pattern);

                if (!regex.IsMatch(tag))
                {
                    throw new Exception(
                        $"The specified tag '${tag}' is invalid based on the following RegEx pattern: {pattern}");
                }

                GlobalDiagnosticsContext.Set(JsonDataRendererConstants.GdcTag, tag);
            }

            SetRendererKeys();

            var networkTarget = new NetworkTarget()
            {
                Encoding = Encoding.UTF8,
                Address = appConfig.NetworkLogAddress.TransformTokens(Environment.GetEnvironmentVariables()),
                NewLine = true,
                ConnectionCacheSize = appConfig.NetworkConnectionCacheSize,
                Layout = LogLayout.JsonLogLayout,
                MaxMessageSize = appConfig.NetworkLogMaxMessageSize
            };

            var wrapper = new AsyncTargetWrapper(networkTarget)
            {
                OverflowAction = appConfig.NetworkLogOverflowDiscard
                    ? AsyncTargetWrapperOverflowAction.Discard
                    : AsyncTargetWrapperOverflowAction.Block,
                QueueLimit = appConfig.NetworkLogQueueLimit
            };

            config.AddTarget("network", wrapper);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, wrapper));
        }
    }
}