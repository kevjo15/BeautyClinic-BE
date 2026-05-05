using Domain_Layer.Common;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace API_Layer.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected string? GetClaimValue(string claimType) => User.FindFirstValue(claimType);

    protected bool TryGetCurrentUserId([NotNullWhen(true)] out string? userId)
    {
        userId = CurrentUserId;
        return !string.IsNullOrWhiteSpace(userId);
    }

    protected ActionResult HandleResult(
        OperationResult result,
        Func<ActionResult>? onSuccess = null,
        Func<string?, ActionResult>? onFailure = null)
    {
        if (!result.Successful)
        {
            return onFailure?.Invoke(result.Error) ?? MapFailure(result);
        }

        return onSuccess?.Invoke() ?? NoContent();
    }

    protected ActionResult HandleResult<T>(
        OperationResult<T> result,
        Func<T, ActionResult>? onSuccess = null,
        Func<string?, ActionResult>? onFailure = null)
    {
        if (!result.Successful)
        {
            return onFailure?.Invoke(result.Error) ?? MapFailure(result);
        }

        if (result.Data is null)
        {
            return NoContent();
        }

        return onSuccess?.Invoke(result.Data) ?? Ok(result.Data);
    }

    private ActionResult MapFailure(OperationResult result)
    {
        return result.FailureType switch
        {
            OperationFailureType.NotFound => NotFound(result.Error),
            OperationFailureType.Forbidden => string.IsNullOrWhiteSpace(result.Error)
                ? Forbid()
                : StatusCode(StatusCodes.Status403Forbidden, result.Error),
            OperationFailureType.Unauthorized => Unauthorized(result.Error),
            OperationFailureType.Conflict => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }
}
