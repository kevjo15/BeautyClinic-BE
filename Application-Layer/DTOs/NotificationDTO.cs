using Domain_Layer.Models;
using System;

namespace Application_Layer.DTOs
{
    public class NotificationDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public NotificationType Type { get; set; }
        public Guid? BookingId { get; set; }
        public string UserId { get; set; }
    }
}
