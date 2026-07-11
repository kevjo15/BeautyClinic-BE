using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Avatar.UploadMyAvatar
{
    public sealed record UploadMyAvatarCommand(string UserId, FileUploadRequest File)
        : IRequest<OperationResult<UploadMyAvatarResult>>;

    public sealed record UploadMyAvatarResult(string AvatarUrl);
}
