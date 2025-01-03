using Application.Common.Interfaces;
using MediatR;
using Domain_Layer.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure_Layer.Repositories.Conversation;
using Application.Features.Conversations.Queries;

namespace Application.Features.Conversations.Handlers
{
    public class GetMessagesForConversationQueryHandler : IRequestHandler<GetMessagesForConversationQuery, List<MessageModel>>
    {
        private readonly IConversationRepository _conversationRepository;

        public GetMessagesForConversationQueryHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<List<MessageModel>> Handle(GetMessagesForConversationQuery request, CancellationToken cancellationToken)
        {
            return await _conversationRepository.GetMessagesAsync(request.ConversationId); // Implement this method in the repository
        }
    }
}
