namespace Application_Layer.DTOs;

public sealed record AuthTokenPairDTO(string AccessToken, string? RefreshToken);
