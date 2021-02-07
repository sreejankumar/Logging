using System.Collections.Generic;

namespace Logging.Constants
{
    public static class WebConstants
    {
        public const string ParentObject = "parentObject";
        public const string RequestUserSecurityProfile = "UserSecurityProfile";

       
        public const string PatchMethod = "PATCH";
        public const string RequestGuid = "RequestGuid";
        public const string RequestIdHeaderKey = "x-request-id";

        public const string AuditTrailCloudCall = "AuditTrailCloudCall";

        public const string LogContextDictionary = "LogContextDictionary";
        public const string RequestControllerName = "RequestControllerName";
        public const string RequestActionName = "RequestActionName";

       public static readonly IList<string> ExcludedSensitiveHeadersIndexOf = new List<string> { "authorization" };
    }
}
