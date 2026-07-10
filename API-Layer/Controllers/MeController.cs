using Application_Layer.Commands.UserCommands.Avatar.DeleteMyAvatar;
using Application_Layer.Commands.UserCommands.Avatar.UploadMyAvatar;
using Application_Layer.Commands.UserCommands.DeleteMyAccount;
using Application_Layer.Commands.UserCommands.Update;
using Application_Layer.Commands.UserCommands.UpdatePassword;
using Application_Layer.DTOs;
using Application_Layer.Queries.UserQueries.GetMyProfile;
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
    public async Task<IActionResult> GetUser()
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var email = GetClaimValue(ClaimTypes.Email);
        var role = GetClaimValue(ClaimTypes.Role);

        var profile = await _mediator.Send(new GetMyProfileQuery(userId));
        if (profile is null)
        {
            // Token is valid but the user row is gone — fall back to claims only.
            return Ok(new UserProfileDTO { UserId = userId, Email = email, Role = role });
        }

        profile.Role = role;
        return Ok(profile);
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();
        if (file is null) return BadRequest("file is required");

        await using var stream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, ct);

        var upload = new FileUploadRequest(
            file.FileName,
            file.ContentType ?? "application/octet-stream",
            memoryStream.ToArray());

        var result = await _mediator.Send(new UploadMyAvatarCommand(userId, upload), ct);
        return HandleResult(result);
    }

    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar(CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var result = await _mediator.Send(new DeleteMyAvatarCommand(userId), ct);
        return HandleResult(result, () => Ok(new { message = "Profilbilden har tagits bort." }));
    }

    [HttpGet("name")]
    public async Task<IActionResult> GetUserName()
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var userNameDto = await _mediator.Send(new GetUserNameQuery(userId));
        return userNameDto is null ? NotFound($"User with ID {userId} was not found.") : Ok(userNameDto);
    }

    /// <summary>GDPR-radering av det egna kontot (anonymiserar; bokningar behålls).</summary>
    [HttpDelete]
    public async Task<IActionResult> DeleteAccount(CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

        var result = await _mediator.Send(new DeleteMyAccountCommand(userId), ct);
        return HandleResult(result, () =>
        {
            // Rensa den nu inaktuella refresh-cookien. Attributen måste spegla dem
            // den sattes med (path /api/auth, Secure/SameSite i prod) — annars
            // matchar inte browsern cookien och den blir kvar (om än redan återkallad).
            var isProduction = !string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development", StringComparison.OrdinalIgnoreCase);

            Response.Cookies.Delete("refreshToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = isProduction,
                SameSite = isProduction ? SameSiteMode.None : SameSiteMode.Lax,
                Path = "/api/auth",
            });
            return Ok(new { message = "Ditt konto har raderats." });
        });
    }
}
