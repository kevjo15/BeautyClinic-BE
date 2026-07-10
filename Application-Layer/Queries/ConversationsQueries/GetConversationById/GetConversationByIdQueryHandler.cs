using Application_Layer.Queries.ConversationsQueries.GetConversationById;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.ConversationsQueries.GetConversationById
{
    public class GetConversationByIdQueryHandler : IRequestHandler<GetConversationByIdQuery, ConversationDTO?>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IApplicationMapper _mapper;

        public GetConversationByIdQueryHandler(IConversationRepository conversationRepository, IApplicationMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _mapper = mapper;
        }

        public async Task<ConversationDTO?> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetByIdAsync(request.ConversationId);

            if (conversation == null)
                return null;

            return _mapper.ToConversationDto(conversation);
        }
    }
}
