using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using MediatR;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
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

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingUser = await _userRepository.FindByEmailAsync(request.LoginUserDTO.Email);
                if (existingUser == null)
                {
                    return CreateLoginResult(false, "Användaren existerar inte.");
                }

                var passwordValid = await _userRepository.CheckPasswordAsync(existingUser, request.LoginUserDTO.Password);
                if (!passwordValid)
                {
                    return CreateLoginResult(false, "Felaktigt lösenord.");
                }

                // Generate refresh token using the new service (hashed and stored in DB)
                var (rawRefreshToken, _) = await _refreshTokenService.GenerateRefreshTokenAsync(
                    existingUser.Id,
                    request.IpAddress,
                    request.UserAgent);

                var roles = await _userRepository.GetRolesAsync(existingUser);

                // Generate access token
                var accessToken = await _jwtTokenGenerator.GenerateToken(existingUser.Id, existingUser.Email, roles);

                return CreateLoginResult(true, null, accessToken, rawRefreshToken);
            }
            catch (Exception ex)
            {
                return CreateLoginResult(false, $"An unexpected error occurred: {ex.Message}");
            }
        }

        private LoginResult CreateLoginResult(bool successful, string? error, string? token = null, string? refreshToken = null)
        {
            return new LoginResult { Successful = successful, Error = error, Token = token, RefreshToken = refreshToken };
        }
    }
}
