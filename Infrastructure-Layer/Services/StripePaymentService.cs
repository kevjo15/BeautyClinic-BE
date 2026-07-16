using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Infrastructure_Layer.Services
{
    /// <summary>
    /// Stripe.net-baserad implementation av kort-på-fil. Registreras endast när
    /// Stripe:SecretKey finns (annars NullStripePaymentService). All kortdata bor hos
    /// Stripe; vi hanterar bara customer-/paymentMethod-id och verifierade webhooks.
    /// </summary>
    public class StripePaymentService : IStripePaymentService
    {
        private readonly StripeClient _client;
        private readonly string _webhookSecret;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(string secretKey, string? webhookSecret, ILogger<StripePaymentService> logger)
        {
            _client = new StripeClient(secretKey);
            _webhookSecret = webhookSecret ?? string.Empty;
            _logger = logger;
        }

        public bool IsConfigured => true;

        public async Task<string> EnsureCustomerAsync(
            string userId, string? email, string? name, string? existingCustomerId, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(existingCustomerId))
            {
                return existingCustomerId;
            }

            var customerService = new CustomerService(_client);
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string> { ["userId"] = userId },
            }, cancellationToken: ct);

            return customer.Id;
        }

        public async Task<string> CreateSetupIntentAsync(string customerId, CancellationToken ct)
        {
            var service = new SetupIntentService(_client);
            var intent = await service.CreateAsync(new SetupIntentCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = ["card"],
                Usage = "off_session", // vi ska kunna dra kortet senare utan kunden närvarande
            }, cancellationToken: ct);

            return intent.ClientSecret;
        }

        public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
            string customerId, long amountMinorUnit, string currency, string description,
            IDictionary<string, string>? metadata, CancellationToken ct)
        {
            var service = new PaymentIntentService(_client);
            var intent = await service.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = amountMinorUnit,
                Currency = currency,
                Customer = customerId,
                Description = description,
                Metadata = metadata == null ? null : new Dictionary<string, string>(metadata),
                // Explicit lista: kort (inkl. Google Pay/Apple Pay-wallets) + Klarna + Amazon Pay.
                // Klarna och Amazon Pay är redirect-metoder — FE skickar return_url vid
                // confirmPayment och /payment-return slutför bokningen.
                PaymentMethodTypes = ["card", "klarna", "amazon_pay"],
                // Spara kortet på customern så det kan återanvändas / no-show-debiteras.
                // Sätts per metod: Klarna stödjer inte off_session-sparande.
                PaymentMethodOptions = new PaymentIntentPaymentMethodOptionsOptions
                {
                    Card = new PaymentIntentPaymentMethodOptionsCardOptions
                    {
                        SetupFutureUsage = "off_session",
                    },
                },
            }, cancellationToken: ct);

            return new PaymentIntentResult(intent.ClientSecret, intent.Id);
        }

        public async Task<PaymentIntentInfo?> GetPaymentIntentAsync(string paymentIntentId, CancellationToken ct)
        {
            try
            {
                // Expandera latest_charge: PI:ns status förblir "succeeded" efter refund,
                // så återbetalning måste läsas från chargen.
                var intent = await new PaymentIntentService(_client).GetAsync(paymentIntentId,
                    new PaymentIntentGetOptions { Expand = ["latest_charge"] }, cancellationToken: ct);
                var refunded = intent.LatestCharge is { } charge
                    && (charge.Refunded || charge.AmountRefunded > 0);
                return new PaymentIntentInfo(intent.Status, intent.AmountReceived, refunded,
                    intent.Metadata ?? new Dictionary<string, string>());
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Could not fetch payment intent {PaymentIntentId}", paymentIntentId);
                return null;
            }
        }

        public async Task<StripeRefundResult> RefundAsync(
            string paymentIntentId, long? amountMinorUnit, string idempotencyKey, CancellationToken ct)
        {
            try
            {
                var options = new RefundCreateOptions { PaymentIntent = paymentIntentId };
                if (amountMinorUnit.HasValue) options.Amount = amountMinorUnit.Value;

                await new RefundService(_client).CreateAsync(
                    options, new RequestOptions { IdempotencyKey = idempotencyKey }, ct);
                return new StripeRefundResult(true, null);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Refund failed for payment intent {PaymentIntentId}", paymentIntentId);
                return new StripeRefundResult(false, ex.StripeError?.Message ?? "Återbetalningen misslyckades.");
            }
        }

        public async Task<CardDetails?> GetCardDetailsAsync(string paymentMethodId, CancellationToken ct)
        {
            try
            {
                var service = new PaymentMethodService(_client);
                var pm = await service.GetAsync(paymentMethodId, cancellationToken: ct);
                return pm.Card == null ? null : new CardDetails(pm.Card.Brand, pm.Card.Last4);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Could not fetch card details for payment method {PaymentMethodId}", paymentMethodId);
                return null;
            }
        }

        public async Task<StripeChargeResult> ChargeOffSessionAsync(
            string customerId, string paymentMethodId, long amountMinorUnit, string currency,
            string description, IDictionary<string, string>? metadata, string idempotencyKey, CancellationToken ct)
        {
            try
            {
                var service = new PaymentIntentService(_client);
                var intent = await service.CreateAsync(new PaymentIntentCreateOptions
                {
                    Amount = amountMinorUnit,
                    Currency = currency,
                    Customer = customerId,
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    OffSession = true, // kunden är inte närvarande — Stripe hoppar 3DS där det går
                    Description = description,
                    Metadata = metadata == null ? null : new Dictionary<string, string>(metadata),
                }, new RequestOptions { IdempotencyKey = idempotencyKey }, ct);

                var succeeded = intent.Status == "succeeded";
                return new StripeChargeResult(succeeded, intent.Id, intent.Status,
                    succeeded ? null : $"Betalningen kunde inte slutföras (status: {intent.Status}).");
            }
            catch (StripeException ex)
            {
                // Vanligast: kortet nekades eller kräver 3DS som inte kan göras off-session.
                _logger.LogWarning(ex, "Off-session charge failed for customer {CustomerId}", customerId);
                return new StripeChargeResult(false, ex.StripeError?.PaymentIntent?.Id, "failed",
                    ex.StripeError?.Message ?? "Kortet kunde inte debiteras.");
            }
        }

        public StripeWebhookResult? ParseWebhook(string json, string signatureHeader)
        {
            try
            {
                // throwOnApiVersionMismatch: false → webhooken avvisas inte om Stripe-kontots
                // API-version skiljer sig från SDK:ns (annars kan riktiga events tyst felas).
                var stripeEvent = EventUtility.ConstructEvent(
                    json, signatureHeader, _webhookSecret, throwOnApiVersionMismatch: false);

                string? bookingId = null;
                string? paymentIntentId = null;
                if (stripeEvent.Data.Object is PaymentIntent pi)
                {
                    paymentIntentId = pi.Id;
                    pi.Metadata?.TryGetValue("bookingId", out bookingId);
                }

                return new StripeWebhookResult(stripeEvent.Type, paymentIntentId, bookingId, null);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Rejected Stripe webhook with invalid signature");
                return null;
            }
        }
    }
}
