using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.PaymentCommands.CreatePaymentIntent
{
    /// <summary>
    /// Skapar en Stripe PaymentIntent för onlinebetalning (hela priset) vid bokning.
    /// Bokningsuppgifterna (tid/personal) läggs i PI-metadatan så betalningen kan
    /// stämmas av till en bokning även via webhook om kunden aldrig kommer tillbaka.
    /// </summary>
    public sealed record CreatePaymentIntentCommand(
        string UserId, Guid ServiceId, string EmployeeId, DateTime StartTime, DateTime EndTime)
        : IRequest<OperationResult<PaymentIntentResponseDTO>>;
}
