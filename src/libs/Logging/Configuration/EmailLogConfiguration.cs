using Logging.Layout;

namespace Logging.Configuration
{
    public class EmailLogConfiguration
    {
        public const string EmailLogPrefix = "EmailLog";
        public bool LogToEmail { get; set; }
        public string EmailLayout { get; set; } = LogLayout.DefaultLayout;
        public NLog.LogLevel EmailLogLevel { get; set; } = NLog.LogLevel.Error;
        public bool TurnWarnEmailAlert { get; set; } = false;
        public string SubjectLayout { get; set; } = "[${gdc:item=platform}] [${machinename}] ${gdc:item=Source}";
        public string FromEmail { get; set; }
        public string[] ToEmail { get; set; }
        public string[] CarbonCopy { get; set; }
        public string[] BlindCarbonCopy { get; set; }
        public string SmtpHost { get; set; } = "mail";
        public int SmtpPort { get; set; }=25;
        public bool SmtpCredentialsRequired { get; set; } = false;
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool EnableSsl => SmtpCredentialsRequired;
    }
}