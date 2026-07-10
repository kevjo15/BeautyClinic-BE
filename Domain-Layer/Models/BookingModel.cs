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

        /// <summary>
        /// Livscykelstatus. Avbokning är en mjuk radering (soft delete): bokningen
        /// får status <see cref="BookingStatus.Cancelled"/> istället för att tas bort,
        /// så att avbokningar går att följa upp i adminrapporten. Operativa queries
        /// (mina bokningar, personalens schema, konfliktkontroll) filtrerar bort
        /// avbokade rader så att beteendet är identiskt med tidigare hård radering.
        /// </summary>
        public BookingStatus Status { get; set; } = BookingStatus.Active;

        /// <summary>
        /// När påminnelsen inför bokningen skickades (svensk väggtid). Null = ännu
        /// inte skickad. Fungerar som exakt-en-gång-spärr för påminnelsejobbet.
        /// </summary>
        public DateTime? ReminderSentAt { get; set; }

        // Navigation properties
        public UserModel? User { get; set; }
        public UserModel? Employee { get; set; }
        public ServiceModel? Service { get; set; }
    }

    public enum BookingStatus
    {
        /// <summary>Aktiv bokning (kommande eller genomförd).</summary>
        Active,

        /// <summary>Avbokad. Behålls i databasen för uppföljning/rapportering.</summary>
        Cancelled
    }
}
