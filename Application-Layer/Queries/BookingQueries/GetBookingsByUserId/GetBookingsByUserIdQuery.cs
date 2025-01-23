using Application_Layer.DTOs;
using MediatR;

public class GetBookingsByUserIdQuery : IRequest<List<BookingDTO>>
{
    public string UserId { get; }

    public GetBookingsByUserIdQuery(string userId)
    {
        UserId = userId;
    }
}