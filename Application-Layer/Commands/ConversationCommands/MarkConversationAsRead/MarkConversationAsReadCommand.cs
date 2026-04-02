using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.ConversationCommands.MarkConversationAsRead
{
    public class MarkConversationAsReadCommand : IRequest<OperationResult<List<MarkedMessageResult>>>
    {
        public Guid ConversationId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class MarkedMessageResult
    {
        public Guid MessageId { get; set; }
        public DateTime ReadAt { get; set; }
    }
}
