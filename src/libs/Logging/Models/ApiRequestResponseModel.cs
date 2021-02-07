using System;

namespace Logging.Models
{
    public class ApiRequestResponseModel
    { 
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
        public OutputFilterStream FilterStream { get; set; }
    }
}
