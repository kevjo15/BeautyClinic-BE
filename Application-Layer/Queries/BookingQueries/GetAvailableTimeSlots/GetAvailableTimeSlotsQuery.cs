using MediatR;
using Application_Layer.DTOs;

namespace Application_Layer.Queries.BookingQueries.GetAvailableTimeSlots
{
    public class GetAvailableTimeSlotsQuery : IRequest<List<AvailableTimeSlotDTO>>
    {
        public Guid ServiceId { get; set; }
        public string EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public GetAvailableTimeSlotsQuery(Guid serviceId, string employeeId, DateTime? startDate = null)
        {
            ServiceId = serviceId;
            EmployeeId = employeeId;
            StartDate = (startDate ?? DateTime.Today).Date;
            EndDate = StartDate.AddDays(7);
        }

        public GetAvailableTimeSlotsQuery(Guid serviceId, string employeeId, DateTime startDate, DateTime endDate)
        {
            ServiceId = serviceId;
            EmployeeId = employeeId;
            StartDate = startDate.Date;
            EndDate = endDate.Date;
        }
    }
}
