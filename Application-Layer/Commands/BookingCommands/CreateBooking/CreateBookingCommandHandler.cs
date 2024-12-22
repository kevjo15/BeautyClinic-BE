using MediatR;
using AutoMapper;
using Domain_Layer.Models;
using Application_Layer.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Application_Layer.Commands.BookingCommands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IMapper _mapper;

        public CreateBookingCommandHandler(IBookingRepository bookingRepository, IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Map DTO to domain model
                var booking = _mapper.Map<BookingModel>(request.Booking);
                booking.Id = Guid.NewGuid();
                booking.UserId = request.Booking.UserId;

                // Save booking to repository
                await _bookingRepository.AddAsync(booking);

                // Return success result
                return CreateBookingResult.SuccessResult("Booking created successfully.", booking);
            }
            catch (Exception ex)
            {
                // Return failure result
                return CreateBookingResult.FailureResult($"Failed to create booking: {ex.Message}");
            }
        }
    }
} 