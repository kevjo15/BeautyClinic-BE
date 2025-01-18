using MediatR;
using Domain_Layer.Models;

namespace Application_Layer.Commands.NotificationCommands.CreateNotification
{
    public class CreateNotificationCommand : IRequest<CreateNotificationResult>
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public NotificationType Type { get; set; }
        public Guid? BookingId { get; set; }
    }
}
