using Application.Features.Conversations.Commands;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Conversations.Handlers
{
    public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDTO>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMapper _mapper;

        public CreateConversationCommandHandler(IConversationRepository conversationRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<ConversationDTO> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
        {
            await _conversationRepository.CreateAsync(request.Conversation);
            return _mapper.Map<ConversationDTO>(request.Conversation);
        }
    }
}