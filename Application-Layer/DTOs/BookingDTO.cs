namespace Application_Layer.DTOs
{
    public class BookingUserDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class BookingDTO
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? EmployeeId { get; set; }
        public Guid? ConversationId { get; set; }

        // Flat strings kept for backward compat
        public string? CustomerName { get; set; }
        public string? EmployeeName { get; set; }
        public string? ServiceName { get; set; }

        // Livscykel + kort-på-fil-status (för FE-visning)
        public string Status { get; set; } = "Active";
        public bool HasSavedCard { get; set; }
        public string? CardBrand { get; set; }
        public string? CardLast4 { get; set; }

        // Onlinebetalning
        public string PaymentStatus { get; set; } = "None";
        public decimal AmountPaid { get; set; }

        // Nested objects for full access
        public BookingUserDTO? User { get; set; }
        public BookingUserDTO? Employee { get; set; }
    }
}
