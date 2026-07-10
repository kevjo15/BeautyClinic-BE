using MediatR;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using Application_Layer.DTOs;

public class GetBookingsByUserIdQueryHandler : IRequestHandler<GetBookingsByUserIdQuery, List<BookingDTO>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IApplicationMapper _mapper;

    public GetBookingsByUserIdQueryHandler(IBookingRepository bookingRepository, IApplicationMapper mapper)
    {
        _bookingRepository = bookingRepository;
        _mapper = mapper;
    }

    public async Task<List<BookingDTO>> Handle(GetBookingsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _bookingRepository.GetByUserIdAsync(request.UserId);
        return _mapper.ToBookingDtoList(bookings);
    }
}