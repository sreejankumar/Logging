using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NLog;

namespace Logging.Middleware
{
    public abstract class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Logger _logger;

        protected RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task Invoke(HttpContext context)
        {
            if (_logger != null)
            {
                await LoggingMiddlewareHelper.LogApiRequestStart(_logger, context);
            }
            
            await _next(context);
            
            if (_logger != null)
            {
                await LoggingMiddlewareHelper.LogApiRequestEnd(_logger, context, null);
            }
        }
    }
}
