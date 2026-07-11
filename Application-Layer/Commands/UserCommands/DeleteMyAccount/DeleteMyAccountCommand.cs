using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.DeleteMyAccount
{
    /// <summary>
    /// GDPR-radering av det inloggade kontot: rensar avatar, återkallar sessioner
    /// och anonymiserar användaren. Bokningar behålls (avidentifierade).
    /// </summary>
    public sealed record DeleteMyAccountCommand(string UserId) : IRequest<OperationResult>;
}
