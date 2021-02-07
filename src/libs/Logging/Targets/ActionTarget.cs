using System;
using NLog;
using NLog.Targets;

namespace Logging.Targets
{
    public class ActionTarget : TargetWithLayout
    {
        readonly Action<LogEventInfo> _action;
        public ActionTarget(Action<LogEventInfo> action) : base()
        {
            _action = action;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionTarget" /> class.
        /// </summary>
        /// <remarks>
        /// The default value of the layout is: <code>${longdate}|${level:uppercase=true}|${logger}|${message}</code>
        /// </remarks>
        /// <param name="name">Name of the target.</param>
        public ActionTarget(string name, Action<LogEventInfo> action) : this(action)
        {
            Name = name;
        }
        /// <summary>
        /// Writes the specified logging event to the <see cref="System.Diagnostics.Trace"/> facility.
        /// If the log level is greater than or equal to <see cref="LogLevel.Error"/> it uses the
        /// <see cref="System.Diagnostics.Trace.Fail(string)"/> method, otherwise it uses
        /// <see cref="System.Diagnostics.Trace.Write(string)" /> method.
        /// </summary>
        /// <param name="logEvent">The logging event.</param>
        protected override void Write(LogEventInfo logEvent)
        {
            _action?.Invoke(logEvent);
        }
    }
}