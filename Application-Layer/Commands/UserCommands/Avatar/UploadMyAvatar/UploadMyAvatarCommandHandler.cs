using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Avatar.UploadMyAvatar
{
    public class UploadMyAvatarCommandHandler
        : IRequestHandler<UploadMyAvatarCommand, OperationResult<UploadMyAvatarResult>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;

        public UploadMyAvatarCommandHandler(IUserRepository userRepository, IFileService fileService)
        {
            _userRepository = userRepository;
            _fileService = fileService;
        }

        public async Task<OperationResult<UploadMyAvatarResult>> Handle(
            UploadMyAvatarCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return OperationResult<UploadMyAvatarResult>.Failure(
                    "User was not found!", OperationFailureType.NotFound);
            }

            var (blobPath, sasUrl) = await _fileService.UploadUserAvatarAsync(
                request.UserId, request.File, cancellationToken);

            user.AvatarUrl = blobPath;
            var updateResult = await _userRepository.UpdateUserAsync(user);
            if (!updateResult.Successful)
            {
                return OperationResult<UploadMyAvatarResult>.Failure(
                    updateResult.Error ?? "Failed to save avatar.");
            }

            return OperationResult<UploadMyAvatarResult>.Success(new UploadMyAvatarResult(sasUrl));
        }
    }
}
