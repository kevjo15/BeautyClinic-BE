using MediatR;
using System;

namespace Application_Layer.Commands.NotificationCommands
{
    public class DeleteNotificationCommand : IRequest<Unit>
    {
        public Guid NotificationId { get; set; }
        public string UserId { get; set; }
    }
}
