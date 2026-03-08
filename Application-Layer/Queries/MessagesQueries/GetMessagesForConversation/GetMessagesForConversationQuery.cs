using Application_Layer.DTOs;
using MediatR;

namespace Application.Features.Conversations.Queries
{
    public class GetMessagesForConversationQuery : IRequest<List<MessageDTO>>
    {
        public Guid ConversationId { get; set; }
    }
}
