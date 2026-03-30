using Application_Layer.Commands.UserCommands.Update;
using Application_Layer.Commands.UserCommands.UpdatePassword;
using Application_Layer.DTOs;
using Application_Layer.Queries.UserQueries.GetUserName;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API_Layer.Controllers;

[Route("api/me")]
[ApiController]
[Authorize]
public class MeController : BaseApiController
{
    private readonly IMediator _mediator;

    public MeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileDTO updateUserProfileDTO)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized("User is not logged in.");

        var result = await _mediator.Send(new UpdateUserProfileCommand(userId, updateUserProfileDTO));
        return HandleResult(result);
    }

    [HttpPut("password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDTO updatePasswordDTO)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var result = await _mediator.Send(new UpdatePasswordCommand(userId, updatePasswordDTO));
        return HandleResult(result, () => Ok("Password updated successfully."));
    }

    [HttpGet]
    public IActionResult GetUser()
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var email = GetClaimValue(ClaimTypes.Email);
        var role = GetClaimValue(ClaimTypes.Role);

        return Ok(new { userId, email, role });
    }

    [HttpGet("name")]
    public async Task<IActionResult> GetUserName()
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var userNameDto = await _mediator.Send(new GetUserNameQuery(userId));
        return userNameDto is null ? NotFound($"User with ID {userId} was not found.") : Ok(userNameDto);
    }
}
