using MediatR;
using AutoMapper;
using Application_Layer.Interfaces;
using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Domain_Layer.Models;

namespace Application_Layer.Commands.BookingCommands.UpdateBooking
{
    public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;

        public UpdateBookingCommandHandler(IBookingRepository bookingRepository, IMapper mapper, IServiceRepository serviceRepository, IMediator mediator, INotificationService notificationService)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _serviceRepository = serviceRepository;
            _mediator = mediator;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            if (request.Booking.ServiceId == Guid.Empty)
            {
                throw new ArgumentException("ServiceId is required and cannot be empty.");
            }

            // 1) Hämta bokning
            var booking = await _bookingRepository.GetByIdAsync(request.Id);
            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {request.Id} was not found.");

            // 2) Uppdatera bokningsmodell
            _mapper.Map(request.Booking, booking);
            await _bookingRepository.UpdateAsync(booking);

            // 3) Ev. hämta tjänsten (om du vill nämna den i notisen)
            var service = await _serviceRepository.GetServiceByIdAsync(booking.ServiceId);
            var serviceName = service != null ? service.Name : "tjänsten";

            // 4) Skapa notis i DB
            var title = "Bokningsändring";
            var message = $"Din bokning för {serviceName} " +
                          $"({booking.StartTime:yyyy-MM-dd HH:mm}) har uppdaterats.";

            var notificationCmd = new CreateNotificationCommand
            {
                Title = title,
                Message = message,
                UserId = booking.UserId,
                Type = NotificationType.BookingUpdated,  // t.ex. ny enum
                BookingId = booking.Id
            };
            await _mediator.Send(notificationCmd);

            // 5) Skicka realtidsnotis
            await _notificationService.SendBookingNotificationAsync(
                booking.UserId,
                title,
                message,
                NotificationType.BookingUpdated,
                booking.Id
            );


            return true;
        }
    }
}
