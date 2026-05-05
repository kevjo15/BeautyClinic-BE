using Application_Layer.Commands.WorkDayCommands.GenerateWorkDays;
using Application_Layer.Commands.WorkDayCommands.SetWorkDays;
using Application_Layer.DTOs;
using Application_Layer.Queries.WorkDayQueries.GetWorkDays;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/workdays")]
[ApiController]
public class WorkDayController : BaseApiController
{
    private readonly IMediator _mediator;

    public WorkDayController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{employeeId}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<EmployeeWorkDayDTO>>> GetWorkDays(
        string employeeId,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
            return BadRequest("Ogiltigt datumformat. Använd yyyy-MM-dd.");

        var result = await _mediator.Send(new GetWorkDaysQuery(employeeId, fromDate, toDate));
        return Ok(result);
    }

    [HttpPut("{employeeId}")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> SetWorkDays(string employeeId, [FromBody] SetWorkDaysDTO dto)
    {
        if (!TryGetCurrentUserId(out var requesterId)) return Unauthorized();
        if (!User.IsInRole("Admin") && requesterId != employeeId) return Forbid();

        var command = new SetWorkDaysCommand { EmployeeId = employeeId, Dto = dto };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("{employeeId}/generate")]
    [Authorize(Roles = "Admin,Employee")]
    public async Task<IActionResult> GenerateWorkDays(string employeeId, [FromBody] GenerateWorkDaysDTO dto)
    {
        if (!TryGetCurrentUserId(out var requesterId)) return Unauthorized();
        if (!User.IsInRole("Admin") && requesterId != employeeId) return Forbid();

        var command = new GenerateWorkDaysCommand { EmployeeId = employeeId, Dto = dto };
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }
}
