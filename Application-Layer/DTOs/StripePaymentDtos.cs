namespace Application_Layer.DTOs
{
    /// <summary>Kortmärke + sista fyra siffror, för visning ("Visa ••4242"). Aldrig fullt kortnummer.</summary>
    public sealed record CardDetails(string Brand, string Last4);

    /// <summary>Resultat av en off-session-debitering (no-show-avgift).</summary>
    public sealed record StripeChargeResult(bool Succeeded, string? PaymentIntentId, string? Status, string? Error);

    /// <summary>Nyskapad PaymentIntent för onlinebetalning vid bokning.</summary>
    public sealed record PaymentIntentResult(string ClientSecret, string PaymentIntentId);

    /// <summary>
    /// Läst status för en PaymentIntent (serverside-verifiering av att betalning skett).
    /// Metadata bär bokningsuppgifterna (serverside-satta) så en betalning kan stämmas av
    /// till en bokning även via webhook (kund som stänger fliken mitt i en redirect).
    /// Refunded behövs för att Stripe låter status vara "succeeded" även efter återbetalning
    /// — utan flaggan kunde en redan återbetald betalning ge en ny bokning.
    /// </summary>
    public sealed record PaymentIntentInfo(
        string Status, long AmountReceivedMinorUnit, bool Refunded,
        IReadOnlyDictionary<string, string> Metadata);

    /// <summary>Resultat av en återbetalning.</summary>
    public sealed record StripeRefundResult(bool Succeeded, string? Error);

    /// <summary>
    /// Neutral projektion av en verifierad Stripe-webhook-event — så att Stripe-SDK-typer
    /// aldrig läcker ut i API-/Application-lagret. Null returneras vid ogiltig signatur.
    /// </summary>
    public sealed record StripeWebhookResult(string Type, string? PaymentIntentId, string? BookingId, string? Error);
}
