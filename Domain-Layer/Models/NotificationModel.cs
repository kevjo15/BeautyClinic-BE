namespace Domain_Layer.Models;

public class NotificationModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public NotificationType Type { get; set; }
    public string UserId { get; set; }
    public UserModel User { get; set; }
    public Guid? BookingId { get; set; }
    public BookingModel? Booking { get; set; }
}


public enum NotificationType
{
    BookingReminder,
    BookingConfirmation,
    BookingCancellation,
    BookingUpdated,
    MessageReceived
}
