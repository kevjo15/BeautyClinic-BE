using Domain_Layer.Models;
using MediatR;
using Application_Layer.Mapping;
using Application_Layer.Interfaces;

namespace Application_Layer.Commands.MessageCommands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Guid>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IApplicationMapper _mapper;

        public SendMessageCommandHandler(IConversationRepository conversationRepository, IMessageRepository messageRepository, IApplicationMapper mapper)
        {
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            var conversation = await _conversationRepository.GetByIdAsync(request.MessageDto.ConversationId);
            if (conversation == null)
            {
                throw new Exception("Conversation not found");
            }

            var message = _mapper.ToMessageModel(request.MessageDto);
            message.SenderId = request.MessageDto.SenderId;

            await _messageRepository.CreateAsync(message);

            conversation.LastMessageAt = message.SentAt;
            await _conversationRepository.UpdateAsync(conversation);

            return message.Id;
        }
    }
}
