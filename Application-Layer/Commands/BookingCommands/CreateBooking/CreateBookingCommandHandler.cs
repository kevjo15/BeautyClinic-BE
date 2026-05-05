using MediatR;
using AutoMapper;
using Domain_Layer.Common;
using Domain_Layer.Models;
using Application_Layer.Interfaces;
using Application_Layer.Commands.NotificationCommands.CreateNotification;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, OperationResult<BookingModel>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMediator _mediator;

        private readonly INotificationService _notificationService;
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IMapper mapper,
            IServiceRepository serviceRepository,
            IMediator mediator,
            INotificationService notificationService,
            IConversationRepository conversationRepository,
            IUserRepository userRepository
        )
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _serviceRepository = serviceRepository;
            _mediator = mediator;
            _notificationService = notificationService;
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
        }

        public async Task<OperationResult<BookingModel>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1) Hämta tjänsten
                var service = await _serviceRepository.GetServiceByIdAsync(request.Booking.ServiceId);
                if (service == null)
                {
                    return OperationResult<BookingModel>.Failure("Service not found.");
                }

                // 2) Skapa & spara bokning
                var booking = _mapper.Map<BookingModel>(request.Booking);
                booking.Id = Guid.NewGuid();
                // Assign employee only if provided; chat activates first when employee exists
                var employeeId = request.Booking.EmployeeId;
                booking.EmployeeId = employeeId;

                // Create conversation only when an employee is assigned
                if (!string.IsNullOrWhiteSpace(employeeId))
                {
                    var participantIds = new List<Guid>();
                    if (Guid.TryParse(booking.UserId, out var patientGuid))
                    {
                        participantIds.Add(patientGuid);
                    }
                    if (Guid.TryParse(employeeId, out var employeeGuid))
                    {
                        participantIds.Add(employeeGuid);
                    }

                    var conversation = new ConversationModel
                    {
                        Id = Guid.NewGuid(),
                        ParticipantIds = participantIds,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _conversationRepository.CreateAsync(conversation);

                    booking.ConversationId = conversation.Id;
                }

                var added = await _bookingRepository.TryAddIfNoConflictAsync(booking);
                if (!added)
                {
                    return OperationResult<BookingModel>.Failure("Den valda tiden är inte längre tillgänglig. Välj en annan tid.");
                }

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

                return OperationResult<BookingModel>.Success(booking);
            }
            catch (Exception ex)
            {
                return OperationResult<BookingModel>.Failure($"Failed to create booking: {ex.Message}");
            }
        }
    }
}
