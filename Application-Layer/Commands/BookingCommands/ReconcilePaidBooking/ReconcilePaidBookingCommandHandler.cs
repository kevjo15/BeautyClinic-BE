using System.Globalization;
using Application_Layer.Commands.BookingCommands.CreateBooking;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.BookingCommands.ReconcilePaidBooking
{
    public class ReconcilePaidBookingCommandHandler
        : IRequestHandler<ReconcilePaidBookingCommand, OperationResult<BookingModel>>
    {
        private readonly IStripePaymentService _stripe;
        private readonly IBookingRepository _bookingRepository;
        private readonly IMediator _mediator;
        private readonly ILogger<ReconcilePaidBookingCommandHandler> _logger;

        public ReconcilePaidBookingCommandHandler(
            IStripePaymentService stripe,
            IBookingRepository bookingRepository,
            IMediator mediator,
            ILogger<ReconcilePaidBookingCommandHandler> logger)
        {
            _stripe = stripe;
            _bookingRepository = bookingRepository;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<OperationResult<BookingModel>> Handle(
            ReconcilePaidBookingCommand request, CancellationToken cancellationToken)
        {
            if (!_stripe.IsConfigured)
            {
                return OperationResult<BookingModel>.Failure("Betalning är inte konfigurerad.");
            }

            // Idempotens FÖRST (före Stripe-anrop och tidskontroller): är betalningen redan
            // kopplad svarar vi alltid "redan kopplad" — även om tiden hunnit passera.
            if (await _bookingRepository.ExistsByPaymentIntentIdAsync(request.PaymentIntentId))
            {
                return OperationResult<BookingModel>.Failure(
                    "Betalningen är redan kopplad till en bokning.", OperationFailureType.Conflict);
            }

            var info = await _stripe.GetPaymentIntentAsync(request.PaymentIntentId, cancellationToken);
            if (info == null)
            {
                return OperationResult<BookingModel>.Failure(
                    "Betalningen kunde inte hämtas.", OperationFailureType.NotFound);
            }
            if (info.Status != "succeeded")
            {
                // T.ex. en redirect som ännu inte slutförts — webhooken tar den när den blir klar.
                return OperationResult<BookingModel>.Failure("Betalningen är inte slutförd ännu.");
            }
            if (info.Refunded)
            {
                return OperationResult<BookingModel>.Failure("Betalningen är återbetald och kan inte användas.");
            }

            var m = info.Metadata;
            if (!m.TryGetValue("userId", out var userId) || string.IsNullOrWhiteSpace(userId)
                || !m.TryGetValue("serviceId", out var serviceIdStr) || !Guid.TryParse(serviceIdStr, out var serviceId)
                || !m.TryGetValue("startTime", out var startStr)
                || !DateTime.TryParse(startStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var startTime)
                || !m.TryGetValue("endTime", out var endStr)
                || !DateTime.TryParse(endStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var endTime))
            {
                _logger.LogError("PaymentIntent {PaymentIntentId} saknar bokningsuppgifter i metadatan", request.PaymentIntentId);
                return OperationResult<BookingModel>.Failure("Betalningens bokningsuppgifter saknas.");
            }
            m.TryGetValue("employeeId", out var employeeId);

            // Bokningstider är svensk väggtid med Kind=Unspecified (SwedishTime-kontraktet).
            // Lås Kind explicit så en framtida offset i metadatan inte förskjuter tiden.
            startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified);
            endTime = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified);

            // Ägarskap: en inloggad användare får bara slutföra sin egen betalning.
            // (Webhooken skickar RequestingUserId=null — där är Stripe-signaturen auktoriseringen.)
            if (request.RequestingUserId != null && request.RequestingUserId != userId)
            {
                return OperationResult<BookingModel>.Failure(
                    "Betalningen tillhör en annan användare.", OperationFailureType.Forbidden);
            }

            // Tiden hann passera innan avstämningen (t.ex. webhook-omleverans långt senare).
            // En bokning bakåt i tiden är meningslös — återbetala istället för att låta
            // valideringen kasta och pengarna fastna.
            if (startTime <= SwedishTime.Now)
            {
                var refund = await _stripe.RefundAsync(
                    request.PaymentIntentId, amountMinorUnit: null,
                    idempotencyKey: $"reconcile-expired-refund-{request.PaymentIntentId}", cancellationToken);
                _logger.LogWarning(
                    "Betalning {PaymentIntentId} stämdes av efter att tiden passerat — återbetalning {Result}",
                    request.PaymentIntentId, refund.Succeeded ? "genomförd" : "MISSLYCKADES");
                return OperationResult<BookingModel>.Failure(refund.Succeeded
                    ? "Tiden har redan passerat. Din betalning återbetalas automatiskt."
                    : "Tiden har redan passerat och återbetalningen kunde inte genomföras — kontakta kliniken.");
            }

            // Återanvänd det ordinarie bokningsflödet: det verifierar beloppet mot Stripe,
            // skyddar mot dubbel användning av samma betalning och auto-återbetalar vid
            // slot-konflikt. Så både /payment-return och webhooken blir idempotenta.
            var dto = new CreateBookingDTO
            {
                UserId = userId,
                ServiceId = serviceId,
                StartTime = startTime,
                EndTime = endTime,
                EmployeeId = employeeId ?? string.Empty,
                PaymentIntentId = request.PaymentIntentId,
            };
            try
            {
                return await _mediator.Send(new CreateBookingCommand(dto), cancellationToken);
            }
            catch (ValidationException ex)
            {
                // Metadatan är serverside-satt så detta ska inte hända — men om det gör det
                // får det aldrig bli en 500-loop mot Stripe (webhooks görs om i dagar).
                _logger.LogError(ex, "Valideringsfel vid avstämning av betalning {PaymentIntentId}", request.PaymentIntentId);
                return OperationResult<BookingModel>.Failure("Bokningsuppgifterna kunde inte valideras. Kontakta kliniken.");
            }
        }
    }
}
