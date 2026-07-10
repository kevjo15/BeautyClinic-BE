using Application_Layer.DTOs;
using Application_Layer.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure_Layer.Identity
{
    /// <summary>
    /// Validerar Google ID-tokens via Google.Apis.Auth (hämtar och cachar Googles
    /// publika nycklar, kontrollerar signatur, utgångstid och audience =
    /// GoogleAuth:ClientId). Returnerar null för allt som inte är ett giltigt
    /// token utfärdat för vår klient.
    /// </summary>
    public class GoogleIdTokenValidator : IGoogleIdTokenValidator
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GoogleIdTokenValidator> _logger;

        public GoogleIdTokenValidator(IConfiguration configuration, ILogger<GoogleIdTokenValidator> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct)
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
            {
                _logger.LogWarning("Google login attempted but GoogleAuth:ClientId is not configured");
                return null;
            }

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });

                return new GoogleUserInfo(
                    Subject: payload.Subject,
                    Email: payload.Email,
                    EmailVerified: payload.EmailVerified,
                    FirstName: payload.GivenName,
                    LastName: payload.FamilyName);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogInformation("Rejected invalid Google ID token: {Reason}", ex.Message);
                return null;
            }
        }
    }
}
