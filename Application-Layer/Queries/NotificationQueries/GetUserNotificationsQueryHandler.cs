using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using MediatR;

namespace Application_Layer.Queries.NotificationQueries
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, List<NotificationDTO>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IApplicationMapper _mapper;

        public GetUserNotificationsQueryHandler(
            INotificationRepository notificationRepository,
            IApplicationMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<List<NotificationDTO>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(request.UserId, request.Limit);
            return _mapper.ToNotificationDtoList(notifications);
        }
    }
}
