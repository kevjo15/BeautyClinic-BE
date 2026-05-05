using Domain_Layer.Models;

namespace Application_Layer.DTOs;

public sealed record RefreshTokenRotationResultDTO(string RawToken, UserRefreshToken TokenEntity);
