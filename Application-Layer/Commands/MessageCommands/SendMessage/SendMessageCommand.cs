using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Commands.MessageCommands.SendMessage
{
    public class SendMessageCommand : IRequest<Guid>
    {
        public SendMessageDTO MessageDto { get; set; } = new();
    }
}
