using Application_Layer.DTOs;

namespace Application_Layer.Interfaces
{
    /// <summary>
    /// Stripe-integration för kort-på-fil: spara kort utan debitering (SetupIntent) vid bokning,
    /// och dra no-show-avgift off-session från sparat kort. Stripe håller korten — vi lagrar bara
    /// referens-id (customer/paymentMethod). Utan konfigurerade nycklar körs NullStripePaymentService.
    /// </summary>
    public interface IStripePaymentService
    {
        /// <summary>Är Stripe konfigurerat (nycklar satta)? Styr om kort-steget krävs vid bokning.</summary>
        bool IsConfigured { get; }

        /// <summary>Hämtar befintlig eller skapar en Stripe-customer för användaren. Returnerar customerId.</summary>
        Task<string> EnsureCustomerAsync(string userId, string? email, string? name, string? existingCustomerId, CancellationToken ct);

        /// <summary>Skapar en SetupIntent (spara kort, 0 kr) för customern. Returnerar clientSecret till frontend.</summary>
        Task<string> CreateSetupIntentAsync(string customerId, CancellationToken ct);

        /// <summary>Skapar en PaymentIntent (debitera belopp online vid bokning). Returnerar clientSecret + id.</summary>
        Task<PaymentIntentResult> CreatePaymentIntentAsync(
            string customerId, long amountMinorUnit, string currency, string description,
            IDictionary<string, string>? metadata, CancellationToken ct);

        /// <summary>Läser en PaymentIntents status/belopp — för serverside-verifiering att betalningen gick igenom.</summary>
        Task<PaymentIntentInfo?> GetPaymentIntentAsync(string paymentIntentId, CancellationToken ct);

        /// <summary>Återbetalar en tidigare onlinebetalning (helt om amountMinorUnit är null).</summary>
        Task<StripeRefundResult> RefundAsync(
            string paymentIntentId, long? amountMinorUnit, string idempotencyKey, CancellationToken ct);

        /// <summary>Hämtar kortmärke + sista fyra för ett sparat betalningssätt (för visning).</summary>
        Task<CardDetails?> GetCardDetailsAsync(string paymentMethodId, CancellationToken ct);

        /// <summary>
        /// Drar ett belopp (minsta enhet, t.ex. öre) off-session från sparat kort.
        /// idempotencyKey gör att ett omförsök (t.ex. efter att DB-uppdateringen
        /// misslyckats mellan debitering och statusändring) inte dubbeldebiterar —
        /// Stripe återanvänder samma PaymentIntent för samma nyckel.
        /// </summary>
        Task<StripeChargeResult> ChargeOffSessionAsync(
            string customerId, string paymentMethodId, long amountMinorUnit, string currency,
            string description, IDictionary<string, string>? metadata, string idempotencyKey, CancellationToken ct);

        /// <summary>Verifierar signatur och tolkar en webhook-payload. Null vid ogiltig signatur.</summary>
        StripeWebhookResult? ParseWebhook(string json, string signatureHeader);
    }
}
