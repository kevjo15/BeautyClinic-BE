using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, OperationResult<AuthTokenPairDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<OperationResult<AuthTokenPairDTO>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _userRepository.FindByEmailAsync(request.LoginUserDTO.Email);
                var passwordValid = existingUser != null &&
                    await _userRepository.CheckPasswordAsync(existingUser, request.LoginUserDTO.Password);
                if (existingUser == null || !passwordValid)
                {
                    return OperationResult<AuthTokenPairDTO>.Failure("Felaktigt email eller lösenord.");
                }

                // Generate refresh token using the new service (hashed and stored in DB)
                var (rawRefreshToken, _) = await _refreshTokenService.GenerateRefreshTokenAsync(
                    existingUser.Id,
                    request.IpAddress,
                    request.UserAgent);

                var roles = await _userRepository.GetRolesAsync(existingUser);

                // Generate access token
                var accessToken = await _jwtTokenGenerator.GenerateToken(existingUser.Id, existingUser.Email, roles);

                return OperationResult<AuthTokenPairDTO>.Success(new AuthTokenPairDTO(accessToken, rawRefreshToken));
            }
            catch (Exception ex)
            {
                return OperationResult<AuthTokenPairDTO>.Failure($"An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
