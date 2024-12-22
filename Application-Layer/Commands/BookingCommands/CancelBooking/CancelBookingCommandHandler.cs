using MediatR;
using Application_Layer.Interfaces;

namespace Application_Layer.Commands.BookingCommands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
    {
        private readonly IBookingRepository _bookingRepository;

        public CancelBookingCommandHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.Id);
            
            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {request.Id} was not found.");

            // Check if the booking is in the future
            if (booking.StartTime <= DateTime.Now)
                throw new InvalidOperationException("Cannot cancel a booking that has already started or completed.");

            await _bookingRepository.DeleteAsync(request.Id);
            return true;
        }
    }
}
