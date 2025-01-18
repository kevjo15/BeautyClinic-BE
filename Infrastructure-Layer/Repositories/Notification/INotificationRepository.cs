using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Repositories.Notification
{
    public interface INotificationRepository
    {
        Task<NotificationModel> CreateAsync(NotificationModel notification);
        Task<List<NotificationModel>> GetUserNotificationsAsync(string userId);
        Task<NotificationModel?> GetByIdAsync(Guid id);
        Task<NotificationModel> MarkAsReadAsync(Guid id);
        Task<List<NotificationModel>> GetUnreadNotificationsAsync(string userId);
        Task DeleteAsync(Guid id);
    }
}
