namespace Application_Layer.DTOs
{
    public class CreateBookingDTO
    {
        public string UserId { get; set; } = string.Empty;
        public Guid ServiceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string EmployeeId { get; set; } = string.Empty;

        /// <summary>Stripe payment-method-id från det sparade kortet (betala på plats/kort-på-fil).</summary>
        public string? PaymentMethodId { get; set; }

        /// <summary>Stripe PaymentIntent-id om kunden betalade hela priset online.</summary>
        public string? PaymentIntentId { get; set; }
    }
}
