using Application_Layer.Interfaces;
using Application_Layer.DTOs;
using AutoMapper;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands.CreateNotification
{
    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, OperationResult<NotificationDTO>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public CreateNotificationCommandHandler(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<NotificationDTO>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = _mapper.Map<NotificationModel>(request);
            notification.CreatedAt = DateTime.UtcNow;

            await _notificationRepository.CreateAsync(notification);

            return OperationResult<NotificationDTO>.Success(_mapper.Map<NotificationDTO>(notification));
        }
    }
}
