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

// Enum för att definiera olika typer av notifikationer
public enum NotificationType
{
    BookingReminder,    // För påminnelser om bokningar
    BookingConfirmation,// För bekräftelse av ny bokning
    BookingCancellation,// För avbokningar
    MessageReceived     // För nya chattmeddelanden
}
