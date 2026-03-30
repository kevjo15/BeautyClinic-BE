using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Queries.ConversationsQueries.GetAllConversations
{
    public class GetAllConversationsQuery : IRequest<List<ConversationDTO>> { }
}
