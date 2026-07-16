using Application_Layer.Commands.BookingCommands.CreateBooking;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;

namespace Test_Layer.BookingTests;

/// <summary>Kort-på-fil-beteendet i bokningsskapandet (Stripe-vakt + kortlagring).</summary>
[TestFixture]
public class CreateBookingCommandHandlerStripeTests
{
    private IBookingRepository _bookingRepository = null!;
    private IApplicationMapper _mapper = null!;
    private IServiceRepository _serviceRepository = null!;
    private IMediator _mediator = null!;
    private INotificationService _notificationService = null!;
    private IConversationRepository _conversationRepository = null!;
    private IUserRepository _userRepository = null!;
    private IStripePaymentService _stripe = null!;
    private CreateBookingCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = A.Fake<IBookingRepository>();
        _mapper = A.Fake<IApplicationMapper>();
        _serviceRepository = A.Fake<IServiceRepository>();
        _mediator = A.Fake<IMediator>();
        _notificationService = A.Fake<INotificationService>();
        _conversationRepository = A.Fake<IConversationRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _stripe = A.Fake<IStripePaymentService>();

        _handler = new CreateBookingCommandHandler(
            _bookingRepository, _mapper, _serviceRepository, _mediator,
            _notificationService, _conversationRepository, _userRepository, _stripe);

