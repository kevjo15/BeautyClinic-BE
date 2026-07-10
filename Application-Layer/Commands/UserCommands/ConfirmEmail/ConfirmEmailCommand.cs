using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.ConfirmEmail
{
    public record ConfirmEmailCommand(ConfirmEmailDTO Request) : IRequest<OperationResult>;
}
