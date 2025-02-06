using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.BookingQueries.GetAvailableTimeSlots
{
    public class GetAvailableTimeSlotsQuery : IRequest<List<AvailableTimeSlotDTO>>
    {
        public Guid ServiceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public GetAvailableTimeSlotsQuery(Guid serviceId, DateTime? startDate = null)
        {
            ServiceId = serviceId;
            StartDate = (startDate ?? DateTime.Today).Date;
            EndDate = StartDate.AddDays(7); // Get a week's worth of slots by default
        }

        public GetAvailableTimeSlotsQuery(Guid serviceId, DateTime startDate, DateTime endDate)
        {
            ServiceId = serviceId;
            StartDate = startDate.Date;
            EndDate = endDate.Date;
        }
    }
}
