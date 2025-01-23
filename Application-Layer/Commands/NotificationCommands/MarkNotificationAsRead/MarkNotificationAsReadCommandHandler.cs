using Application_Layer.Interfaces;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Unit>
    {
        private readonly INotificationRepository _notificationRepository;

        public MarkNotificationAsReadCommandHandler(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<Unit> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var notification = await _notificationRepository.GetByIdAsync(request.NotificationId);
            if (notification == null || notification.UserId != request.UserId)
            {
                throw new KeyNotFoundException("Notification not found or does not belong to this user");
            }

            await _notificationRepository.MarkAsReadAsync(request.NotificationId);

            return Unit.Value;
        }
    }
}
