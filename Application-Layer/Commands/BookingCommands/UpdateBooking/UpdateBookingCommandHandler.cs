using MediatR;
using Application_Layer.Mapping;
using Application_Layer.Interfaces;
using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Domain_Layer.Common;
using Domain_Layer.Models;

namespace Application_Layer.Commands.BookingCommands.UpdateBooking
{
    public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, OperationResult<BookingModel>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationMapper _mapper;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;

        public UpdateBookingCommandHandler(IBookingRepository bookingRepository, IApplicationMapper mapper, IServiceRepository serviceRepository, IMediator mediator, INotificationService notificationService)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _serviceRepository = serviceRepository;
            _mediator = mediator;
            _notificationService = notificationService;
        }

        public async Task<OperationResult<BookingModel>> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
        {
            if (request.Booking.ServiceId == Guid.Empty)
            {
                return OperationResult<BookingModel>.Failure("ServiceId is required and cannot be empty.");
            }

            var booking = await _bookingRepository.GetByIdAsync(request.Id);
            if (booking == null)
            {
                return OperationResult<BookingModel>.Failure(
                    $"Booking with ID {request.Id} was not found.",
                    OperationFailureType.NotFound);
            }

            if (!request.CanManageBooking && booking.UserId != request.RequestingUserId)
            {
                return OperationResult<BookingModel>.Failure(
                    "You do not have permission to update this booking.",
                    OperationFailureType.Forbidden);
            }

            _mapper.UpdateBookingModel(request.Booking, booking);

            if (!string.IsNullOrEmpty(booking.EmployeeId))
            {
                var hasConflict = await _bookingRepository.HasConflictAsync(
                    booking.Id, booking.EmployeeId, booking.StartTime, booking.EndTime);
                if (hasConflict)
                    return OperationResult<BookingModel>.Failure(
                        "Medarbetaren har redan en bokning på den valda tiden.",
                        OperationFailureType.Conflict);
            }

            await _bookingRepository.UpdateAsync(booking);

            var service = await _serviceRepository.GetServiceByIdAsync(booking.ServiceId);
            var serviceName = service != null ? service.Name : "tjänsten";

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

            await _notificationService.SendBookingNotificationAsync(
                booking.UserId,
                title,
                message,
                NotificationType.BookingUpdated,
                booking.Id
            );

            return OperationResult<BookingModel>.Success(booking);
        }
    }
}
