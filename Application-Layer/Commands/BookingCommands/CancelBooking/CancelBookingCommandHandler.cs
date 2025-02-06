using MediatR;
using Application_Layer.Interfaces;
using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Domain_Layer.Models;

namespace Application_Layer.Commands.BookingCommands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;

        public CancelBookingCommandHandler(
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository,
            IMediator mediator,
            INotificationService notificationService
        )
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _mediator = mediator;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            // 1) Hämta bokning
            var booking = await _bookingRepository.GetByIdAsync(request.Id);
            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {request.Id} was not found.");

            // 2) Kontrollera att den är i framtiden
            if (booking.StartTime <= DateTime.Now)
                throw new InvalidOperationException("Cannot cancel a booking that has already started or completed.");

            // 3) Avboka (radera) bokning
            await _bookingRepository.DeleteAsync(request.Id);

            // 4) Hämta tjänsten för notisens meddelande
            var service = await _serviceRepository.GetServiceByIdAsync(booking.ServiceId);
            var serviceName = service != null ? service.Name : "tjänsten";

            // 5) Skapa notis i DB
            var title = "Bokning avbokad";
            var message = $"Din bokning för {serviceName} " +
                          $"({booking.StartTime:yyyy-MM-dd HH:mm}) har avbokats.";

            var notificationCmd = new CreateNotificationCommand
            {
                Title = title,
                Message = message,
                UserId = booking.UserId,
                Type = NotificationType.BookingCancellation,
            };
            await _mediator.Send(notificationCmd);

            // 6) Skicka realtidsnotis
            await _notificationService.SendBookingNotificationAsync(
                booking.UserId,
                title,
                message,
                NotificationType.BookingCancellation,
                booking.Id
            );

            return true;
        }
    }
}
