using Application_Layer.Commands.BookingCommands.SendDueBookingReminders;
using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using FakeItEasy;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;

namespace Test_Layer.BookingTests;

[TestFixture]
public class SendDueBookingRemindersCommandHandlerTests
{
    private IBookingRepository _bookingRepository = null!;
    private IMediator _mediator = null!;
    private INotificationService _notificationService = null!;
    private SendDueBookingRemindersCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = A.Fake<IBookingRepository>();
        _mediator = A.Fake<IMediator>();
        _notificationService = A.Fake<INotificationService>();
        _handler = new SendDueBookingRemindersCommandHandler(
            _bookingRepository, _mediator, _notificationService,
            NullLogger<SendDueBookingRemindersCommandHandler>.Instance);
    }

    private static BookingModel DueBooking(string userId = "user-1") => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        ServiceId = Guid.NewGuid(),
        StartTime = SwedishTime.Now.AddHours(20),
        EndTime = SwedishTime.Now.AddHours(20).AddMinutes(30),
        Status = BookingStatus.Active,
        ReminderSentAt = null,
        Service = new ServiceModel { Name = "Botox Panna" },
    };

    [Test]
    public async Task Handle_SendsReminderOncePerDueBooking_AndMarksSent()
    {
        var booking = DueBooking();
        A.CallTo(() => _bookingRepository.GetDueForReminderAsync(A<DateTime>._, A<DateTime>._))
            .Returns([booking]);

        var result = await _handler.Handle(
            new SendDueBookingRemindersCommand(24), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(result.Data, Is.EqualTo(1));
            Assert.That(booking.ReminderSentAt, Is.Not.Null, "exakt-en-gång-spärren ska sättas");
        });
        A.CallTo(() => _mediator.Send(
            A<CreateNotificationCommand>.That.Matches(c =>
                c.Type == NotificationType.BookingReminder &&
                c.UserId == booking.UserId &&
                c.BookingId == booking.Id &&
                c.Message.Contains("Botox Panna")),
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _notificationService.SendBookingNotificationAsync(
            booking.UserId, A<string>._, A<string>._,
            NotificationType.BookingReminder, booking.Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _bookingRepository.UpdateAsync(
            A<BookingModel>.That.Matches(b => b.Id == booking.Id && b.ReminderSentAt != null)))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task Handle_QueriesWindowFromNowToLeadTime()
    {
        DateTime capturedStart = default, capturedEnd = default;
        A.CallTo(() => _bookingRepository.GetDueForReminderAsync(A<DateTime>._, A<DateTime>._))
            .Invokes((DateTime start, DateTime end) => { capturedStart = start; capturedEnd = end; })
            .Returns([]);

        await _handler.Handle(new SendDueBookingRemindersCommand(24), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(capturedEnd - capturedStart, Is.EqualTo(TimeSpan.FromHours(24)));
            Assert.That(capturedStart, Is.EqualTo(SwedishTime.Now).Within(TimeSpan.FromMinutes(1)),
                "fönstret ska utgå från svensk väggtid");
        });
    }

    [Test]
    public async Task Handle_WhenNothingIsDue_SendsNothing()
    {
        A.CallTo(() => _bookingRepository.GetDueForReminderAsync(A<DateTime>._, A<DateTime>._))
            .Returns([]);

        var result = await _handler.Handle(
            new SendDueBookingRemindersCommand(24), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(result.Data, Is.EqualTo(0));
        });
        A.CallTo(() => _notificationService.SendBookingNotificationAsync(
            A<string>._, A<string>._, A<string>._, A<NotificationType>._, A<Guid>._))
            .MustNotHaveHappened();
        A.CallTo(() => _bookingRepository.UpdateAsync(A<BookingModel>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task Handle_WhenOneBookingFails_OthersAreStillReminded()
    {
        var failing = DueBooking("user-fail");
        var healthy = DueBooking("user-ok");
        A.CallTo(() => _bookingRepository.GetDueForReminderAsync(A<DateTime>._, A<DateTime>._))
            .Returns([failing, healthy]);
        A.CallTo(() => _bookingRepository.UpdateAsync(
                A<BookingModel>.That.Matches(b => b.Id == failing.Id)))
            .Throws(new InvalidOperationException("db down"));

        var result = await _handler.Handle(
            new SendDueBookingRemindersCommand(24), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Successful, Is.True);
            Assert.That(result.Data, Is.EqualTo(1), "bara den lyckade räknas");
        });
        A.CallTo(() => _notificationService.SendBookingNotificationAsync(
            healthy.UserId, A<string>._, A<string>._,
            NotificationType.BookingReminder, healthy.Id)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _bookingRepository.UpdateAsync(
            A<BookingModel>.That.Matches(b => b.Id == healthy.Id)))
            .MustHaveHappenedOnceExactly();
    }
}
