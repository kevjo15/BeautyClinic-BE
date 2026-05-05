using Application_Layer.DTOs;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.UpdateBooking
{
public class UpdateBookingCommand : IRequest<OperationResult<BookingModel>>
{
    public Guid Id { get; set; }
    public string RequestingUserId { get; set; }
    public bool CanManageBooking { get; set; }
    public UpdateBookingDTO Booking { get; set; }

    public UpdateBookingCommand(Guid id, string requestingUserId, bool canManageBooking, UpdateBookingDTO booking)
    {
        Id = id;
        RequestingUserId = requestingUserId;
        CanManageBooking = canManageBooking;
        Booking = booking;
    }
}
}
