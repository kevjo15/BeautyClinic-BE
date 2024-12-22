using MediatR;
using AutoMapper; // Add AutoMapper using directive
using Application_Layer.Interfaces;
using Domain_Layer.Models;

namespace Application_Layer.Commands.BookingCommands.UpdateBooking
{
public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, bool>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IMapper _mapper; // Inject AutoMapper

    public UpdateBookingCommandHandler(IBookingRepository bookingRepository, IMapper mapper)
    {
        _bookingRepository = bookingRepository; // Initialize repository
        _mapper = mapper; // Initialize AutoMapper
    }

    public async Task<bool> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepository.GetByIdAsync(request.Id);
        
        if (booking == null)
            throw new KeyNotFoundException($"Booking with ID {request.Id} was not found.");

        // Use AutoMapper to map properties
        _mapper.Map(request.Booking, booking);

        await _bookingRepository.UpdateAsync(booking);
        return true;
    }
}
}
