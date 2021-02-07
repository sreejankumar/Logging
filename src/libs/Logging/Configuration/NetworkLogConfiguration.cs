
namespace Logging.Configuration
{
    public class NetworkLogConfiguration
    {
        public int NetworkConnectionCacheSize { get; set; } = 1;
        public bool NetworkLogOverflowDiscard { get; set; } = true;
        public int NetworkLogQueueLimit { get; set; } = 1000;
        public int NetworkLogMaxMessageSize { get; set; } = 6400000;
        /// <summary>
        /// Disable console logging.
        /// </summary>
        public bool DisableConsoleLogging { get; set; } = false;
        /// <summary>
        /// The Log Address : usually UDP to Log stash buffer
        /// </summary>
        public string NetworkLogAddress { get; set; }
        /// <summary>
        /// Network Log Source : AWS-Staging, AWS-London, AWS-Boston
        /// </summary>
        public string NetworkLogSource { get; set; }
        /// <summary>
        /// Index name in Log stash where you want to see the logs.
        /// </summary>
        public string NetworkLogTag { get; set; }
    }
}
