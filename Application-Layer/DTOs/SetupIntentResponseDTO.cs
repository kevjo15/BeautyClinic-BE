namespace Application_Layer.DTOs
{
    /// <summary>Svar på skapad SetupIntent — clientSecret som frontend använder för att spara kortet via Stripe.js.</summary>
    public class SetupIntentResponseDTO
    {
        public string ClientSecret { get; set; } = string.Empty;
    }
}
