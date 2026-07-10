using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.UserCommands.Avatar.DeleteMyAvatar
{
    public class DeleteMyAvatarCommandHandler : IRequestHandler<DeleteMyAvatarCommand, OperationResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<DeleteMyAvatarCommandHandler> _logger;

        public DeleteMyAvatarCommandHandler(
            IUserRepository userRepository,
            IFileService fileService,
            ILogger<DeleteMyAvatarCommandHandler> logger)
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<OperationResult> Handle(DeleteMyAvatarCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return OperationResult.Failure("User was not found!", OperationFailureType.NotFound);
            }

            if (string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                return OperationResult.Success();
            }

            try
            {
                await _fileService.DeleteAsync(user.AvatarUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                // En kvarglömd blob är ofarlig — databasfältet är sanningen.
                _logger.LogWarning(ex, "Could not delete avatar blob {BlobPath}", user.AvatarUrl);
            }

            user.AvatarUrl = null;
            return await _userRepository.UpdateUserAsync(user);
        }
    }
}
