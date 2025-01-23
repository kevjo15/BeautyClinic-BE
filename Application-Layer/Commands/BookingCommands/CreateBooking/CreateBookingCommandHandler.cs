using MediatR;
using AutoMapper;
using Domain_Layer.Models;
using Application_Layer.Interfaces;
using Application_Layer.Commands.NotificationCommands.CreateNotification;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMediator _mediator;

        private readonly INotificationService _notificationService;

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IMapper mapper,
            IServiceRepository serviceRepository,
            IMediator mediator,
            INotificationService notificationService
        )
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _serviceRepository = serviceRepository;
            _mediator = mediator;
            _notificationService = notificationService;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1) Hämta tjänsten
                var service = await _serviceRepository.GetServiceByIdAsync(request.Booking.ServiceId);
                if (service == null)
                {
                    return CreateBookingResult.FailureResult("Service not found.");
                }

                // 2) Skapa & spara bokning
                var booking = _mapper.Map<BookingModel>(request.Booking);
                booking.Id = Guid.NewGuid();
                await _bookingRepository.AddAsync(booking);

                // 3) Spara notifikation i DB
                var title = "Bokningsbekräftelse";
                var message = $"Din bokning för {service.Name} den {booking.StartTime:yyyy-MM-dd HH:mm} har bekräftats.";
                var notificationCommand = new CreateNotificationCommand
                {
                    Title = title,
                    Message = message,
                    UserId = booking.UserId,
                    Type = NotificationType.BookingConfirmation,
                    BookingId = booking.Id
                };
                await _mediator.Send(notificationCommand);

                // 4) Skicka realtidsnotis via INotificationService
                await _notificationService.SendBookingNotificationAsync(
                    booking.UserId,
                    title,
                    message,
                    NotificationType.BookingConfirmation,
                    booking.Id
                );

                return CreateBookingResult.SuccessResult("Booking created successfully.", booking);
            }
            catch (Exception ex)
            {
                return CreateBookingResult.FailureResult($"Failed to create booking: {ex.Message}");
            }
        }
    }
}
