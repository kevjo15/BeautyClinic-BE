using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.Commands.UserCommands.RefreshToken;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.Commands.UserCommands.RevokeRefreshToken;
using Application_Layer.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private const string RefreshTokenCookieName = "refreshToken";

    public AuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDTO)
    {
        var result = await _mediator.Send(new RegisterUserCommand(registerUserDTO));
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDTO loginUserDTO)
    {
        var command = new LoginCommand(loginUserDTO, GetIpAddress(), GetUserAgent());
        var result = await _mediator.Send(command);

        if (!result.Successful)
        {
            return BadRequest(result.Error);
        }

        if (!string.IsNullOrEmpty(result.Data?.RefreshToken))
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
        }

        return Ok(new { accessToken = result.Data!.AccessToken });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("No refresh token provided.");
        }

        var command = new RefreshAccessTokenCommand(refreshToken, GetIpAddress(), GetUserAgent());
        var result = await _mediator.Send(command);

        if (!result.Successful)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(result.Error);
        }

        if (!string.IsNullOrEmpty(result.Data?.RefreshToken))
        {
            SetRefreshTokenCookie(result.Data.RefreshToken);
        }

        return Ok(new { accessToken = result.Data!.AccessToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> RevokeRefreshToken()
    {
        var command = new RevokeRefreshTokenCommand(
            refreshToken: Request.Cookies[RefreshTokenCookieName],
            userId: CurrentUserId,
            ipAddress: GetIpAddress(),
            reason: "User logout");

        var result = await _mediator.Send(command);
        ClearRefreshTokenCookie();

        return HandleResult(result, () => Ok(new { message = "Logged out successfully." }));
    }

    [Authorize]
    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeAllTokens()
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var command = new RevokeRefreshTokenCommand(
            refreshToken: null,
            userId: userId,
            ipAddress: GetIpAddress(),
            reason: "User revoked all sessions");

        var result = await _mediator.Send(command);
        ClearRefreshTokenCookie();

        return HandleResult(result, () => Ok(new { message = "All sessions have been terminated." }));
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var refreshTokenDays = _configuration.GetValue<int>("JwtSettings:RefreshTokenExpiryDays", 7);
        var isProduction = !string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(refreshTokenDays),
            Path = "/api/auth"
        };

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    private void ClearRefreshTokenCookie()
    {
        var isProduction = !string.Equals(
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            "Development",
            StringComparison.OrdinalIgnoreCase);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = isProduction,
            SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/api/auth"
        };

        Response.Cookies.Append(RefreshTokenCookieName, "", cookieOptions);
    }

    private string? GetIpAddress()
    {
        var request = HttpContext?.Request;
        if (request == null)
        {
            return null;
        }

        if (request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return request.Headers["X-Forwarded-For"]
                .FirstOrDefault()?
                .Split(',')
                .FirstOrDefault()?
                .Trim();
        }

        return HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private string? GetUserAgent()
    {
        return HttpContext?.Request?.Headers["User-Agent"].FirstOrDefault();
    }
}
