using Application_Layer.DTOs;
using Domain_Layer.Common;
using MediatR;

namespace Application_Layer.Commands.UserCommands.GoogleLogin
{
    /// <summary>
    /// Loggar in med ett Google ID-token (GIS-flödet). Loggar in en redan kopplad
    /// användare, länkar Google-identiteten till ett befintligt konto med samma
    /// verifierade e-post, eller provisionerar ett nytt kundkonto.
    /// </summary>
    public sealed record GoogleLoginCommand(string IdToken, string? IpAddress, string? UserAgent)
        : IRequest<OperationResult<AuthTokenPairDTO>>;
}
