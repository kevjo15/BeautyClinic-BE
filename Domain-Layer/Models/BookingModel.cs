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

        /// <summary>Stripe payment-method-id för kortet som sparades vid bokning (no-show debiteras detta). Null = kortlös bokning.</summary>
        public string? StripePaymentMethodId { get; set; }

        /// <summary>Kortmärke för visning ("Visa"), aldrig fullt kortnummer.</summary>
        public string? CardBrand { get; set; }

        /// <summary>Kortets sista fyra siffror för visning.</summary>
        public string? CardLast4 { get; set; }

        /// <summary>När no-show-avgiften drogs. Null = ej debiterad. Idempotensspärr så avgiften inte dras två gånger.</summary>
        public DateTime? NoShowFeeChargedAt { get; set; }

        /// <summary>Betalningsstatus för onlinebetalning. None = betala på plats (kort-på-fil).</summary>
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.None;

        /// <summary>Belopp (kr) som faktiskt debiterats online (hela priset). 0 = inget dragits.</summary>
        public decimal AmountPaid { get; set; }

        /// <summary>Stripe PaymentIntent-id för onlinebetalningen (används vid återbetalning). Null = ingen onlinebetalning.</summary>
        public string? StripePaymentIntentId { get; set; }

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
        Cancelled,

        /// <summary>Utebliven (no-show). Sätts av personal på en passerad bokning; triggar no-show-avgift.</summary>
        NoShow
    }

    /// <summary>Onlinebetalningens status för en bokning.</summary>
    public enum PaymentStatus
    {
        /// <summary>Ingen onlinebetalning — kunden betalar på plats (ev. kort-på-fil för no-show).</summary>
        None,

        /// <summary>Hela beloppet betalt online.</summary>
        PaidInFull,

        /// <summary>Onlinebetalningen är återbetalad (avbokning i tid).</summary>
        Refunded
    }
}
