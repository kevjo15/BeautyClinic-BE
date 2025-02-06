using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Domain_Layer.Models;
using Application_Layer.DTOs;

namespace API_Layer.Hubs
{
    public interface INotificationClient
    {
        Task ReceiveNotification(NotificationDTO notification);
    }

    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            await base.OnDisconnectedAsync(exception);
        }

        internal static async Task SendNotificationToUser(
            IHubContext<NotificationHub, INotificationClient> hubContext,
            string userId,
            string title,
            string message,
            NotificationType type)
        {
            var notification = new NotificationDTO
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                UserId = userId
            };

            await hubContext.Clients.Group(userId).ReceiveNotification(notification);
        }


        public async Task SendBookingNotification(string userId, string title, string message, NotificationType type)
        {
            var notificationDto = new NotificationDTO
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                UserId = userId
            };

            await Clients.Group(userId).ReceiveNotification(notificationDto);
        }

    }
}
