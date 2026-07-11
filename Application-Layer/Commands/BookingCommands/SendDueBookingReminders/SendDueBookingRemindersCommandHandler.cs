using Application_Layer.Commands.NotificationCommands.CreateNotification;
using Application_Layer.Interfaces;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.BookingCommands.SendDueBookingReminders
{
    public class SendDueBookingRemindersCommandHandler
        : IRequestHandler<SendDueBookingRemindersCommand, OperationResult<int>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMediator _mediator;
        private readonly INotificationService _notificationService;
        private readonly ILogger<SendDueBookingRemindersCommandHandler> _logger;

        public SendDueBookingRemindersCommandHandler(
            IBookingRepository bookingRepository,
            IMediator mediator,
            INotificationService notificationService,
            ILogger<SendDueBookingRemindersCommandHandler> logger)
        {
            _bookingRepository = bookingRepository;
            _mediator = mediator;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<OperationResult<int>> Handle(
            SendDueBookingRemindersCommand request, CancellationToken cancellationToken)
        {
            var now = SwedishTime.Now;
            var dueBookings = await _bookingRepository.GetDueForReminderAsync(
                now, now.AddHours(request.LeadTimeHours));

            var sent = 0;
            foreach (var booking in dueBookings)
            {
                // Per bokning: ett fel får inte stoppa övriga påminnelser. Vid krasch
                // mellan utskick och markering skickas påminnelsen om vid nästa tick —
                // en dubblett är mindre illa än en missad påminnelse (en instans i drift).
                try
                {
                    var serviceName = booking.Service?.Name ?? "din behandling";
                    var title = "Påminnelse om din bokning";
                    var message = $"Påminnelse: din bokning för {serviceName} " +
                                  $"({booking.StartTime:yyyy-MM-dd HH:mm}) närmar sig. Välkommen!";

                    // Skicka den flyktiga notisen (SignalR/e-post/SMS) först. Kastar
                    // den försvinner inget — ReminderSentAt förblir null och nästa
                    // tick försöker om, UTAN att en dubblett-notis redan hunnit sparas.
                    await _notificationService.SendBookingNotificationAsync(
                        booking.UserId,
                        title,
                        message,
                        NotificationType.BookingReminder,
                        booking.Id);

                    // Persistera bell-notisen och markera bokningen som påmind.
                    // En krasch mellan dessa två kan ge en dubblett vid nästa tick
                    // — accepterat (dubblett hellre än missad påminnelse).
                    var notificationCmd = new CreateNotificationCommand
                    {
                        Title = title,
                        Message = message,
                        UserId = booking.UserId,
                        Type = NotificationType.BookingReminder,
                        BookingId = booking.Id,
                    };
                    await _mediator.Send(notificationCmd, cancellationToken);

                    booking.ReminderSentAt = now;
                    await _bookingRepository.UpdateAsync(booking);
                    sent++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to send booking reminder for booking {BookingId}", booking.Id);
                }
            }

            if (sent > 0)
            {
                _logger.LogInformation("Sent {Count} booking reminder(s)", sent);
            }

            return OperationResult<int>.Success(sent);
        }
    }
}
