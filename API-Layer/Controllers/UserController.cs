using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.Commands.UserCommands.RefreshToken;
using Application_Layer.Commands.UserCommands.RegisterUser;
using Application_Layer.Commands.UserCommands.RevokeRefreshToken;
using Application_Layer.Commands.UserCommands.Update;
using Application_Layer.Commands.UserCommands.UpdatePassword;
using Application_Layer.DTO_s;
using Application_Layer.Queries.UserQueries.GetUserById;
using Domain_Layer.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace API_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDTO loginUserDTO)
        {
            var command = new LoginCommand(loginUserDTO);
            var result = await _mediator.Send(command);

            if (!result.Successful)
            {
                return BadRequest(result.Error);
            }

            return Ok(new { token = result.Token });
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

        [HttpPost("refreshAccessToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshAccessTokenRequestDTO request)
        {
            var command = new RefreshAccessTokenCommand(request.AccessToken);
            var result = await _mediator.Send(command);

            if (!result.Successful)
            {
                return Unauthorized(result.Error);
            }

            return Ok(new { AccessToken = result.AccessToken });
        }

        [HttpPost("revokeRefreshtoken")]
        public async Task<IActionResult> RevokeRefreshToken()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var command = new RevokeRefreshTokenCommand(userId);
            var result = await _mediator.Send(command);

            if (!result) return BadRequest("Failed to revoke refresh token.");

            return Ok("Refresh token revoked successfully.");
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

        [HttpGet("me")]
        public IActionResult GetUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null) return Unauthorized();

            return Ok(new { userId, email, role });
        }


        //[HttpGet("test-auth")]
        //public IActionResult TestAuth()
        //{
        //    // Kontrollera om användaren är autentiserad
        //    var isAuthenticated = User.Identity.IsAuthenticated;

        //    // Hämta användarens claims
        //    var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        //    // Returnera en respons som visar om användaren är autentiserad samt alla claims
        //    return Ok(new
        //    {
        //        IsAuthenticated = isAuthenticated,
        //        Claims = claims
        //    });
        //}
    }
}
