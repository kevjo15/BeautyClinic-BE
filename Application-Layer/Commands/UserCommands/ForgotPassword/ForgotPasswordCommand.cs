using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.ForgotPassword
{
    public record ForgotPasswordCommand(ForgotPasswordDTO Request) : IRequest<OperationResult>;
}
