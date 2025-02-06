using Application_Layer.Interfaces;
using AutoMapper;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.NotificationCommands.CreateNotification
{
    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, CreateNotificationResult>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public CreateNotificationCommandHandler(INotificationRepository notificationRepository, IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<CreateNotificationResult> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = _mapper.Map<NotificationModel>(request);

            await _notificationRepository.CreateAsync(notification);

            return _mapper.Map<CreateNotificationResult>(notification);
        }
    }
}
