using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Logging.Constants;
using Logging.Extensions;
using Logging.Models;
using Logging.Utility;
using Microsoft.AspNetCore.Http;
using NLog;

namespace Logging.Middleware
{
    public static class LoggingMiddlewareHelper
    {
        //We don't need to overload the logstash buffers and filters. We need to we can increase it
        private const int ResponseBodyTruncateLimit = 16000;

        /// <summary>
        /// Log the request start.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task LogApiRequestStart(Logger logger, HttpContext context)
        {
            try
            {
                var requestGuid = Guid.NewGuid();
                var requestId = requestGuid.ToString();
                var requestBody = await FormatRequest(context.Request);

                var apiRequestModel = new ApiRequestResponseModel
                {
                    RequestDate = DateTime.UtcNow,
                    Authority = context.Request.Host.Host,
                    LocalPath = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.ToString() : null,
                    RequestGuid = requestGuid,
                    RequestBody = requestBody,
                    HttpMethod = context.Request.Method,
                    MachineName = Environment.MachineName,
                    RemoteAddress = context.Connection.RemoteIpAddress?.MapToIPv6().ToString(),
                    XForwardedFor = context.Connection.LocalIpAddress?.MapToIPv6().ToString(),
                    FilterStream = new OutputFilterStream(context),
                    Username = GetUsernameFromHeader(context)
                };
                GetDetailsFromClaimsIdentity(context, apiRequestModel);
                context.Items.Add(WebConstants.AuditTrailCloudCall, apiRequestModel);
                context.Items.Add(WebConstants.RequestGuid, requestId);
                context.Response.Headers.Add(WebConstants.RequestIdHeaderKey, requestId);
                if (apiRequestModel.QueryString?.Length > 0)
                {
                    var queryString = HttpUtility.ParseQueryString(apiRequestModel.QueryString);
                    if (queryString["jwt"] != null)
                        queryString["jwt"] = "*";

                    apiRequestModel.QueryString = $"?{queryString}";
                }

                var timestampString = apiRequestModel.RequestDate.ToString("o");
                var logContext = new Dictionary<string, object>()
                {
                    {"request_date", timestampString},
                    {"authority", apiRequestModel.Authority},
                    {"local_path", apiRequestModel.LocalPath},
                    {"query", apiRequestModel.QueryString},
                    {"remote_address", apiRequestModel.RemoteAddress},
                    {"x_forwarded_for", apiRequestModel.XForwardedFor},
                    {"http_method", apiRequestModel.HttpMethod}
                };
                var requestHeaders = context.Request.Headers;

                var obj = new
                {
                    request_date = timestampString,
                    request_headers = requestHeaders,
                    request_body = apiRequestModel.RequestBody
                };
                JsonHelper.LogJson(logger, ApiLogEventId.RequestStart, obj, LogLevel.Debug, logContext);
            }
            catch (Exception exception)
            {
                logger.Log(LogLevel.Error, exception, "Error Occured in LogAPI Start");
            }

        }

        /// <summary>
        /// This method is used to log the API request finish.
        /// For Errors, in your errorMiddleware, pass the exception.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static async Task LogApiRequestEnd(Logger logger, HttpContext context, Exception exception = null)
        {
            try
            {
                var apiRequestModel = (ApiRequestResponseModel)context.Items[WebConstants.AuditTrailCloudCall];
                if (apiRequestModel == null) return;

                var responseBody = await apiRequestModel.FilterStream.ReadStreamAsync(context.Response);
                apiRequestModel.ResponseBody = responseBody.Truncate(ResponseBodyTruncateLimit);

                apiRequestModel.HttpStatusCode = context.Response.StatusCode;
                apiRequestModel.RequestEnd = DateTime.UtcNow;
                apiRequestModel.RequestDuration =
                    (apiRequestModel.RequestEnd - apiRequestModel.RequestStart).TotalSeconds;

                var timestampString = apiRequestModel.RequestDate.ToString("o");
                var logContext = new Dictionary<string, object>()
                {
                    {"request_date", timestampString},
                    {"authority", apiRequestModel.Authority},
                    {"local_path", apiRequestModel.LocalPath},
                    {"query", apiRequestModel.QueryString},
                    {"remote_address", apiRequestModel.RemoteAddress},
                    {"x_forwarded_for", apiRequestModel.XForwardedFor},
                    {"http_method", apiRequestModel.HttpMethod},
                    {"username",apiRequestModel.Username}
                };
                var obj = new
                {
                    request_end = apiRequestModel.RequestEnd,
                    request_duration = apiRequestModel.RequestDuration,
                    http_status_code = apiRequestModel.HttpStatusCode,
                    response_body = apiRequestModel.ResponseBody
                };
                if (exception == null)
                {
                    JsonHelper.LogJson(logger, ApiLogEventId.RequestEnd, obj, LogLevel.Debug, logContext);
                }
                else
                {
                    JsonHelper.LogJson(logger, ApiLogEventId.RequestError, obj, LogLevel.Debug, logContext, exception);
                }
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, nameof(RequestResponseLoggingMiddleware));
            }
        }


        private static void GetDetailsFromClaimsIdentity(HttpContext context,
            ApiRequestResponseModel requestResponseModel)
        {
            var claims = context.User.Claims.ToList();
            int.TryParse(claims.FirstOrDefault(x => x.Type == "Id")?.Value, out var id);
            requestResponseModel.Id = id;

            int.TryParse(claims.FirstOrDefault(x => x.Type == "adminId")?.Value, out var adminId);
            requestResponseModel.AdminId = adminId;
        }

        private static async Task<string> FormatRequest(HttpRequest request)
        {
            //This line allows us to set the reader for the request back at the beginning of its stream.
            request.EnableBuffering();

            //We now need to read the request stream.  First, we create a new byte[] with the same length as the request stream...
            var buffer = new byte[Convert.ToInt32(request.ContentLength)];

            //...Then we copy the entire request stream into the new buffer.
            await request.Body.ReadAsync(buffer, 0, buffer.Length);

            //We convert the byte[] into a string using UTF8 encoding...
            var bodyAsText = Encoding.UTF8.GetString(buffer);

            //..and finally, assign the read body back to the request body, which is allowed because of EnableBuffering()
            request.Body.Seek(0, SeekOrigin.Begin);

            return bodyAsText;
        }

        private static string GetUsernameFromHeader(HttpContext context)
        {
            var username = context.Request.Headers["username"];
            return string.IsNullOrWhiteSpace(username) ? string.Empty : username.ToString();
        }
    }
}
