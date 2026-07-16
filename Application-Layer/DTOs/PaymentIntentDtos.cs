namespace Application_Layer.DTOs
{
    /// <summary>Body för att skapa en PaymentIntent vid bokning (hela priset betalas online).</summary>
    public class PaymentIntentRequestDTO
    {
        public Guid ServiceId { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    /// <summary>Svar med clientSecret + belopp (kr) som ska betalas.</summary>
    public class PaymentIntentResponseDTO
    {
        public string ClientSecret { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    /// <summary>Body för att slutföra en bokning efter en genomförd onlinebetalning (redirect-retur).</summary>
    public class FinalizePaymentDTO
    {
        public string PaymentIntentId { get; set; } = string.Empty;
    }
}
