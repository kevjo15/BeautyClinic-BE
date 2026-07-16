using Application_Layer.Commands.BookingCommands.ReconcilePaidBooking;
using Application_Layer.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

/// <summary>
/// Tar emot Stripe-webhooks. Anonym (Stripe autentiserar via signatur, inte JWT) och
/// läser rå body för signaturverifiering — därför en egen controller utanför [Authorize].
/// Orphan-skydd: vid payment_intent.succeeded stäms betalningen av mot en bokning, så
/// en kund som betalar via redirect (Klarna) men aldrig kommer tillbaka ändå får sin
/// bokning (eller en automatisk återbetalning om tiden hunnit tas).
/// </summary>
[Route("api/stripe")]
[ApiController]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly IStripePaymentService _stripe;
    private readonly IMediator _mediator;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        IStripePaymentService stripe, IMediator mediator, ILogger<StripeWebhookController> logger)
    {
        _stripe = stripe;
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Handle()
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = _stripe.ParseWebhook(json, signature);
        if (result == null)
        {
            // Ogiltig signatur (eller Stripe ej konfigurerat) → avvisa.
            return BadRequest();
        }

        switch (result.Type)
        {
            case "payment_intent.succeeded" when !string.IsNullOrWhiteSpace(result.PaymentIntentId):
                // Säkerställ att betalningen har en bokning. Idempotent: om /payment-return
                // redan skapat den svarar avstämningen "redan kopplad" (Conflict) och vi
                // struntar i det. Saknas bokningen skapas den nu (eller auto-återbetalas
                // om tiden tagits).
                // RequestingUserId=null: webhooken agerar som systemet — Stripe-signaturen
                // är auktoriseringen och PI-metadatan avgör vem bokningen tillhör.
                var reconcile = await _mediator.Send(new ReconcilePaidBookingCommand(result.PaymentIntentId!, null));
                if (reconcile.Successful)
                {
                    _logger.LogInformation(
                        "Webhook skapade bokning för betalning {PaymentIntentId}", result.PaymentIntentId);
                }
                else
                {
                    _logger.LogInformation(
                        "Webhook-avstämning för {PaymentIntentId}: {Reason}",
                        result.PaymentIntentId, reconcile.Error);
                }
                break;
            case "payment_intent.payment_failed":
                _logger.LogWarning("Stripe payment failed for payment {PaymentIntentId}", result.PaymentIntentId);
                break;
            default:
                _logger.LogInformation("Received Stripe event {EventType}", result.Type);
                break;
        }

        // Kvittera alltid 200 för verifierade events så Stripe inte gör om leveransen.
        return Ok();
    }
}
