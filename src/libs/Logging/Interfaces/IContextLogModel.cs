using System;

namespace Logging.Interfaces
{
    public interface IContextLogModel
    {
        /// <summary>
        /// Id of the external message received.
        /// For eg: SQS: MessageId, Lambda EventId, or unique identifier consuming message
        /// </summary>
        Guid ExternalCorrelationId { get; set; }
        /// <summary>
        /// Identifier for process inside your service.
        /// </summary>
        Guid InternalCorrelationId { get; }
    }
}
