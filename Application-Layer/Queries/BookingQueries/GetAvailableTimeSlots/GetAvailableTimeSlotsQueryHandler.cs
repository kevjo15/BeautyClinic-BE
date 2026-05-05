using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;

namespace Application_Layer.Queries.BookingQueries.GetAvailableTimeSlots;

public class GetAvailableTimeSlotsQueryHandler : IRequestHandler<GetAvailableTimeSlotsQuery, List<AvailableTimeSlotDTO>>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IEmployeeWorkDayRepository _workDayRepository;

    public GetAvailableTimeSlotsQueryHandler(
        IBookingRepository bookingRepository,
        IServiceRepository serviceRepository,
        IEmployeeWorkDayRepository workDayRepository)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _workDayRepository = workDayRepository;
    }

    public async Task<List<AvailableTimeSlotDTO>> Handle(GetAvailableTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
        if (service == null)
            throw new KeyNotFoundException($"Service with ID {request.ServiceId} was not found.");

        var fromDateOnly = DateOnly.FromDateTime(request.StartDate);
        var toDateOnly = DateOnly.FromDateTime(request.EndDate);

        var workDays = await _workDayRepository.GetByEmployeeAndRangeAsync(request.EmployeeId, fromDateOnly, toDateOnly);
        var workDayByDate = workDays.ToDictionary(w => w.Date);

        var existingBookings = await _bookingRepository.GetByEmployeeAndRangeAsync(
            request.EmployeeId, request.StartDate, request.EndDate);

        var result = new List<AvailableTimeSlotDTO>();

        for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            var dateOnly = DateOnly.FromDateTime(date);
            if (!workDayByDate.TryGetValue(dateOnly, out var workDay))
                continue;

            var daySlots = new AvailableTimeSlotDTO
            {
                Date = date,
                AvailableSlots = new List<TimeSlot>()
            };

            var currentTime = date.Date + workDay.StartTime;
            var workEnd = date.Date + workDay.EndTime;

            while (currentTime.Add(service.Duration) <= workEnd)
            {
                var slotEnd = currentTime.Add(service.Duration);
                var isSlotAvailable = !existingBookings.Any(booking =>
                    (currentTime >= booking.StartTime && currentTime < booking.EndTime) ||
                    (slotEnd > booking.StartTime && slotEnd <= booking.EndTime) ||
                    (currentTime <= booking.StartTime && slotEnd >= booking.EndTime));

                daySlots.AvailableSlots.Add(new TimeSlot
                {
                    StartTime = currentTime,
                    EndTime = slotEnd,
                    IsAvailable = isSlotAvailable
                });

                currentTime = currentTime.AddMinutes(30);
            }

            if (daySlots.AvailableSlots.Any(s => s.IsAvailable))
                result.Add(daySlots);
        }

        return result;
    }
}
