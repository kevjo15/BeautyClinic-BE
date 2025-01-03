using ApplicationLayer.DTOs;
using MediatR;

namespace Application.Features.MessageCommands.SendMessage
{
    public class SendMessageCommand : IRequest<Guid>
    {
        public SendMessageDTO MessageDto { get; set; }
    }
}
