using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.PaymentCommands.CreateSetupIntent
{
    /// <summary>Skapar en Stripe SetupIntent så att den inloggade användaren kan spara ett kort (0 kr).</summary>
    public sealed record CreateSetupIntentCommand(string UserId) : IRequest<OperationResult<SetupIntentResponseDTO>>;
}
