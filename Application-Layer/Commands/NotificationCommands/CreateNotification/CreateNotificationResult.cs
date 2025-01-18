using System;

namespace Application_Layer.Commands.NotificationCommands.CreateNotification
{
    public class CreateNotificationResult
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
    }
}
