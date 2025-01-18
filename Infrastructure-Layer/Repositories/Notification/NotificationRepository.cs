using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure_Layer.Repositories.Notification
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ElsaBeautyDbContext _context;

        public NotificationRepository(ElsaBeautyDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationModel> CreateAsync(NotificationModel notification)
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.IsRead = false;
            
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
            
            return notification;
        }

        public async Task<List<NotificationModel>> GetUserNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Include(n => n.Booking)
                .ToListAsync();
        }

        public async Task<NotificationModel?> GetByIdAsync(Guid id)
        {
            return await _context.Notifications
                .Include(n => n.Booking)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<NotificationModel> MarkAsReadAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                throw new KeyNotFoundException($"Notification with ID {id} not found");

            notification.IsRead = true;
            
            await _context.SaveChangesAsync();
            return notification;
        }

        public async Task<List<NotificationModel>> GetUnreadNotificationsAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Include(n => n.Booking)
                .ToListAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                throw new KeyNotFoundException($"Notification with ID {id} not found");

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }
    }
}
