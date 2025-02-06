using Application_Layer.DTOs;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.UpdateBooking
{
public class UpdateBookingCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public UpdateBookingDTO Booking { get; set; }

    public UpdateBookingCommand(Guid id, UpdateBookingDTO booking)
    {
        Id = id;
        Booking = booking;
    }
}
}
