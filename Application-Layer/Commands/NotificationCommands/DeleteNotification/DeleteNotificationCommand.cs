using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands
{
    public class DeleteNotificationCommand : IRequest<OperationResult>
    {
        public Guid NotificationId { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
