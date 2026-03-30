using Application_Layer.DTOs;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands.CreateNotification
{
    public class CreateNotificationCommand : IRequest<OperationResult<NotificationDTO>>
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public NotificationType Type { get; set; }
        public Guid? BookingId { get; set; }
    }
}
