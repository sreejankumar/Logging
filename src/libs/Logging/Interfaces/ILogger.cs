using System;
using System.Runtime.CompilerServices;

namespace Logging.Interfaces
{
    public interface ILogger<out T>
    {
        IContextLogModel LogContext { get; }

        void LogInfo(string info, [CallerMemberName] string callerMemberName = null);
        void LogDebug(string debug, [CallerMemberName] string callerMemberName = null);
        void LogWarn(string warn, [CallerMemberName] string callerMemberName = null);
        void LogError(string error, Exception ex, [CallerMemberName] string callerMemberName = null);
    }
}
