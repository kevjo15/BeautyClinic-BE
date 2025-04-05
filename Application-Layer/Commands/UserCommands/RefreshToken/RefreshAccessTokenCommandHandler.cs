using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application_Layer.Interfaces;
using Application_Layer.Jwt;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace Application_Layer.Commands.UserCommands.RefreshToken
{
    public class RefreshAccessTokenCommandHandler : IRequestHandler<RefreshAccessTokenCommand, RefreshTokenResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public RefreshAccessTokenCommandHandler(
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator,
            TokenValidationParameters tokenValidationParameters)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public async Task<RefreshTokenResult> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validera token-signaturen men ignorera livstidskontrollen så att vi kan extrahera claims även om tokenet är utgånget.
                var principal = tokenHandler.ValidateToken(request.AccessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = _tokenValidationParameters.ValidateIssuerSigningKey,
                    IssuerSigningKey = _tokenValidationParameters.IssuerSigningKey,
                    ValidateIssuer = _tokenValidationParameters.ValidateIssuer,
                    ValidIssuer = _tokenValidationParameters.ValidIssuer,
                    ValidateAudience = _tokenValidationParameters.ValidateAudience,
                    ValidAudience = _tokenValidationParameters.ValidAudience,
                    ValidateLifetime = false // Ignorera utgången
                }, out SecurityToken validatedToken);

                // Validera att tokenet är av rätt typ och med rätt algoritm (t.ex. HS256)
                if (!(validatedToken is JwtSecurityToken jwtToken) ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new RefreshTokenResult(false, "Felaktig token.");
                }

                // Hämta refresh-periodens utgångstid från token (claim-namn kan t.ex. vara "RefreshTokenExpiryTime")
                var refreshExpiryClaim = principal.FindFirst("RefreshTokenExpiryTime");
                if (refreshExpiryClaim == null)
                {
                    return new RefreshTokenResult(false, "Saknar refresh expiry claim.");
                }

                if (!DateTime.TryParse(refreshExpiryClaim.Value, out DateTime refreshExpiryTime))
                {
                    return new RefreshTokenResult(false, "Ogiltigt värde för refresh expiry.");
                }

                // Kontrollera att vi fortfarande är inom den tillåtna refresh-perioden
                if (DateTime.UtcNow > refreshExpiryTime)
                {
                    return new RefreshTokenResult(false, "Refreshperioden har gått ut.");
                }

                // Hämta användarens ID från token (förutsatt att ClaimTypes.NameIdentifier finns med)
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return new RefreshTokenResult(false, "Användar-ID saknas i token.");
                }
                var userId = userIdClaim.Value;

                // Hämta användaren från databasen
                var user = await _userRepository.FindByIdAsync(userId);
                if (user == null)
                {
                    return new RefreshTokenResult(false, "Användaren hittades inte.");
                }

                // (Valfritt) Jämför gärna det lagrade värdet för refresh-perioden (i DB) med det som kom från token
                // if (user.RefreshTokenExpiryTime != refreshExpiryTime)
                // {
                //     return new RefreshTokenResult(false, "Mismatch i refresh token information.");
                // }

                // Generera ett nytt accessToken
                var newAccessToken = await _jwtTokenGenerator.GenerateToken(user);
                return new RefreshTokenResult(true, null, newAccessToken);
            }
            catch (SecurityTokenExpiredException ex)
            {
                return new RefreshTokenResult(false, "Token has expired: " + ex.Message);
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
               return new RefreshTokenResult(false, "Invalid token signature: " + ex.Message);
           }
           catch (SecurityTokenValidationException ex)
           {
               return new RefreshTokenResult(false, "Token validation failed: " + ex.Message);
           }
           catch (Exception ex)
           {
               return new RefreshTokenResult(false, "An unexpected error occurred: " + ex.Message);
           }
        }
    }
}
