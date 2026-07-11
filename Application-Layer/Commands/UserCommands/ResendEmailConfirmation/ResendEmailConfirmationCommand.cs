using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.ResendEmailConfirmation
{
    public record ResendEmailConfirmationCommand(ResendConfirmationDTO Request) : IRequest<OperationResult>;
}
