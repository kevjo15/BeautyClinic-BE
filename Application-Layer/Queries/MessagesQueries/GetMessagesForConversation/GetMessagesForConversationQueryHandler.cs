using Application_Layer.Queries.MessagesQueries.GetMessagesForConversation;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.MessagesQueries.GetMessagesForConversation
{
    public class GetMessagesForConversationQueryHandler : IRequestHandler<GetMessagesForConversationQuery, List<MessageDTO>>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IApplicationMapper _mapper;

        public GetMessagesForConversationQueryHandler(IConversationRepository conversationRepository, IApplicationMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<List<MessageDTO>> Handle(GetMessagesForConversationQuery request, CancellationToken cancellationToken)
        {
            var messages = await _conversationRepository.GetMessagesAsync(request.ConversationId);
            return _mapper.ToMessageDtoList(messages);
        }
    }
}
