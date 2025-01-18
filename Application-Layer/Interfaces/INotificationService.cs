using Domain_Layer.Models;
using System;
using System.Threading.Tasks;

namespace Application_Layer.Interfaces
{
    public interface INotificationService
    {
        Task SendBookingNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType type,
            Guid bookingId
        );
    }
}
