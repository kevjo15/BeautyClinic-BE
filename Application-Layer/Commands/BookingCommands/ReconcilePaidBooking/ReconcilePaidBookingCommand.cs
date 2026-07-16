using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;

namespace Application_Layer.Commands.BookingCommands.ReconcilePaidBooking
{
    /// <summary>
    /// Skapar bokningen för en redan genomförd onlinebetalning, utifrån bokningsuppgifterna
    /// i PaymentIntent-metadatan. Anropas av /payment-return (kunden kom tillbaka) OCH av
    /// Stripe-webhooken (orphan-skydd om kunden aldrig kom tillbaka). Idempotent — samma
    /// betalning ger bara en bokning.
    /// RequestingUserId = inloggad användare (måste äga betalningen); null = webhooken
    /// (Stripe-signaturen är auktoriseringen, metadatan avgör ägaren).
    /// </summary>
    public sealed record ReconcilePaidBookingCommand(string PaymentIntentId, string? RequestingUserId)
        : IRequest<OperationResult<BookingModel>>;
}
