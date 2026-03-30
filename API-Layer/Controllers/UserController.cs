using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.Commands.UserCommands.RefreshToken;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.Commands.UserCommands.RevokeRefreshToken;
using Application_Layer.Commands.UserCommands.Update;
using Application_Layer.Commands.UserCommands.UpdatePassword;
using Application_Layer.DTOs;
using Application_Layer.Queries.UserQueries.GetUserById;
using Application_Layer.Queries.UserQueries.GetUserName;
using Application_Layer.Queries.UserQueries.GetEmployees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_Layer.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private const string RefreshTokenCookieName = "refreshToken";

        public UserController(IMediator mediator, IConfiguration configuration)
        {
            _mediator = mediator;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("auth/register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDTO)
        {
            var result = await _mediator.Send(new RegisterUserCommand(registerUserDTO));
            return HandleResult(result);
        }

        [AllowAnonymous]
        [HttpPost("auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginUserDTO)
        {
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var command = new LoginCommand(loginUserDTO, ipAddress, userAgent);
            var result = await _mediator.Send(command);

            if (!result.Successful)
            {
                return BadRequest(result.Error);
            }

            // Set refresh token in HttpOnly cookie
            if (!string.IsNullOrEmpty(result.Data?.RefreshToken))
            {
                SetRefreshTokenCookie(result.Data.RefreshToken);
            }

            // Return only the access token in the response body
            return Ok(new { accessToken = result.Data!.AccessToken });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _mediator.Send(new GetUserByIdQuery(id));
            return user is null ? NotFound($"User with ID {id} was not found.") : Ok(user);
        }

        [Authorize]
        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDTO updateUserProfileDTO)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized("User is not logged in.");

            var result = await _mediator.Send(new UpdateUserProfileCommand(userId, updateUserProfileDTO));
            return HandleResult(result);
        }

        [Authorize]
        [Authorize(Roles = "Admin")]
        [HttpGet("users/admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("This is an Admin-only area.");
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            // Read refresh token from HttpOnly cookie
            var refreshToken = Request.Cookies[RefreshTokenCookieName];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized("No refresh token provided.");
            }

            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();

            var command = new RefreshAccessTokenCommand(refreshToken, ipAddress, userAgent);
            var result = await _mediator.Send(command);

            if (!result.Successful)
            {
                // Clear the invalid cookie
                ClearRefreshTokenCookie();
                return Unauthorized(result.Error);
            }

            // Set the new refresh token in HttpOnly cookie (rotation)
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
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            var userId = CurrentUserId;
            var ipAddress = GetIpAddress();

            var command = new RevokeRefreshTokenCommand(
                refreshToken: refreshToken,
                userId: userId,
                ipAddress: ipAddress,
                reason: "User logout");

            var result = await _mediator.Send(command);

            // Always clear the cookie on logout
            ClearRefreshTokenCookie();

            return HandleResult(result, () => Ok(new { message = "Logged out successfully." }));
        }

        [Authorize]
        [HttpDelete("sessions")]
        public async Task<IActionResult> RevokeAllTokens()
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var ipAddress = GetIpAddress();

            var command = new RevokeRefreshTokenCommand(
                refreshToken: null,
                userId: userId,
                ipAddress: ipAddress,
                reason: "User revoked all sessions");

            var result = await _mediator.Send(command);

            // Clear the cookie
            ClearRefreshTokenCookie();

            return HandleResult(result, () => Ok(new { message = "All sessions have been terminated." }));
        }

        [Authorize]
        [HttpPut("me/password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO updatePasswordDTO)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var command = new UpdatePasswordCommand(userId, updatePasswordDTO);
            var result = await _mediator.Send(command);
            return HandleResult(result, () => Ok("Password updated successfully."));
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetUser()
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var email = GetClaimValue(ClaimTypes.Email);
            var role = GetClaimValue(ClaimTypes.Role);

            return Ok(new { userId, email, role });
        }

        [Authorize]
        [HttpGet("me/name")]
        public async Task<IActionResult> GetUserName()
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var userNameDto = await _mediator.Send(new GetUserNameQuery(userId));
            if (userNameDto == null) return NotFound($"User with ID {userId} was not found.");

            return Ok(userNameDto);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("users/employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _mediator.Send(new GetEmployeesQuery());
            return Ok(employees);
        }

        #region Private Helper Methods

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
                Secure = isProduction, // Only require HTTPS in production
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(refreshTokenDays),
                Path = "/api/auth" // Restrict cookie to auth endpoints
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

        #endregion
    }
}
