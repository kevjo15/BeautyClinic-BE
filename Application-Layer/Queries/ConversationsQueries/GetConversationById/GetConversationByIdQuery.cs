using Application_Layer.DTOs;
using MediatR;

namespace Application.Features.Conversations.Queries
{
    public class GetConversationByIdQuery : IRequest<ConversationDTO?>
    {
        public Guid ConversationId { get; set; }
    }
}
