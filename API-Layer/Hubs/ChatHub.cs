using Microsoft.AspNetCore.SignalR;
using MediatR;
using Application.Features.MessageCommands.SendMessage;
using ApplicationLayer.DTOs;

namespace API_Layer.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMediator _mediator;

        public ChatHub(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task SendMessage(Guid userId, string message, Guid conversationId)
        {
            var messageDto = new SendMessageDTO
            {
                SenderId = userId,
                Content = message,
                ConversationId = conversationId,
                SentAt = DateTime.UtcNow
            };

            var command = new SendMessageCommand { MessageDto = messageDto };
            var result = await _mediator.Send(command);

            // Broadcast meddelandet till alla anslutna klienter i konversationen
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", messageDto);
        }

        public async Task JoinConversation(Guid conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            await Clients.Caller.SendAsync("JoinedConversation", conversationId);
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            await Clients.Caller.SendAsync("LeftConversation", conversationId);
        }
    }
}
