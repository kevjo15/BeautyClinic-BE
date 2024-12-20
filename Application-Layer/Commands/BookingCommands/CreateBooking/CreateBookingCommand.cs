using MediatR;
using Application_Layer.DTO_s;

public class CreateBookingCommand : IRequest<Guid>
{
    public BookingDTO BookingDto { get; }

    public CreateBookingCommand(BookingDTO bookingDto)
    {
        BookingDto = bookingDto;
    }
} 