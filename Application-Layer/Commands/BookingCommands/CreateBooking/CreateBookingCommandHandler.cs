using MediatR;
using Application_Layer.Mapping;
using Application_Layer.Common;
using Domain_Layer.Common;
using Domain_Layer.Models;
using Application_Layer.Interfaces;
using Application_Layer.Commands.NotificationCommands.CreateNotification;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, OperationResult<BookingModel>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationMapper _mapper;
        private readonly IServiceRepository _serviceRepository;
        private readonly IMediator _mediator;

        private readonly INotificationService _notificationService;
        private readonly IConversationRepository _conversationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStripePaymentService _stripePaymentService;

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IApplicationMapper mapper,
            IServiceRepository serviceRepository,
            IMediator mediator,
            INotificationService notificationService,
            IConversationRepository conversationRepository,
            IUserRepository userRepository,
            IStripePaymentService stripePaymentService
        )
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _serviceRepository = serviceRepository;
            _mediator = mediator;
            _notificationService = notificationService;
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
            _stripePaymentService = stripePaymentService;
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

                // Betalning: när Stripe är konfigurerat krävs antingen en online-betalning
                // (PaymentIntent) ELLER ett sparat kort (kort-på-fil, betala på plats).
                // Utan Stripe (lokal dev) bokas kortlöst.
                var paymentMethodId = request.Booking.PaymentMethodId;
                var paymentIntentId = request.Booking.PaymentIntentId;
                if (_stripePaymentService.IsConfigured
                    && string.IsNullOrWhiteSpace(paymentMethodId)
                    && string.IsNullOrWhiteSpace(paymentIntentId))
                {
                    return OperationResult<BookingModel>.Failure("Betalning eller sparat kort krävs för att boka.");
                }

                // 2) Skapa & spara bokning
                var booking = _mapper.ToBookingModel(request.Booking);
                booking.Id = Guid.NewGuid();

                if (!string.IsNullOrWhiteSpace(paymentIntentId))
                {
                    // Samma betalning får inte användas till flera bokningar (t.ex. om
                    // Klarna-returflödet råkar köras om efter att bokningen redan skapats).
                    if (await _bookingRepository.ExistsByPaymentIntentIdAsync(paymentIntentId))
                    {
                        return OperationResult<BookingModel>.Failure(
                            "Betalningen är redan kopplad till en bokning.", OperationFailureType.Conflict);
                    }

                    // Onlinebetalning: verifiera serverside att den faktiskt gått igenom och
                    // att beloppet matchar (klienten får aldrig bestämma beloppet).
                    var info = await _stripePaymentService.GetPaymentIntentAsync(paymentIntentId, cancellationToken);
                    if (info == null || info.Status != "succeeded")
                    {
                        return OperationResult<BookingModel>.Failure("Betalningen kunde inte verifieras.");
                    }
                    if (info.Refunded)
                    {
                        // T.ex. en konflikt-återbetald betalning vars webhook levereras om.
                        return OperationResult<BookingModel>.Failure("Betalningen är återbetald och kan inte användas.");
                    }

                    var expectedKr = service.Price;
                    if (info.AmountReceivedMinorUnit != PaymentAmounts.ToMinorUnit(expectedKr))
                    {
                        // Priset hann ändras mellan betalning och bokning → behåll aldrig
                        // pengar utan bokning; återbetala automatiskt.
                        var mismatchRefund = await _stripePaymentService.RefundAsync(
                            paymentIntentId, amountMinorUnit: null,
                            idempotencyKey: $"amount-mismatch-refund-{paymentIntentId}", cancellationToken);
                        return OperationResult<BookingModel>.Failure(mismatchRefund.Succeeded
                            ? "Betalt belopp stämmer inte med behandlingens pris. Din betalning återbetalas automatiskt."
                            : "Betalt belopp stämmer inte med behandlingens pris och återbetalningen kunde inte genomföras — kontakta kliniken.");
                    }

                    booking.StripePaymentIntentId = paymentIntentId;
                    booking.AmountPaid = expectedKr;
                    booking.PaymentStatus = PaymentStatus.PaidInFull;
                }
                else if (!string.IsNullOrWhiteSpace(paymentMethodId))
                {
                    // Betala på plats: koppla det sparade kortet (no-show debiteras detta).
                    booking.StripePaymentMethodId = paymentMethodId;
                    var card = await _stripePaymentService.GetCardDetailsAsync(paymentMethodId, cancellationToken);
                    booking.CardBrand = card?.Brand;
                    booking.CardLast4 = card?.Last4;
                }
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
                    // Kunden har redan betalat online men tiden hann tas (t.ex. under en
                    // Klarna-redirect) → återbetala automatiskt så inga pengar fastnar.
                    if (booking.PaymentStatus == PaymentStatus.PaidInFull
                        && !string.IsNullOrWhiteSpace(booking.StripePaymentIntentId))
                    {
                        // Race-skydd: om returflödet och webhooken tävlade om SAMMA betalning
                        // har vinnaren redan skapat bokningen — då är detta ingen konflikt
                        // att återbetala (det hade gett kunden både bokning och pengar).
                        if (await _bookingRepository.ExistsByPaymentIntentIdAsync(booking.StripePaymentIntentId))
                        {
                            return OperationResult<BookingModel>.Failure(
                                "Betalningen är redan kopplad till en bokning.", OperationFailureType.Conflict);
                        }

                        var refund = await _stripePaymentService.RefundAsync(
                            booking.StripePaymentIntentId, amountMinorUnit: null,
                            idempotencyKey: $"conflict-refund-{booking.StripePaymentIntentId}", cancellationToken);
                        return OperationResult<BookingModel>.Failure(refund.Succeeded
                            ? "Den valda tiden är inte längre tillgänglig. Din betalning återbetalas automatiskt."
                            : "Den valda tiden är inte längre tillgänglig och återbetalningen kunde inte genomföras automatiskt — kontakta kliniken.");
                    }

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
                // Race-förloraren när två avstämningar (retur + webhook) tävlar om samma
                // betalning stoppas av det unika indexet på StripePaymentIntentId →
                // svara "redan kopplad" istället för ett generiskt fel. Ingen refund:
                // pengarna hör till vinnarens bokning.
                if (!string.IsNullOrWhiteSpace(request.Booking.PaymentIntentId)
                    && await _bookingRepository.ExistsByPaymentIntentIdAsync(request.Booking.PaymentIntentId))
                {
                    return OperationResult<BookingModel>.Failure(
                        "Betalningen är redan kopplad till en bokning.", OperationFailureType.Conflict);
                }
                return OperationResult<BookingModel>.Failure($"Failed to create booking: {ex.Message}");
            }
        }
    }
}
