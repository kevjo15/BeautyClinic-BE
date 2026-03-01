using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.Commands.UserCommands.RefreshToken;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.Commands.UserCommands.RevokeRefreshToken;
using Application_Layer.Commands.UserCommands.Update;
using Application_Layer.Commands.UserCommands.UpdatePassword;
using Application_Layer.DTO_s;
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
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
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
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDTO)
        {
            var result = await _mediator.Send(new RegisterUserCommand(registerUserDTO));

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.CreatedUser);
        }

        [AllowAnonymous]
        [HttpPost("login")]
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
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            // Return only the access token in the response body
            return Ok(new { accessToken = result.Token });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _mediator.Send(new GetUserByIdQuery(id));

            if (user != null)
            {
                return Ok(user);
            }

            return BadRequest($"User with ID {id} was not found.");
        }

        [Authorize]
        [HttpPost("me/update-profile")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDTO updateUserProfileDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized("User is not logged in.");
            }
            var result = await _mediator.Send(new UpdateUserProfileCommand(userId, updateUserProfileDTO));
            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.UpdatedUserProfile);
        }

        [Authorize]
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("This is an Admin-only area.");
        }

        [AllowAnonymous]
        [HttpPost("refreshAccessToken")]
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
            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                SetRefreshTokenCookie(result.RefreshToken);
            }

            return Ok(new { accessToken = result.AccessToken });
        }

        [Authorize]
        [HttpPost("revokeRefreshToken")]
        public async Task<IActionResult> RevokeRefreshToken()
        {
            var refreshToken = Request.Cookies[RefreshTokenCookieName];
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = GetIpAddress();

            var command = new RevokeRefreshTokenCommand(
                refreshToken: refreshToken,
                userId: userId,
                ipAddress: ipAddress,
                reason: "User logout");

            var result = await _mediator.Send(command);

            // Always clear the cookie on logout
            ClearRefreshTokenCookie();

            if (!result)
            {
                return BadRequest("Failed to revoke refresh token.");
            }

            return Ok(new { message = "Logged out successfully." });
        }

        [Authorize]
        [HttpPost("revokeAllTokens")]
        public async Task<IActionResult> RevokeAllTokens()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var ipAddress = GetIpAddress();

            var command = new RevokeRefreshTokenCommand(
                refreshToken: null,
                userId: userId,
                ipAddress: ipAddress,
                reason: "User revoked all sessions");

            var result = await _mediator.Send(command);

            // Clear the cookie
            ClearRefreshTokenCookie();

            if (!result)
            {
                return BadRequest("Failed to revoke tokens.");
            }

            return Ok(new { message = "All sessions have been terminated." });
        }

        [Authorize]
        [HttpPost("me/update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO updatePasswordDTO)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var command = new UpdatePasswordCommand(userId, updatePasswordDTO);
            var result = await _mediator.Send(command);

            if (!result) return BadRequest("Failed to update password.");

            return Ok("Password updated successfully.");
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null) return Unauthorized();

            return Ok(new { userId, email, role });
        }

        [Authorize]
        [HttpGet("me/name")]
        public async Task<IActionResult> GetUserName()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var userNameDto = await _mediator.Send(new GetUserNameQuery(userId));
            if (userNameDto == null) return NotFound($"User with ID {userId} was not found.");

            return Ok(userNameDto);
        }

        [Authorize(Roles = "Admin,Employee")]
        [HttpGet("employees")]
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
                Path = "/api/User" // Restrict cookie to auth endpoints
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
                Path = "/api/User"
            };

            Response.Cookies.Append(RefreshTokenCookieName, "", cookieOptions);
        }

        private string? GetIpAddress()
        {
            // Check for forwarded IP first (if behind proxy/load balancer)
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                return Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            }

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }

        private string? GetUserAgent()
        {
            return Request.Headers["User-Agent"].FirstOrDefault();
        }

        #endregion
    }
}
