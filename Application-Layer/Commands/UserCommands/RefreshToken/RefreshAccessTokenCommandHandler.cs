using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, RefreshTokenResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;

        public RefreshAccessTokenCommandHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<RefreshTokenResult> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return new RefreshTokenResult(false, "Refresh token is required.");
                }

                // Rotate the refresh token (validates old token, revokes it, creates new one)
                var rotationResult = await _refreshTokenService.RotateRefreshTokenAsync(
                    request.RefreshToken,
                    request.IpAddress,
                    request.UserAgent);

                if (rotationResult == null)
                {
                    // Validation failed - token may be invalid, expired, or reused
                    return new RefreshTokenResult(false, "Invalid or expired refresh token.");
                }

                var (newRawRefreshToken, newTokenEntity) = rotationResult.Value;

                // Get the user to generate a new access token
                var user = await _userRepository.FindByIdAsync(newTokenEntity.UserId);
                if (user == null)
                {
                    return new RefreshTokenResult(false, "User not found.");
                }

                var roles = await _userRepository.GetRolesAsync(user);

                // Generate new access token
                var newAccessToken = await _jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles);

                return new RefreshTokenResult(
                    successful: true,
                    error: null,
                    accessToken: newAccessToken,
                    refreshToken: newRawRefreshToken);
            }
            catch (Exception ex)
            {
                return new RefreshTokenResult(false, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
