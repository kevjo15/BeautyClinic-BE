using MediatR;
using Domain_Layer.Models;
using Application.Features.Conversations.Queries;
using Application_Layer.Interfaces;

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
