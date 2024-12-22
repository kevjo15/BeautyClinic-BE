using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommand : IRequest<CreateBookingResult>
    {
        public CreateBookingDTO Booking { get; }

        public CreateBookingCommand(CreateBookingDTO booking)
        {
            Booking = booking;
        }
    }
}
