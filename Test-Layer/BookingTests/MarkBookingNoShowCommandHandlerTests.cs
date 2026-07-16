using Application_Layer.Commands.BookingCommands.MarkBookingNoShow;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.BookingTests;

[TestFixture]
public class MarkBookingNoShowCommandHandlerTests
{
    private IBookingRepository _bookingRepository = null!;
    private IUserRepository _userRepository = null!;
    private IServiceRepository _serviceRepository = null!;
    private IStripePaymentService _stripe = null!;
    private IMediator _mediator = null!;
    private INotificationService _notificationService = null!;
    private MarkBookingNoShowCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = A.Fake<IBookingRepository>();
        _userRepository = A.Fake<IUserRepository>();
        _serviceRepository = A.Fake<IServiceRepository>();
        _stripe = A.Fake<IStripePaymentService>();
        _mediator = A.Fake<IMediator>();
        _notificationService = A.Fake<INotificationService>();

        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Stripe:NoShowFeeAmount"] = "200",
            ["Stripe:Currency"] = "sek",
        }).Build();

        _handler = new MarkBookingNoShowCommandHandler(
            _bookingRepository, _userRepository, _serviceRepository, _stripe,
            _mediator, _notificationService, config,
            NullLogger<MarkBookingNoShowCommandHandler>.Instance);
    }

    private static BookingModel PastBookingWithCard() => new()
    {
        Id = Guid.NewGuid(),
        UserId = "user-1",
        ServiceId = Guid.NewGuid(),
        StartTime = SwedishTime.Now.AddHours(-2),
        EndTime = SwedishTime.Now.AddHours(-1),
        Status = BookingStatus.Active,
        StripePaymentMethodId = "pm_123",
    };

    private void StripeConfigured(bool configured) =>
        A.CallTo(() => _stripe.IsConfigured).Returns(configured);

    [Test]
    public async Task Handle_WhenConfigured_ChargesCardAndMarksNoShow()
    {
        var booking = PastBookingWithCard();
        StripeConfigured(true);
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);
        A.CallTo(() => _userRepository.FindByIdAsync("user-1"))
            .Returns(new UserModel { Id = "user-1", StripeCustomerId = "cus_123" });
        A.CallTo(() => _stripe.ChargeOffSessionAsync(
                "cus_123", "pm_123", 20000, "sek", A<string>._, A<IDictionary<string, string>>._, A<string>._, A<CancellationToken>._))
            .Returns(new StripeChargeResult(true, "pi_1", "succeeded", null));

        var result = await _handler.Handle(new MarkBookingNoShowCommand(booking.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.NoShow));
            Assert.That(booking.NoShowFeeChargedAt, Is.Not.Null);
        });
        A.CallTo(() => _stripe.ChargeOffSessionAsync(
                "cus_123", "pm_123", 20000, "sek", A<string>._, A<IDictionary<string, string>>._, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _bookingRepository.UpdateAsync(booking)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.SendBookingNotificationAsync(
                "user-1", A<string>._, A<string>._, NotificationType.NoShowFeeCharged, booking.Id))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenBookingIsInFuture_FailsWithoutCharging()
    {
        var booking = PastBookingWithCard();
        booking.StartTime = SwedishTime.Now.AddDays(1);
        StripeConfigured(true);
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);

        var result = await _handler.Handle(new MarkBookingNoShowCommand(booking.Id), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _stripe.ChargeOffSessionAsync(
                A<string>._, A<string>._, A<long>._, A<string>._, A<string>._, A<IDictionary<string, string>>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _bookingRepository.UpdateAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenBookingNotFound_ReturnsNotFound()
    {
        var id = Guid.NewGuid();
        A.CallTo(() => _bookingRepository.GetByIdAsync(id)).Returns((BookingModel?)null);

        var result = await _handler.Handle(new MarkBookingNoShowCommand(id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.NotFound));
        });
    }

    [Test]
    public async Task Handle_WhenChargeFails_DoesNotMarkNoShow()
    {
        var booking = PastBookingWithCard();
        StripeConfigured(true);
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);
        A.CallTo(() => _userRepository.FindByIdAsync("user-1"))
            .Returns(new UserModel { Id = "user-1", StripeCustomerId = "cus_123" });
        A.CallTo(() => _stripe.ChargeOffSessionAsync(
                A<string>._, A<string>._, A<long>._, A<string>._, A<string>._, A<IDictionary<string, string>>._, A<string>._, A<CancellationToken>._))
            .Returns(new StripeChargeResult(false, null, "failed", "Kortet nekades."));

        var result = await _handler.Handle(new MarkBookingNoShowCommand(booking.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active), "status ska inte ändras om debiteringen misslyckas");
        });
        A.CallTo(() => _bookingRepository.UpdateAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenStripeNotConfigured_MarksNoShowWithoutCharge()
    {
        var booking = PastBookingWithCard();
        StripeConfigured(false);
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);

        var result = await _handler.Handle(new MarkBookingNoShowCommand(booking.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.NoShow));
            Assert.That(booking.NoShowFeeChargedAt, Is.Null);
        });
        A.CallTo(() => _stripe.ChargeOffSessionAsync(
                A<string>._, A<string>._, A<long>._, A<string>._, A<string>._, A<IDictionary<string, string>>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => _bookingRepository.UpdateAsync(booking)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenConfiguredButBookingHasNoCard_Fails()
    {
        var booking = PastBookingWithCard();
        booking.StripePaymentMethodId = null;
        StripeConfigured(true);
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);
        A.CallTo(() => _userRepository.FindByIdAsync("user-1"))
            .Returns(new UserModel { Id = "user-1", StripeCustomerId = "cus_123" });

        var result = await _handler.Handle(new MarkBookingNoShowCommand(booking.Id), CancellationToken.None);

        Assert.That(result.Successful, Is.False);
        A.CallTo(() => _stripe.ChargeOffSessionAsync(
                A<string>._, A<string>._, A<long>._, A<string>._, A<string>._, A<IDictionary<string, string>>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenPrepaidOnline_MarksNoShowWithoutChargingCard()
    {
        // Kunden har redan betalat online → ingen kortdebitering, bara markera utebliven.
        var booking = PastBookingWithCard();
        booking.PaymentStatus = PaymentStatus.PaidInFull;
        booking.StripePaymentMethodId = null;
        StripeConfigured(true);
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);

        var result = await _handler.Handle(new MarkBookingNoShowCommand(booking.Id), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.NoShow));
        });
        A.CallTo(() => _stripe.ChargeOffSessionAsync(
                A<string>._, A<string>._, A<long>._, A<string>._, A<string>._, A<IDictionary<string, string>>._, A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
