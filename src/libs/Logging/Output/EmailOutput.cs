using System;
using System.Linq;
using System.Net;
using System.Text;
using Logging.Configuration;
using Logging.Extensions;
using NLog.Config;
using NLog.MailKit;

namespace Logging.Output
{
    public static partial class LogOutput
    {
        private static void EmailOutput(LoggingConfiguration config, EmailLogConfiguration appConfig)
        {
            if (!appConfig.LogToEmail) return;
            
            if (!appConfig.FromEmail.HasValue() || !appConfig.ToEmail.Any())
            {
                throw new Exception($"FromEmail: {appConfig.FromEmail} or " +
                                    $"ToEmail : {appConfig.ToEmail}.");
            }

            var mailTarget = new MailTarget
            {
                Encoding = Encoding.UTF8,
                Body = appConfig.EmailLayout,
                Subject = appConfig.SubjectLayout,
                From = appConfig.FromEmail,
                To = appConfig.ToEmail.ToCsv(),
                EnableSsl = appConfig.EnableSsl,
                SmtpAuthentication = appConfig.SmtpCredentialsRequired ? SmtpAuthenticationMode.Basic : SmtpAuthenticationMode.None,
                SmtpPort = appConfig.SmtpPort,
                SmtpServer = appConfig.SmtpHost,
                Layout = appConfig.EmailLayout,
                SkipCertificateValidation = true,
            };
            if (appConfig.SmtpCredentialsRequired)
            {
                mailTarget.SmtpPassword = appConfig.SmtpPassword;
                mailTarget.SmtpUserName = appConfig.SmtpUsername;
            }

            if (appConfig.CarbonCopy != null && appConfig.CarbonCopy.Any())
            {
                mailTarget.Cc = appConfig.CarbonCopy.ToCsv(";");
            }

            if (appConfig.BlindCarbonCopy != null && appConfig.BlindCarbonCopy.Any())
            {
                mailTarget.Bcc = appConfig.BlindCarbonCopy.ToCsv(";");
            }

            config.AddTarget("mail", mailTarget);
            if (appConfig.TurnWarnEmailAlert)
            {
                config.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Warn, mailTarget));
            }
            config.LoggingRules.Add(new LoggingRule("*", appConfig.EmailLogLevel, mailTarget));

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        }
    }
}