        // Standard: en giltig tjänst och en mappad bokning utan tilldelad personal
        // (så ingen konversation skapas), och konfliktfri insättning.
        A.CallTo(() => _serviceRepository.GetServiceByIdAsync(A<Guid>._))
            .Returns(new ServiceModel { Name = "Botox Panna", Price = 2000m });
        A.CallTo(() => _mapper.ToBookingModel(A<CreateBookingDTO>._))
            .ReturnsLazily(() => new BookingModel { UserId = "user-1" });
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).Returns(true);
    }

    private static CreateBookingCommand Command(string? paymentMethodId) => new(new CreateBookingDTO
    {
        UserId = "user-1",
        ServiceId = Guid.NewGuid(),
        StartTime = DateTime.Now.AddDays(5),
        EndTime = DateTime.Now.AddDays(5).AddMinutes(30),
        EmployeeId = "", // ingen personal → ingen konversation
        PaymentMethodId = paymentMethodId,
    });

    [Test]
    public async Task Handle_WhenStripeConfiguredAndNoCard_FailsWithoutCreatingBooking()
    {
        A.CallTo(() => _stripe.IsConfigured).Returns(true);

        var result = await _handler.Handle(Command(paymentMethodId: null), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenStripeConfiguredWithCard_StoresPaymentMethodAndCardDetails()
    {
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _stripe.GetCardDetailsAsync("pm_123", A<CancellationToken>._))
            .Returns(new CardDetails("visa", "4242"));

        BookingModel? saved = null;
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._))
            .Invokes((BookingModel b) => saved = b)
            .Returns(true);

        var result = await _handler.Handle(Command("pm_123"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(saved!.StripePaymentMethodId, Is.EqualTo("pm_123"));
            Assert.That(saved.CardBrand, Is.EqualTo("visa"));
            Assert.That(saved.CardLast4, Is.EqualTo("4242"));
        });
    }

    [Test]
    public async Task Handle_WhenStripeNotConfigured_CreatesBookingWithoutCard()
    {
        A.CallTo(() => _stripe.IsConfigured).Returns(false);

        BookingModel? saved = null;
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._))
            .Invokes((BookingModel b) => saved = b)
            .Returns(true);

        var result = await _handler.Handle(Command(paymentMethodId: null), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(saved!.StripePaymentMethodId, Is.Null);
        });
        A.CallTo(() => _stripe.GetCardDetailsAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    private static CreateBookingCommand OnlineCommand(string paymentIntentId) => new(new CreateBookingDTO
    {
        UserId = "user-1",
        ServiceId = Guid.NewGuid(),
        StartTime = DateTime.Now.AddDays(5),
        EndTime = DateTime.Now.AddDays(5).AddMinutes(30),
        EmployeeId = "",
        PaymentIntentId = paymentIntentId,
    });

    [Test]
    public async Task Handle_WhenPaidInFullOnline_StoresPaidInFullAndAmount()
    {
        // Service = 2000 kr → full betalning = 200000 öre.
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_1", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, new Dictionary<string, string>()));

        BookingModel? saved = null;
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._))
            .Invokes((BookingModel b) => saved = b).Returns(true);

        var result = await _handler.Handle(OnlineCommand("pi_1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(saved!.PaymentStatus, Is.EqualTo(PaymentStatus.PaidInFull));
            Assert.That(saved.AmountPaid, Is.EqualTo(2000m));
            Assert.That(saved.StripePaymentIntentId, Is.EqualTo("pi_1"));
        });
    }

    [Test]
    public async Task Handle_WhenPaymentNotSucceeded_FailsWithoutBooking()
    {
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_3", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("requires_payment_method", 0, false, new Dictionary<string, string>()));

        var result = await _handler.Handle(OnlineCommand("pi_3"), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenPaymentIntentAlreadyUsed_FailsWithoutBooking()
    {
        // Skydd: samma betalning får inte ge två bokningar (t.ex. omkört Klarna-returflöde).
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _bookingRepository.ExistsByPaymentIntentIdAsync("pi_used")).Returns(true);

        var result = await _handler.Handle(OnlineCommand("pi_used"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.Conflict));
        });
        A.CallTo(() => _stripe.GetPaymentIntentAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenSlotTakenAfterOnlinePayment_RefundsAutomatically()
    {
        // Kunden betalade (t.ex. via Klarna-redirect) men tiden hann tas → auto-refund.
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_5", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, new Dictionary<string, string>()));
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).Returns(false);
        A.CallTo(() => _stripe.RefundAsync("pi_5", null, "conflict-refund-pi_5", A<CancellationToken>._))
            .Returns(new StripeRefundResult(true, null));

        var result = await _handler.Handle(OnlineCommand("pi_5"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Error, Does.Contain("återbetalas"));
        });
        A.CallTo(() => _stripe.RefundAsync("pi_5", null, "conflict-refund-pi_5", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenPaymentRefunded_FailsWithoutBooking()
    {
        // Stripe låter status vara "succeeded" efter refund — flaggan måste stoppa bokning.
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_ref", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, true, new Dictionary<string, string>()));

        var result = await _handler.Handle(OnlineCommand("pi_ref"), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenConflictButSamePaymentAlreadyBooked_DoesNotRefund()
    {
        // Race: returflödet och webhooken tävlar om samma PI. Förloraren får slot-konflikt,
        // men betalningen sitter redan på vinnarens bokning → INGEN refund (annars hade
        // kunden fått både bokning och pengar tillbaka).
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_race", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, new Dictionary<string, string>()));
        // Första exists-kollen (vakten) passerar, andra (efter konflikt) ser vinnarens bokning.
        A.CallTo(() => _bookingRepository.ExistsByPaymentIntentIdAsync("pi_race"))
            .ReturnsNextFromSequence(false, true);
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).Returns(false);

        var result = await _handler.Handle(OnlineCommand("pi_race"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.Conflict));
        });
        A.CallTo(() => _stripe.RefundAsync(A<string>._, A<long?>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenPaidAmountMismatch_Fails()
    {
        // Betalade bara 500 kr men "full" borde vara 2000 → nekas (klienten kan inte manipulera beloppet).
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_4", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 50000, false, new Dictionary<string, string>()));

        A.CallTo(() => _stripe.RefundAsync("pi_4", null, "amount-mismatch-refund-pi_4", A<CancellationToken>._))
            .Returns(new StripeRefundResult(true, null));

        var result = await _handler.Handle(OnlineCommand("pi_4"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Error, Does.Contain("återbetalas"));
        });
        A.CallTo(() => _bookingRepository.TryAddIfNoConflictAsync(A<BookingModel>._)).MustNotHaveHappened();
        // Pengar utan bokning får aldrig behållas → beloppsmiss återbetalas automatiskt.
        A.CallTo(() => _stripe.RefundAsync("pi_4", null, "amount-mismatch-refund-pi_4", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
