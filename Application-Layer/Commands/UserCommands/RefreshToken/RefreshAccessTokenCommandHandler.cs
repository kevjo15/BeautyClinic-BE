using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, OperationResult<AuthTokenPairDTO>>
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

        public async Task<OperationResult<AuthTokenPairDTO>> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return OperationResult<AuthTokenPairDTO>.Failure("Refresh token is required.");
                }

                // Rotate the refresh token (validates old token, revokes it, creates new one)
                var rotationResult = await _refreshTokenService.RotateRefreshTokenAsync(
                    request.RefreshToken,
                    request.IpAddress,
                    request.UserAgent);

                if (!rotationResult.Successful || rotationResult.Data == null)
                {
                    return OperationResult<AuthTokenPairDTO>.Failure(
                        rotationResult.Error ?? "Invalid or expired refresh token.");
                }

                var newRawRefreshToken = rotationResult.Data.RawToken;
                var newTokenEntity = rotationResult.Data.TokenEntity;

                // Get the user to generate a new access token
                var user = await _userRepository.FindByIdAsync(newTokenEntity.UserId);
                if (user == null)
                {
                    return OperationResult<AuthTokenPairDTO>.Failure("User not found.");
                }

                var roles = await _userRepository.GetRolesAsync(user);

                // Generate new access token
                var newAccessToken = await _jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles);

                return OperationResult<AuthTokenPairDTO>.Success(
                    new AuthTokenPairDTO(newAccessToken, newRawRefreshToken));
            }
            catch (Exception ex)
            {
                return OperationResult<AuthTokenPairDTO>.Failure($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
