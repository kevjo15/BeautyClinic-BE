using Application_Layer.DTOs;
using MediatR;
using System.Collections.Generic;

public class GetBookingsByUserIdQuery : IRequest<List<BookingDTO>>
{
    public string UserId { get; }

    public GetBookingsByUserIdQuery(string userId)
    {
        UserId = userId;
    }
} 