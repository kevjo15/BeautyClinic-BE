using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Mapping;
using Domain_Layer.Common;

namespace Application_Layer.Queries.BookingQueries.GetBookingById
{
    public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, OperationResult<BookingDTO>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationMapper _mapper;

        public GetBookingByIdQueryHandler(IBookingRepository bookingRepository, IApplicationMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<OperationResult<BookingDTO>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.Id);

            if (booking == null)
            {
                return OperationResult<BookingDTO>.Failure(
                    $"Booking with ID {request.Id} was not found.",
                    OperationFailureType.NotFound);
            }

            if (!request.CanManageBooking && booking.UserId != request.RequestingUserId)
            {
                return OperationResult<BookingDTO>.Failure(
                    "You do not have access to this booking.",
                    OperationFailureType.Forbidden);
            }

            return OperationResult<BookingDTO>.Success(_mapper.ToBookingDto(booking));
        }
    }
}
