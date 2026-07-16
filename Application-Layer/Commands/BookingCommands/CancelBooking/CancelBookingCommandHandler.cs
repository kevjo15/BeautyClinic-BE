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
        private readonly IStripePaymentService _stripe;

        public CancelBookingCommandHandler(
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository,
            IMediator mediator,
            INotificationService notificationService,
            IStripePaymentService stripe
        )
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _mediator = mediator;
            _notificationService = notificationService;
            _stripe = stripe;
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

            if (booking.StartTime <= SwedishTime.Now)
            {
                return OperationResult.Failure("Cannot cancel a booking that has already started or completed.");
            }

            // Återbetalning: om kunden betalat online återbetalas hela beloppet vid
            // avbokning i god tid (>24h före). Senare än så behålls pengarna som
            // sen-avboknings-avgift (samma gräns som no-show, enligt villkoren).
            var refunded = false;
            var paidOnline = !string.IsNullOrWhiteSpace(booking.StripePaymentIntentId)
                && booking.PaymentStatus == PaymentStatus.PaidInFull;
            if (_stripe.IsConfigured && paidOnline)
            {
                var hoursUntilStart = (booking.StartTime - SwedishTime.Now).TotalHours;
                if (hoursUntilStart > 24)
                {
                    var refund = await _stripe.RefundAsync(
                        booking.StripePaymentIntentId!, amountMinorUnit: null,
                        idempotencyKey: $"refund-{booking.Id}", cancellationToken);
                    if (!refund.Succeeded)
                    {
                        // Blockera avbokningen om återbetalningen inte gick — undvik att
                        // bokningen försvinner medan kundens pengar är kvar. Låt kunden försöka igen.
                        return OperationResult.Failure(refund.Error ?? "Återbetalningen misslyckades. Försök igen.");
                    }
                    booking.PaymentStatus = PaymentStatus.Refunded;
                    refunded = true;
                }
            }

            // Soft delete: behåll bokningen men markera den som avbokad, så att
            // avbokningen kan följas upp i adminrapporten. Operativa queries
            // (mina bokningar, personalens schema, konfliktkontroll) filtrerar
            // bort avbokade rader så tiden frigörs precis som vid hård radering.
            booking.Status = BookingStatus.Cancelled;
            await _bookingRepository.UpdateAsync(booking);

            var service = await _serviceRepository.GetServiceByIdAsync(booking.ServiceId);
            var serviceName = service != null ? service.Name : "tjänsten";

            var title = "Bokning avbokad";
            var refundNote = refunded
                ? " Ditt inbetalda belopp återbetalas."
                : paidOnline ? " Enligt villkoren återbetalas inte beloppet vid avbokning senare än 24 timmar före besöket." : "";
            var message = $"Din bokning för {serviceName} " +
                          $"({booking.StartTime:yyyy-MM-dd HH:mm}) har avbokats.{refundNote}";

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
