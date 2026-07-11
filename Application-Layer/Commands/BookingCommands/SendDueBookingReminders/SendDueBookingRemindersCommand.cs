using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.SendDueBookingReminders
{
    /// <summary>
    /// Skickar påminnelser (in-app + mejl/SMS via notistjänsten) för aktiva bokningar
    /// som startar inom <paramref name="LeadTimeHours"/> timmar och inte redan påmints.
    /// Skickas periodiskt av <c>BookingReminderWorker</c>. Returnerar antal skickade.
    /// </summary>
    public record SendDueBookingRemindersCommand(int LeadTimeHours) : IRequest<OperationResult<int>>;
}
