using Logging.Configuration;
using Logging.Output;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Logging.Extensions
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Add console logging.
        /// </summary>
        /// <param name="services"></param>
        public static void AddConsoleLogging(this IServiceCollection services)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            services.AddSingleton(loggerFactory);
            services.AddLogging(logging => { logging.AddNLog(LogOutput.RegisterLogTargets()); });
            services.AddScoped(typeof(Interfaces.ILogger<>), typeof(Logger<>));
        }

        /// <summary>
        /// Add Network logging, passing logs to a tcp/udp endpoint.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="networkLogConfiguration"></param>
        public static void AddNetworkLogging(this IServiceCollection services,
            NetworkLogConfiguration networkLogConfiguration)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            services.AddSingleton(loggerFactory);
            services.AddLogging(logging => { logging.AddNLog(LogOutput.RegisterLogTargets(networkLogConfiguration)); });
            services.AddScoped(typeof(Interfaces.ILogger<>), typeof(Logger<>));
        }

        /// <summary>
        /// Add Network logging, passing logs to a tcp/udp endpoint.
        /// Also notify by email if there are errors or warning.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="networkLogConfiguration"></param>
        /// <param name="emailLogConfiguration"></param>
        public static void AddNetworkLoggingWithErrorEmailNotify(this IServiceCollection services,
            NetworkLogConfiguration networkLogConfiguration, EmailLogConfiguration emailLogConfiguration)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            services.AddSingleton(loggerFactory);
            services.AddLogging(logging => { logging.AddNLog(LogOutput.RegisterLogTargets(networkLogConfiguration, emailLogConfiguration)); });
            services.AddScoped(typeof(Interfaces.ILogger<>), typeof(Logger<>));
        }
    }
}
