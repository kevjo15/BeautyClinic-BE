using Microsoft.AspNetCore.SignalR;
using MediatR;
using Application.Features.MessageCommands.SendMessage;
using ApplicationLayer.DTOs;
using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Domain_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Application_Layer.Interfaces;

namespace API_Layer.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHub;
        private readonly IConversationRepository _conversationRepository;

        public ChatHub(
            IMediator mediator,
            IHubContext<NotificationHub, INotificationClient> notificationHub,
            IConversationRepository conversationRepository)
        {
            _mediator = mediator;
            _notificationHub = notificationHub;
            _conversationRepository = conversationRepository;
        }

        public async Task SendMessage(string message, Guid conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            // Verifiera att användaren är en del av konversationen
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                throw new HubException("Conversation not found");
            }

            if (!conversation.ParticipantIds.Any(id => id.ToString() == userId))
            {
                throw new HubException("User is not a participant in this conversation");
            }

            var messageDto = new SendMessageDTO
            {
                SenderId = Guid.Parse(userId), // Konvertera till Guid för SendMessageDTO
                Content = message,
                ConversationId = conversationId,
                SentAt = DateTime.UtcNow
            };

            var messageCommand = new SendMessageCommand { MessageDto = messageDto };
            var messageResult = await _mediator.Send(messageCommand);

            // Hitta mottagaren
            var recipientId = conversation.ParticipantIds.FirstOrDefault(id => id.ToString() != userId);
            if (recipientId != Guid.Empty)
            {
                // Skapa notifikation i databasen
                var notificationCommand = new CreateNotificationCommand 
                { 
                    Title = "New Message",
                    Message = message,
                    UserId = recipientId.ToString(),
                    Type = NotificationType.MessageReceived
                };
                var notificationResult = await _mediator.Send(notificationCommand);

                // Skicka real-time notifikation via NotificationHub
                await NotificationHub.SendNotificationToUser(
                    _notificationHub,
                    recipientId.ToString(),
                    "New Message",
                    message,
                    NotificationType.MessageReceived
                );
            }

            // Broadcast meddelandet till alla anslutna klienter i konversationen
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", messageDto);
        }

        public async Task JoinConversation(Guid conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            // Verifiera att användaren är en del av konversationen
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                throw new HubException("Conversation not found");
            }

            if (!conversation.ParticipantIds.Any(id => id.ToString() == userId))
            {
                throw new HubException("User is not a participant in this conversation");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
            await Clients.Caller.SendAsync("JoinedConversation", conversationId);
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            // Verifiera att användaren är en del av konversationen
            var conversation = await _conversationRepository.GetByIdAsync(conversationId);
            if (conversation == null)
            {
                throw new HubException("Conversation not found");
            }

            if (!conversation.ParticipantIds.Any(id => id.ToString() == userId))
            {
                throw new HubException("User is not a participant in this conversation");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
            await Clients.Caller.SendAsync("LeftConversation", conversationId);
        }
    }
}
