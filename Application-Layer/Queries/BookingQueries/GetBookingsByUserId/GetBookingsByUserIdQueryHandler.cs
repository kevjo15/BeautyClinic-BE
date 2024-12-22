using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application_Layer.Interfaces;
using AutoMapper;
using Application_Layer.DTOs;

public class GetBookingsByUserIdQueryHandler : IRequestHandler<GetBookingsByUserIdQuery, List<BookingDTO>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper;

    public GetBookingsByUserIdQueryHandler(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<List<BookingDTO>> Handle(GetBookingsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(request.UserId);
        return _mapper.Map<List<BookingDTO>>(bookings);
    }
} 