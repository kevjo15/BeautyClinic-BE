using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.ConversationsQueries.GetConversationById
{
    public class GetConversationByIdQuery : IRequest<ConversationDTO?>
    {
        public Guid ConversationId { get; set; }
    }
}
