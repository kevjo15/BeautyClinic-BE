using Application.Features.Conversations.Commands;
using Application_Layer.Interfaces;
using Domain_Layer.Models;
using MediatR;

namespace Application.Features.Conversations.Handlers
{
    public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationModel>
    {
        private readonly IConversationRepository _conversationRepository;

        public CreateConversationCommandHandler(IConversationRepository conversationRepository)
        {
            _conversationRepository = conversationRepository;
        }

        public async Task<ConversationModel> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
        {
            await _conversationRepository.CreateAsync(request.Conversation);
            return request.Conversation;
        }
    }
}