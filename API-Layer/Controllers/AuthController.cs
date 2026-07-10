using Application_Layer.Commands.UserCommands.ConfirmEmail;
using Application_Layer.Commands.UserCommands.ForgotPassword;
using Application_Layer.Commands.UserCommands.GoogleLogin;
using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.Commands.UserCommands.ResendEmailConfirmation;
using Application_Layer.Commands.UserCommands.RefreshToken;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.Commands.UserCommands.ResetPassword;
using Application_Layer.Commands.UserCommands.RevokeRefreshToken;
using Application_Layer.DTOs;
using API_Layer.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
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

    /// <summary>
    /// Logga in med Google (GIS ID-token). Loggar in kopplade konton, länkar
    /// Google till befintligt konto med samma verifierade e-post, eller
    /// registrerar ett nytt kundkonto. Svarar som vanlig login:
    /// accessToken i body + refresh-token som HttpOnly-cookie.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO googleLoginDTO)
    {
        var command = new GoogleLoginCommand(googleLoginDTO.IdToken, GetIpAddress(), GetUserAgent());
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
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDTO confirmEmailDTO)
    {
        var result = await _mediator.Send(new ConfirmEmailCommand(confirmEmailDTO));
        return HandleResult(result, () => Ok(new { message = "E-postadressen har bekräftats." }));
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.PasswordResetPolicy)]
    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationDTO resendConfirmationDTO)
    {
        await _mediator.Send(new ResendEmailConfirmationCommand(resendConfirmationDTO));

        // Alltid samma svar — avslöjar inte om adressen finns.
        return Ok(new { message = "Om adressen finns hos oss har vi skickat ett nytt bekräftelsemejl." });
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.PasswordResetPolicy)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
    {
        await _mediator.Send(new ForgotPasswordCommand(forgotPasswordDTO));

        // Alltid samma svar — avslöjar inte om adressen finns.
        return Ok(new { message = "Om adressen finns hos oss har vi skickat en återställningslänk." });
    }

    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.PasswordResetPolicy)]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetPasswordDTO)
    {
        var result = await _mediator.Send(new ResetPasswordCommand(resetPasswordDTO));
        return HandleResult(result, () => Ok(new { message = "Lösenordet har uppdaterats." }));
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
