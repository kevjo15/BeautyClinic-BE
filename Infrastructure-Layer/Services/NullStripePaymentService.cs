using Application_Layer.DTOs;
using Application_Layer.Interfaces;

namespace Infrastructure_Layer.Services
{
    /// <summary>
    /// Används när Stripe inte är konfigurerat (t.ex. lokal dev utan nycklar). Bokning
    /// fungerar då kortlöst och kort-steget döljs i frontend (IsConfigured = false).
    /// Faktiska betaloperationer ska aldrig anropas i det läget.
    /// </summary>
    public sealed class NullStripePaymentService : IStripePaymentService
    {
        public bool IsConfigured => false;

        public Task<string> EnsureCustomerAsync(string userId, string? email, string? name, string? existingCustomerId, CancellationToken ct)
            => throw new InvalidOperationException("Stripe is not configured.");

        public Task<string> CreateSetupIntentAsync(string customerId, CancellationToken ct)
            => throw new InvalidOperationException("Stripe is not configured.");

        public Task<PaymentIntentResult> CreatePaymentIntentAsync(
            string customerId, long amountMinorUnit, string currency, string description,
            IDictionary<string, string>? metadata, CancellationToken ct)
            => throw new InvalidOperationException("Stripe is not configured.");

        public Task<PaymentIntentInfo?> GetPaymentIntentAsync(string paymentIntentId, CancellationToken ct)
            => Task.FromResult<PaymentIntentInfo?>(null);

        public Task<StripeRefundResult> RefundAsync(
            string paymentIntentId, long? amountMinorUnit, string idempotencyKey, CancellationToken ct)
            => throw new InvalidOperationException("Stripe is not configured.");

        public Task<CardDetails?> GetCardDetailsAsync(string paymentMethodId, CancellationToken ct)
            => Task.FromResult<CardDetails?>(null);

        public Task<StripeChargeResult> ChargeOffSessionAsync(
            string customerId, string paymentMethodId, long amountMinorUnit, string currency,
            string description, IDictionary<string, string>? metadata, string idempotencyKey, CancellationToken ct)
            => throw new InvalidOperationException("Stripe is not configured.");

        public StripeWebhookResult? ParseWebhook(string json, string signatureHeader) => null;
    }
}
