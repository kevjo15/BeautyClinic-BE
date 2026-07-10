using Application_Layer.Interfaces;
using Domain_Layer.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.UserCommands.DeleteMyAccount
{
    public class DeleteMyAccountCommandHandler : IRequestHandler<DeleteMyAccountCommand, OperationResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IFileService _fileService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<DeleteMyAccountCommandHandler> _logger;

        public DeleteMyAccountCommandHandler(
            IUserRepository userRepository,
            IFileService fileService,
            IRefreshTokenService refreshTokenService,
            ILogger<DeleteMyAccountCommandHandler> logger)
        {
            _userRepository = userRepository;
            _fileService = fileService;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<OperationResult> Handle(DeleteMyAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return OperationResult.Failure("User not found.", OperationFailureType.NotFound);
            }

            // Rensa profilbildens blob — misslyckande får inte fälla raderingen.
            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                try
                {
                    await _fileService.DeleteAsync(user.AvatarUrl, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Could not delete avatar blob {BlobPath} during account deletion for {UserId}",
                        user.AvatarUrl, request.UserId);
                }
            }

            // Döda alla sessioner (refresh-tokens).
            await _refreshTokenService.RevokeAllUserTokensAsync(
                request.UserId, ipAddress: null, reason: "Account deleted");

            // Anonymisera identiteten (behåller raden för refererande bokningar).
            return await _userRepository.AnonymizeAndDeactivateAsync(request.UserId);
        }
    }
}
