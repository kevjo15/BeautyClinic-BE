using System.Collections.Generic;

namespace Application_Layer.Jwt
{
    public interface IJwtTokenGenerator
    {
        Task<string> GenerateToken(string userId, string? email, IEnumerable<string> roles);
    }
}
