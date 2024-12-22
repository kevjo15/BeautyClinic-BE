namespace Application_Layer.DTOs
{
    public class AvailableTimeSlotDTO
    {
        public DateTime Date { get; set; }
        public List<TimeSlot> AvailableSlots { get; set; } = new();
    }

    public class TimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
