using Application_Layer.DTOs;
using AutoMapper;
using Domain_Layer.Models;
using Infrastructure_Layer.Repositories.Notification;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Queries.NotificationQueries
{
    public class GetUserNotificationsQueryHandler : IRequestHandler<GetUserNotificationsQuery, List<NotificationDTO>>
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public GetUserNotificationsQueryHandler(
            INotificationRepository notificationRepository, 
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<List<NotificationDTO>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(request.UserId);
            return _mapper.Map<List<NotificationDTO>>(notifications);
        }
    }
}
