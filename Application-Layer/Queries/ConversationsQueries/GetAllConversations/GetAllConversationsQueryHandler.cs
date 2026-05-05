using Application_Layer.Queries.ConversationsQueries.GetAllConversations;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using AutoMapper;
using MediatR;

namespace Application_Layer.Queries.ConversationsQueries.GetAllConversations
{
    public class GetAllConversationsQueryHandler : IRequestHandler<GetAllConversationsQuery, List<ConversationDTO>>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMapper _mapper;

        public GetAllConversationsQueryHandler(IConversationRepository conversationRepository, IMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<List<ConversationDTO>> Handle(GetAllConversationsQuery request, CancellationToken cancellationToken)
        {
            var conversations = await _conversationRepository.GetAllAsync();
            return _mapper.Map<List<ConversationDTO>>(conversations);
        }
    }
}
