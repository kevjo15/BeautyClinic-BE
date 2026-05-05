using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.ConversationCommands.MarkConversationAsRead
{
    public class MarkConversationAsReadCommandHandler
        : IRequestHandler<MarkConversationAsReadCommand, OperationResult<List<MarkedMessageResult>>>
    {
        private readonly IConversationRepository _conversationRepository;

        public MarkConversationAsReadCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<OperationResult<List<MarkedMessageResult>>> Handle(
            MarkConversationAsReadCommand request,
            CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);
            if (conversation == null)
                return OperationResult<List<MarkedMessageResult>>.Failure("Conversation not found", Domain_Layer.Common.OperationFailureType.NotFound);

            if (!conversation.ParticipantIds.Any(id => id.ToString() == request.UserId))
                return OperationResult<List<MarkedMessageResult>>.Failure("User is not a participant", Domain_Layer.Common.OperationFailureType.Forbidden);

            var now = DateTime.UtcNow;
            var marked = new List<MarkedMessageResult>();

            foreach (var msg in conversation.Messages)
            {
                if (msg.SenderId.ToString() != request.UserId && msg.ReadAt == null)
                {
                    msg.ReadAt = now;
                    marked.Add(new MarkedMessageResult { MessageId = msg.Id, ReadAt = now });
                }
            }

            if (marked.Count > 0)
                await _conversationRepository.UpdateAsync(conversation);

            return OperationResult<List<MarkedMessageResult>>.Success(marked);
        }
    }
}
