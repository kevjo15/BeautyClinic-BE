using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using Domain_Layer.Common;
using Domain_Layer.Models;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application_Layer.Commands.UserCommands.GoogleLogin
{
    public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, OperationResult<AuthTokenPairDTO>>
    {
        private const string GoogleProvider = "Google";

        private readonly IGoogleIdTokenValidator _googleValidator;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ILogger<GoogleLoginCommandHandler> _logger;

        public GoogleLoginCommandHandler(
            IGoogleIdTokenValidator googleValidator,
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenService refreshTokenService,
            ILogger<GoogleLoginCommandHandler> logger)
        {
            _googleValidator = googleValidator;
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenService = refreshTokenService;
            _logger = logger;
        }

        public async Task<OperationResult<AuthTokenPairDTO>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            var googleUser = await _googleValidator.ValidateAsync(request.IdToken, cancellationToken);
            if (googleUser == null)
            {
                return OperationResult<AuthTokenPairDTO>.Failure("Google-inloggningen kunde inte verifieras.");
            }

            // Endast verifierade adresser får matchas/registreras — annars kan en
            // angripare ta över ett befintligt konto via en overifierad Google-adress.
            if (!googleUser.EmailVerified)
            {
                return OperationResult<AuthTokenPairDTO>.Failure(
                    "Din Google-adress är inte verifierad hos Google.");
            }

            var user = await ResolveUserAsync(googleUser);
            if (user == null)
            {
                return OperationResult<AuthTokenPairDTO>.Failure("Google-inloggningen misslyckades.");
            }

            // Samma invariant som lösenordslogin (kontrolleras efter identifiering).
            if (user.IsDeleted)
            {
                return OperationResult<AuthTokenPairDTO>.Failure("Kontot är borttaget.");
            }

            var (rawRefreshToken, _) = await _refreshTokenService.GenerateRefreshTokenAsync(
                user.Id, request.IpAddress, request.UserAgent);
            var roles = await _userRepository.GetRolesAsync(user);
            var accessToken = await _jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles);

            return OperationResult<AuthTokenPairDTO>.Success(new AuthTokenPairDTO(accessToken, rawRefreshToken));
        }

        /// <summary>Kopplad användare → befintligt konto via e-post (länkas) → nyregistrering.</summary>
        private async Task<UserModel?> ResolveUserAsync(GoogleUserInfo googleUser)
        {
            var linked = await _userRepository.FindByExternalLoginAsync(GoogleProvider, googleUser.Subject);
            if (linked != null)
            {
                return linked;
            }

            var byEmail = await _userRepository.FindByEmailAsync(googleUser.Email);
            if (byEmail != null)
            {
                var linkResult = await _userRepository.AddExternalLoginAsync(
                    byEmail.Id, GoogleProvider, googleUser.Subject);
                if (!linkResult.Successful)
                {
                    _logger.LogWarning("Failed to link Google login for user {UserId}: {Error}",
                        byEmail.Id, linkResult.Error);
                    return null;
                }

                _logger.LogInformation("Linked Google login to existing user {UserId}", byEmail.Id);
                return byEmail;
            }

            var newUser = new UserModel
            {
                UserName = googleUser.Email,
                Email = googleUser.Email,
                FirstName = googleUser.FirstName,
                LastName = googleUser.LastName,
            };

            var registerResult = await _userRepository.RegisterExternalUserAsync(
                newUser, GoogleProvider, googleUser.Subject);
            if (!registerResult.Successful)
            {
                _logger.LogWarning("Failed to provision Google user: {Error}", registerResult.Error);
                return null;
            }

            _logger.LogInformation("Provisioned new user {UserId} via Google sign-in", newUser.Id);
            return newUser;
        }
    }
}
