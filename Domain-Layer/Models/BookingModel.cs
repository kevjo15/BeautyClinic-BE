namespace Domain_Layer.Models
{
    public class BookingModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? EmployeeId { get; set; }
        public Guid? ConversationId { get; set; }

        // Navigation properties
        public UserModel? User { get; set; }
        public UserModel? Employee { get; set; }
        public ServiceModel? Service { get; set; }
    }
}
