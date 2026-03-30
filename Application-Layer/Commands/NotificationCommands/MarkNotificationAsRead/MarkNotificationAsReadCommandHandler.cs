using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, OperationResult>
    {
        private readonly INotificationRepository _notificationRepository;

        public MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<OperationResult> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
            if (notification == null || notification.UserId != request.UserId)
            {
                return OperationResult.Failure("Notification not found or does not belong to this user");
            }

            await _notificationRepository.MarkAsReadAsync(request.NotificationId);

            return OperationResult.Success();
        }
    }
}
