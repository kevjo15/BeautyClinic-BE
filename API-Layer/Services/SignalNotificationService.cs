using Application_Layer.Interfaces;
using Domain_Layer.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using API_Layer.Hubs;
using Application_Layer.DTOs;
using MediatR;

namespace API_Layer.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;

        public SignalRNotificationService(
            IHubContext<NotificationHub, INotificationClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendBookingNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            Guid bookingId)
        {
            // Bygg en notifikations-DTO
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

            // Skicka notisen till användarens grupp
            await _hubContext.Clients.Group(userId).ReceiveNotification(notificationDto);
        }
    }
}
