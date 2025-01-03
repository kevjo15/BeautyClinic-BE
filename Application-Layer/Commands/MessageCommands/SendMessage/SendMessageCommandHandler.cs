using Application.Common.Interfaces;
using Domain_Layer.Models;
using MediatR;
using AutoMapper;
using Infrastructure_Layer.Repositories.Conversation;

namespace Application.Features.MessageCommands.SendMessage
{
    public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Guid>
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public SendMessageCommandHandler(IConversationRepository conversationRepository, IMessageRepository messageRepository, IMapper mapper)
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

            var message = _mapper.Map<MessageModel>(request.MessageDto);
            message.SenderId = request.MessageDto.SenderId;

            await _messageRepository.CreateAsync(message);

            conversation.LastMessageAt = message.SentAt;
            await _conversationRepository.UpdateAsync(conversation);

            return message.Id;
        }
    }
}
