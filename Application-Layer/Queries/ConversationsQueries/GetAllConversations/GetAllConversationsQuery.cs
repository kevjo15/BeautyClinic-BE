using MediatR;
using Domain_Layer.Models;
using System.Collections.Generic;

namespace Application.Features.Conversations.Queries
{
    public class GetAllConversationsQuery : IRequest<List<ConversationModel>> { }
}
