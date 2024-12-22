using MediatR;
using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Domain_Layer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure_Layer.Repositories.Service;

namespace Application_Layer.Queries.BookingQueries.GetAvailableTimeSlots
{
    public class GetAvailableTimeSlotsQueryHandler : IRequestHandler<GetAvailableTimeSlotsQuery, List<AvailableTimeSlotDTO>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;

        public GetAvailableTimeSlotsQueryHandler(
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository)
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<List<AvailableTimeSlotDTO>> Handle(GetAvailableTimeSlotsQuery request, CancellationToken cancellationToken)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
            if (service == null)
            {
                throw new KeyNotFoundException($"Service with ID {request.ServiceId} was not found.");
            }

            // Get all bookings for the requested date range
            var existingBookings = await _bookingRepository.GetByDateRangeAsync(request.StartDate, request.EndDate);
            var result = new List<AvailableTimeSlotDTO>();

            // For each day in the range
            for (var date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
            {
                var daySlots = new AvailableTimeSlotDTO
                {
                    Date = date,
                    AvailableSlots = new List<TimeSlot>()
                };

                var currentTime = date.AddHours(9); // Start at 9 AM
                var endTime = date.AddHours(17);    // End at 5 PM

                while (currentTime.Add(service.Duration) <= endTime)
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

                    currentTime = currentTime.AddMinutes(30); // 30-minute intervals
                }

                // Only add days that have at least one available slot
                if (daySlots.AvailableSlots.Any(s => s.IsAvailable))
                {
                    result.Add(daySlots);
                }
            }

            return result;
        }
    }
}
