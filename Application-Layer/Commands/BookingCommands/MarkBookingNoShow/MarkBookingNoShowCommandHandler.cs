using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Application_Layer.Common;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.BookingCommands.MarkBookingNoShow
{
    public class MarkBookingNoShowCommandHandler : IRequestHandler<MarkBookingNoShowCommand, OperationResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IStripePaymentService _stripe;
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MarkBookingNoShowCommandHandler> _logger;

        public MarkBookingNoShowCommandHandler(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IServiceRepository serviceRepository,
            IStripePaymentService stripe,
            IMediator mediator,
            INotificationService notificationService,
            IConfiguration configuration,
            ILogger<MarkBookingNoShowCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _serviceRepository = serviceRepository;
            _stripe = stripe;
            _mediator = mediator;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<OperationResult> Handle(MarkBookingNoShowCommand request, CancellationToken cancellationToken)
        {
            // GetByIdAsync returnerar bara Active-bokningar, så en redan avbokad/no-show-markerad
            // bokning hittas inte → idempotent (kan inte dubbelmarkeras/dubbeldebiteras).
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                return OperationResult.Failure(
                    $"Booking with ID {request.BookingId} was not found or is not active.",
                    OperationFailureType.NotFound);
            }

            if (booking.StartTime > SwedishTime.Now)
            {
                return OperationResult.Failure("Kan inte markera en framtida bokning som utebliven.");
            }

            // Förbetald bokning (hela beloppet online): pengarna finns redan → ingen
            // kortdebitering, bara markera utebliven (det inbetalda behålls som straff).
            var prepaid = booking.PaymentStatus == PaymentStatus.PaidInFull;

            // Debitera no-show-avgift när Stripe är konfigurerat och bokningen INTE är förbetald.
            // Måste lyckas innan bokningen markeras — annars stannar den kvar så personalen kan försöka igen.
            if (_stripe.IsConfigured && !prepaid)
            {
                var user = await _userRepository.FindByIdAsync(booking.UserId);
                if (user == null || string.IsNullOrWhiteSpace(user.StripeCustomerId)
                    || string.IsNullOrWhiteSpace(booking.StripePaymentMethodId))
                {
                    return OperationResult.Failure(
                        "Bokningen saknar sparat kort och kan inte debiteras automatiskt.");
                }

                var feeKr = _configuration.GetValue<decimal>("Stripe:NoShowFeeAmount", 200m);
                var currency = _configuration["Stripe:Currency"] ?? "sek";
                var amountMinor = PaymentAmounts.ToMinorUnit(feeKr);

                var charge = await _stripe.ChargeOffSessionAsync(
                    user.StripeCustomerId, booking.StripePaymentMethodId, amountMinor, currency,
                    $"No-show fee for booking {booking.Id}",
                    new Dictionary<string, string> { ["bookingId"] = booking.Id.ToString() },
                    idempotencyKey: $"noshow-{booking.Id}",
                    cancellationToken);

                if (!charge.Succeeded)
                {
                    _logger.LogWarning("No-show charge failed for booking {BookingId}: {Error}",
                        booking.Id, charge.Error);
                    return OperationResult.Failure(charge.Error ?? "No-show-avgiften kunde inte dras.");
                }

                booking.NoShowFeeChargedAt = SwedishTime.Now;
            }

            booking.Status = BookingStatus.NoShow;
            await _bookingRepository.UpdateAsync(booking);

            var service = await _serviceRepository.GetServiceByIdAsync(booking.ServiceId);
            var serviceName = service?.Name ?? "din behandling";
            var title = "Utebliven bokning";
            var message = booking.NoShowFeeChargedAt.HasValue
                ? $"Du uteblev från din bokning för {serviceName} ({booking.StartTime:yyyy-MM-dd HH:mm}). En avgift har dragits från ditt sparade kort."
                : $"Din bokning för {serviceName} ({booking.StartTime:yyyy-MM-dd HH:mm}) har markerats som utebliven.";

            await _mediator.Send(new CreateNotificationCommand
            {
                Title = title,
                Message = message,
                UserId = booking.UserId,
                Type = NotificationType.NoShowFeeCharged,
                BookingId = booking.Id,
            }, cancellationToken);

            await _notificationService.SendBookingNotificationAsync(
                booking.UserId, title, message, NotificationType.NoShowFeeCharged, booking.Id);

            return OperationResult.Success();
        }
    }
}
