using MediatR;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using Domain_Layer.Models;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommand : IRequest<OperationResult<BookingModel>>
    {
        public CreateBookingDTO Booking { get; }

        public CreateBookingCommand(CreateBookingDTO booking)
        {
            Booking = booking;
        }
    }
}
