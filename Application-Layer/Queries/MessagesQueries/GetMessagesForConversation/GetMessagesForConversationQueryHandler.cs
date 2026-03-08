using Application.Features.Conversations.Queries;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Features.Conversations.Handlers
{
    public class GetMessagesForConversationQueryHandler : IRequestHandler<GetMessagesForConversationQuery, List<MessageDTO>>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMapper _mapper;

        public GetMessagesForConversationQueryHandler(IConversationRepository conversationRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<List<MessageDTO>> Handle(GetMessagesForConversationQuery request, CancellationToken cancellationToken)
        {
            var messages = await _conversationRepository.GetMessagesAsync(request.ConversationId);
            return _mapper.Map<List<MessageDTO>>(messages);
        }
    }
}
