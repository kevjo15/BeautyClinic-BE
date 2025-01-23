using Application_Layer.Interfaces;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands
{
    public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Unit>
    {
        private readonly INotificationRepository _notificationRepository;

        public DeleteNotificationCommandHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Unit> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
            if (notification == null || notification.UserId != request.UserId)
            {
                throw new KeyNotFoundException("Notification not found or does not belong to this user");
            }

            await _notificationRepository.DeleteAsync(request.NotificationId);

            return Unit.Value;
        }
    }
}
