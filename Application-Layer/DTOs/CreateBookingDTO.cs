namespace Application_Layer.DTOs
{
    public class CreateBookingDTO
    {
        public string UserId { get; set; }
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
