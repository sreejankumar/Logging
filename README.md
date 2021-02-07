# Logging
Logging abstraction with NLog
* Console Logging
* Netwrok Logging
* Error Email Notification
* Request Response Logging Middleware
* Error Logging Middleware

## Basic
To use, add the following in your startup class

For example:

```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConsoleLogging();
            services.AddScoped<IContextLogModel, LogContext>();           
        }
```

The LogContext is the class that you need to create inheriting the **IContextLogModel ** class. 
This is so application specific fields can be mapped in the logs.

Once you have performed the above step, you can now inject the **Ilogger<T>** where T is the class where you injecting the logger.

## Network Logging
### To have network config from Configuration file, you can use the following code in the start up 

```csharp
    var config = Configuration.GetSection(NetworkLogConfiguration.NetworkLogPrefix)
                    .Get<NetworkLogConfiguration>();
            services.AddNetworkLogging(config);
            services.AddScoped<IContextLogModel, ApiLogContext>();
```

### For config for network logging is in Enviroment variables, you can use the following code in the start up
```csharp

      var config = new NetworkLogConfiguration
            {
                NetworkLogAddress = "Log Address",
                NetworkLogSource = "LogSource",
                NetworkLogTag = "Name of index you want your logs in "
            };    
            services.AddNetworkLogging(config);
            services.AddScoped<IContextLogModel, ApiLogContext>();
```
**Please note, if you dont want to log to console(CloudWatch), you can disable it by specify the property in NetworkLogConfiguration Called DisableConsoleLogging to true.** 

## Network Logging with Error Email Notificaiton 

### To have network config and email config from Configuration file, you can use the following code in the start up 

```csharp
    var networkConfig = Configuration.GetSection(NetworkLogConfiguration.NetworkLogPrefix)
                    .Get<NetworkLogConfiguration>();
    var emailConfig = Configuration.GetSection(EmailLogConfiguration.EmailLogPrefix)
                    .Get<EmailLogConfiguration>();

            services.AddNetworkLoggingWithErrorEmailNotify(networkConfig,emailConfig);
            services.AddScoped<IContextLogModel, ApiLogContext>();
```

## For API
For logging API request and responses, there is an **RequestResponseLoggingMiddleware** that you can use in your API. 
The RequestResponseLoggingMiddleware logs the following properties 
```csharp
        public DateTime RequestDate { get; set; }
        public DateTime RequestStart { get; set; }
        public DateTime RequestEnd { get; set; }
        public double RequestDuration { get; set; }
        public string Username { get; set; }
        public long AdminId { get; set; }
        public long Id { get; set; }
        public int HttpStatusCode { get; set; }
        public string Authority { get; set; }
        public string LocalPath { get; set; }
        public string QueryString { get; set; }
        public string RemoteAddress { get; set; }
        public string XForwardedFor { get; set; }
        public Guid RequestGuid { get; set; }
        public string HttpMethod { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string MachineName { get; set; }
        public string SessionId { get; set; }
```
These properties are the common fields that is used by CloudCall in other API's. These common fields can be filtered in Logstash or in cloudwatch logs.

Regarding how the Logs can be identified easily is as follows
```csharp
        public const int RequestStart = 1001;
        public const int RequestEnd = 1002;
        public const int RequestError = 1003;
```

To use this please add the following in your starp up.
```csharp
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
          app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }

```
In your API, if you have got an ErrorMiddleware to handle the errors , you can use the below code in your ErrorMiddleware so that you API logs are compatiable with the rest of our API's'

```csharp
         public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static Logger _logger;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public async Task Invoke(HttpContext context /* other scoped dependencies */)
        {
            try
            {
                    await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }


        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var code = HttpStatusCode.InternalServerError; // 500 if unexpected

            if (exception is AuthException) code = HttpStatusCode.Unauthorized;
            else if (exception is ValidationException) code = HttpStatusCode.BadRequest;
            else if (exception is DeserialisationException) code = HttpStatusCode.BadRequest;
            else if (exception is CommandException) code = HttpStatusCode.InternalServerError;
            else if (exception is NotFoundException) code = HttpStatusCode.NotFound;
            else if (exception is PermissionDeniedException) code = HttpStatusCode.Forbidden;
            else if (exception is ExternalServiceException)
                code = ((ExternalServiceException)exception).HttpStatusCode;

            var result = JsonConvert.SerializeObject(new { error = exception.Message });
            context.Response.ContentType = MimeTypesConstants.Application.Json;
            context.Response.StatusCode = (int)code;


            await context.Response.WriteAsync(result);

            if (_logger != null)
            {
                await LoggingMiddlewareHelper.LogApiRequestEnd(_logger, context, exception);
            }
        }
    }
```
**Please remeber your ErrorMiddleware needs to top of your loggindMiddleware when you configure it.** 
For example
```csharp
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
             app.UseMiddleware<ErrorMiddleware>()
             app.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
```
