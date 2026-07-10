using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Avatar.DeleteMyAvatar
{
    public sealed record DeleteMyAvatarCommand(string UserId) : IRequest<OperationResult>;
}
