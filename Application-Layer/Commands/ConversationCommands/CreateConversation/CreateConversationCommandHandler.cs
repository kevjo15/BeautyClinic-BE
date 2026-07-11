using Application_Layer.Commands.ConversationCommands.CreateConversation;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Commands.ConversationCommands.CreateConversation
{
    public class CreateConversationCommandHandler : IRequestHandler<CreateConversationCommand, ConversationDTO>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IApplicationMapper _mapper;

        public CreateConversationCommandHandler(IConversationRepository conversationRepository, IApplicationMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<ConversationDTO> Handle(CreateConversationCommand request, CancellationToken cancellationToken)
        {
            await _conversationRepository.CreateAsync(request.Conversation);
            return _mapper.ToConversationDto(request.Conversation);
        }
    }
}
