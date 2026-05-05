using Application_Layer.Commands.ScheduleCommands.SetEmployeeSchedule;
using Application_Layer.DTOs;
using Application_Layer.Queries.ScheduleQueries.GetEmployeeSchedule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers
{
    [Route("api/schedules")]
    [ApiController]
    public class ScheduleController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ScheduleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{employeeId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<EmployeeScheduleDTO>>> GetSchedule(string employeeId)
        {
            var query = new GetEmployeeScheduleQuery(employeeId);
            var schedule = await _mediator.Send(query);
            return Ok(schedule);
        }

        [HttpPut("{employeeId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> SetSchedule(string employeeId, [FromBody] List<EmployeeScheduleDTO> schedule)
        {
            if (!TryGetCurrentUserId(out var requesterId)) return Unauthorized();

            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && requesterId != employeeId)
                return Forbid();

            var command = new SetEmployeeScheduleCommand(employeeId, schedule);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }
    }
}
