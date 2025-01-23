public class BookingModel
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public Guid ServiceId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}