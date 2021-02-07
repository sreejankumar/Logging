using System.Text;
using Logging.Constants;
using NLog;
using NLog.LayoutRenderers;

namespace Logging.Renderer
{
    [LayoutRenderer(JsonDataRendererConstants.SequenceIdRendererKey)]
    public class SequenceIdRenderer : LayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            builder.Append(logEvent.SequenceID);
        }
    }
}