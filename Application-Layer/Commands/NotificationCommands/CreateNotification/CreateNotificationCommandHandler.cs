using Application_Layer.Interfaces;
using Application_Layer.DTOs;
using Application_Layer.Mapping;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands.CreateNotification
{
    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, OperationResult<NotificationDTO>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IApplicationMapper _mapper;

        public CreateNotificationCommandHandler(INotificationRepository notificationRepository, IApplicationMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<NotificationDTO>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = _mapper.ToNotificationModel(request);
            notification.CreatedAt = DateTime.UtcNow;

            await _notificationRepository.CreateAsync(notification);

            return OperationResult<NotificationDTO>.Success(_mapper.ToNotificationDto(notification));
        }
    }
}
