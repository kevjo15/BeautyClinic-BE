using Application_Layer.DTOs;

namespace Application_Layer.Interfaces
{
    /// <summary>
    /// Validerar ett Google ID-token (signatur, audience, giltighetstid)
    /// kryptografiskt mot Googles publika nycklar.
    /// </summary>
    public interface IGoogleIdTokenValidator
    {
        /// <summary>Returnerar användarens claims, eller null om tokenet är ogiltigt.</summary>
        Task<GoogleUserInfo?> ValidateAsync(string idToken, CancellationToken ct);
    }
}
