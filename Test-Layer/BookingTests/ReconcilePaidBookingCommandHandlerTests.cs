using Application_Layer.Commands.BookingCommands.CreateBooking;
using Application_Layer.Commands.BookingCommands.ReconcilePaidBooking;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.BookingTests;

/// <summary>Avstämning av en genomförd onlinebetalning till en bokning (retur/webhook).</summary>
[TestFixture]
public class ReconcilePaidBookingCommandHandlerTests
{
    private IStripePaymentService _stripe = null!;
    private IBookingRepository _bookingRepository = null!;
    private IMediator _mediator = null!;
    private ReconcilePaidBookingCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _stripe = A.Fake<IStripePaymentService>();
        _bookingRepository = A.Fake<IBookingRepository>();
        _mediator = A.Fake<IMediator>();
        A.CallTo(() => _stripe.IsConfigured).Returns(true);
        _handler = new ReconcilePaidBookingCommandHandler(
            _stripe, _bookingRepository, _mediator,
            NullLogger<ReconcilePaidBookingCommandHandler>.Instance);
    }

    private static Dictionary<string, string> ValidMeta(DateTime? start = null) => new()
    {
        ["userId"] = "user-1",
        ["serviceId"] = Guid.NewGuid().ToString(),
        ["employeeId"] = Guid.NewGuid().ToString(),
        ["startTime"] = (start ?? DateTime.Now.AddDays(3)).ToString("o"),
        ["endTime"] = (start ?? DateTime.Now.AddDays(3)).AddMinutes(30).ToString("o"),
    };

    [Test]
    public async Task Handle_WhenSucceededWithMetadata_SendsCreateBooking()
    {
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_1", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, ValidMeta()));
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._))
            .Returns(OperationResult<BookingModel>.Success(new BookingModel { UserId = "user-1" }));

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_1", "user-1"), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
        A.CallTo(() => _mediator.Send(
                A<CreateBookingCommand>.That.Matches(c => c.Booking.PaymentIntentId == "pi_1"
                    && c.Booking.UserId == "user-1"),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenPaymentAlreadyLinked_ReturnsConflictWithoutStripeCall()
    {
        // Idempotens: webhooken och returflödet kan båda stämma av samma betalning.
        A.CallTo(() => _bookingRepository.ExistsByPaymentIntentIdAsync("pi_dup")).Returns(true);

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_dup", "user-1"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.Conflict));
        });
        A.CallTo(() => _stripe.GetPaymentIntentAsync(A<string>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenCallerDoesNotOwnPayment_ReturnsForbidden()
    {
        // Användare A får inte slutföra användare B:s betalning.
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_1", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, ValidMeta()));

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_1", "user-EVIL"), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.Forbidden));
        });
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenWebhookWithoutUser_SkipsOwnershipCheck()
    {
        // Webhooken (RequestingUserId=null) auktoriseras av Stripe-signaturen.
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_1", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, ValidMeta()));
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._))
            .Returns(OperationResult<BookingModel>.Success(new BookingModel { UserId = "user-1" }));

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_1", null), CancellationToken.None);

        Assert.That(result.Successful, Is.True);
    }

    [Test]
    public async Task Handle_WhenSlotAlreadyPassed_RefundsInsteadOfBooking()
    {
        // Sen omleverans av webhook: tiden har passerat → återbetala, boka inte.
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_late", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, ValidMeta(SwedishTime.Now.AddHours(-2))));
        A.CallTo(() => _stripe.RefundAsync("pi_late", null, "reconcile-expired-refund-pi_late", A<CancellationToken>._))
            .Returns(new StripeRefundResult(true, null));

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_late", null), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.Error, Does.Contain("återbetalas"));
        });
        A.CallTo(() => _stripe.RefundAsync("pi_late", null, "reconcile-expired-refund-pi_late", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenPaymentRefunded_FailsWithoutBooking()
    {
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_ref", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, true, ValidMeta()));

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_ref", null), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenNotSucceeded_FailsWithoutBooking()
    {
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_2", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("processing", 0, false, ValidMeta()));

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_2", null), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenMetadataMissing_FailsWithoutBooking()
    {
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_3", A<CancellationToken>._))
            .Returns(new PaymentIntentInfo("succeeded", 200000, false, new Dictionary<string, string>()));

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_3", null), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _mediator.Send(A<CreateBookingCommand>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenPaymentIntentNotFound_ReturnsNotFound()
    {
        A.CallTo(() => _stripe.GetPaymentIntentAsync("pi_x", A<CancellationToken>._))
            .Returns((PaymentIntentInfo?)null);

        var result = await _handler.Handle(new ReconcilePaidBookingCommand("pi_x", null), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.NotFound));
        });
    }
}
