using Application_Layer.Commands.BookingCommands.CancelBooking;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;

namespace Test_Layer.BookingTests;

[TestFixture]
public class CancelBookingCommandHandlerTests
{
    private IBookingRepository _bookingRepository = null!;
    private IServiceRepository _serviceRepository = null!;
    private IMediator _mediator = null!;
    private INotificationService _notificationService = null!;
    private CancelBookingCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = A.Fake<IBookingRepository>();
        _serviceRepository = A.Fake<IServiceRepository>();
        _mediator = A.Fake<IMediator>();
        _notificationService = A.Fake<INotificationService>();
        _handler = new CancelBookingCommandHandler(
            _bookingRepository, _serviceRepository, _mediator, _notificationService);
    }

    private static BookingModel FutureBooking(string userId = "user-1") => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ServiceId = Guid.NewGuid(),
        StartTime = SwedishTime.Now.AddDays(1),
        EndTime = SwedishTime.Now.AddDays(1).AddMinutes(30),
        Status = BookingStatus.Active,
    };

    [Test]
    public async Task Handle_SoftDeletes_SetsStatusCancelledAndUpdates()
    {
        var booking = FutureBooking();
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);
        A.CallTo(() => _serviceRepository.GetServiceByIdAsync(booking.ServiceId))
            .Returns(new ServiceModel { Name = "Botox Panna" });

        var result = await _handler.Handle(
            new CancelBookingCommand(booking.Id, booking.UserId, CanManageBooking: false),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Cancelled));
        });
        // Soft delete: bokningen uppdateras (ej raderas) och behåller sitt ID.
        A.CallTo(() => _bookingRepository.UpdateAsync(
            A<BookingModel>.That.Matches(b => b.Id == booking.Id && b.Status == BookingStatus.Cancelled)))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_SendsCancellationNotification()
    {
        var booking = FutureBooking();
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);
        A.CallTo(() => _serviceRepository.GetServiceByIdAsync(booking.ServiceId))
            .Returns(new ServiceModel { Name = "Botox Panna" });

        await _handler.Handle(
            new CancelBookingCommand(booking.Id, booking.UserId, false), CancellationToken.None);

        A.CallTo(() => _notificationService.SendBookingNotificationAsync(
            booking.UserId, A<string>._, A<string>._,
            NotificationType.BookingCancellation, booking.Id))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_WhenBookingMissing_ReturnsNotFoundAndDoesNotUpdate()
    {
        var id = Guid.NewGuid();
        A.CallTo(() => _bookingRepository.GetByIdAsync(id)).Returns((BookingModel?)null);

        var result = await _handler.Handle(
            new CancelBookingCommand(id, "user-1", false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.NotFound));
        });
        A.CallTo(() => _bookingRepository.UpdateAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenNotOwnerAndCannotManage_ReturnsForbidden()
    {
        var booking = FutureBooking(userId: "owner");
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);

        var result = await _handler.Handle(
            new CancelBookingCommand(booking.Id, "someone-else", CanManageBooking: false),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(result.FailureType, Is.EqualTo(OperationFailureType.Forbidden));
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active));
        });
        A.CallTo(() => _bookingRepository.UpdateAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenBookingAlreadyStarted_ReturnsFailureAndDoesNotCancel()
    {
        var booking = FutureBooking();
        booking.StartTime = SwedishTime.Now.AddHours(-1);
        A.CallTo(() => _bookingRepository.GetByIdAsync(booking.Id)).Returns(booking);

        var result = await _handler.Handle(
            new CancelBookingCommand(booking.Id, booking.UserId, false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.False);
            Assert.That(booking.Status, Is.EqualTo(BookingStatus.Active));
        });
        A.CallTo(() => _bookingRepository.UpdateAsync(A<BookingModel>._)).MustNotHaveHappened();
    }
}
