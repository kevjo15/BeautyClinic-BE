using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.UserCommands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, OperationResult<AuthTokenPairDTO>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<LoginCommandHandler> _logger;

        public LoginCommandHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService,
            ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
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

                // Kollas först EFTER lyckad lösenordskontroll — läcker inget till utomstående.
                if (existingUser.IsDeleted)
                {
                    return OperationResult<AuthTokenPairDTO>.Failure("Kontot är borttaget.");
                }

                if (!existingUser.EmailConfirmed)
                {
                    return OperationResult<AuthTokenPairDTO>.Failure(
                        "Din e-postadress är inte bekräftad. Klicka på länken i bekräftelsemejlet vi skickade när du registrerade dig.");
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
                // Logga internt; exponera aldrig råa exception-detaljer till klienten.
                _logger.LogError(ex, "Unhandled exception during login");
                return OperationResult<AuthTokenPairDTO>.Failure("An unexpected error occurred.");
            }
        }
    }
}
