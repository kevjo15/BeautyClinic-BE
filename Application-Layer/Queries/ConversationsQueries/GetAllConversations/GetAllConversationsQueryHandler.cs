using Application_Layer.Queries.ConversationsQueries.GetAllConversations;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.ConversationsQueries.GetAllConversations
{
    public class GetAllConversationsQueryHandler : IRequestHandler<GetAllConversationsQuery, List<ConversationDTO>>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IApplicationMapper _mapper;

        public GetAllConversationsQueryHandler(IConversationRepository conversationRepository, IApplicationMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<List<ConversationDTO>> Handle(GetAllConversationsQuery request, CancellationToken cancellationToken)
        {
            var conversations = await _conversationRepository.GetAllAsync();
            return _mapper.ToConversationDtoList(conversations);
        }
    }
}
