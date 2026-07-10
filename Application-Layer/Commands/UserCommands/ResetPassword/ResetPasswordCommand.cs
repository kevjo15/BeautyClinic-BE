using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.ResetPassword
{
    public record ResetPasswordCommand(ResetPasswordDTO Request) : IRequest<OperationResult>;
}
