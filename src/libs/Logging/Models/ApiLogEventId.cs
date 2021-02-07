namespace Logging.Models
{
    public static class ApiLogEventId
    {
        private const int HttpRequestBase = 1000;
        public const int RequestStart = HttpRequestBase + 1;
        public const int RequestEnd = HttpRequestBase + 2;
        public const int RequestError = HttpRequestBase + 3;
    }
}
