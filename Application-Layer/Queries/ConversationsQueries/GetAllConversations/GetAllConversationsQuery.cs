using Application_Layer.DTOs;
using MediatR;

namespace Application.Features.Conversations.Queries
{
    public class GetAllConversationsQuery : IRequest<List<ConversationDTO>> { }
}
