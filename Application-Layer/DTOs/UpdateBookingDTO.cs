namespace Application_Layer.DTOs
{
    public class UpdateBookingDTO
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid ServiceId { get; set; }
    }
}
