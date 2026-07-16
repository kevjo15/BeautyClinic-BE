using Application_Layer.Commands.PaymentCommands.CreatePaymentIntent;
using Application_Layer.Commands.PaymentCommands.CreateSetupIntent;
using Application_Layer.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/payments")]
[ApiController]
[Authorize]
public class PaymentController : BaseApiController
{
    private readonly IMediator _mediator;

    public PaymentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Skapar en Stripe SetupIntent så att den inloggade kunden kan spara ett kort (0 kr dras).
    /// Frontend använder clientSecret för att bekräfta kortet via Stripe.js.
    /// </summary>
    [HttpPost("setup-intent")]
    public async Task<IActionResult> CreateSetupIntent(CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var result = await _mediator.Send(new CreateSetupIntentCommand(userId), ct);
        return HandleResult(result);
    }

    /// <summary>
    /// Skapar en PaymentIntent för onlinebetalning vid bokning (hela priset).
    /// Frontend bekräftar betalningen med clientSecret via Stripe.js.
    /// </summary>
    [HttpPost("payment-intent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequestDTO body, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var result = await _mediator.Send(
            new CreatePaymentIntentCommand(userId, body.ServiceId, body.EmployeeId, body.StartTime, body.EndTime), ct);
        return HandleResult(result);
    }
}
