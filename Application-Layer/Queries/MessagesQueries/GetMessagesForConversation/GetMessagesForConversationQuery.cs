using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.MessagesQueries.GetMessagesForConversation
{
    public class GetMessagesForConversationQuery : IRequest<List<MessageDTO>>
    {
        public Guid ConversationId { get; set; }
    }
}
