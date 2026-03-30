using MediatR;
using Application_Layer.Interfaces;
using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Domain_Layer.Common;
using Domain_Layer.Models;

namespace Application_Layer.Commands.BookingCommands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, OperationResult>
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

        public async Task<OperationResult> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.Id);
            if (booking == null)
            {
                return OperationResult.Failure(
                    $"Booking with ID {request.Id} was not found.",
                    OperationFailureType.NotFound);
            }

            if (!request.CanManageBooking && booking.UserId != request.RequestingUserId)
            {
                return OperationResult.Failure(
                    "You do not have permission to cancel this booking.",
                    OperationFailureType.Forbidden);
            }

            if (booking.StartTime <= DateTime.Now)
            {
                return OperationResult.Failure("Cannot cancel a booking that has already started or completed.");
            }

            await _bookingRepository.DeleteAsync(request.Id);

            var service = await _serviceRepository.GetServiceByIdAsync(booking.ServiceId);
            var serviceName = service != null ? service.Name : "tjänsten";

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

            await _notificationService.SendBookingNotificationAsync(
                booking.UserId,
                title,
                message,
                NotificationType.BookingCancellation,
                booking.Id
            );

            return OperationResult.Success();
        }
    }
}
