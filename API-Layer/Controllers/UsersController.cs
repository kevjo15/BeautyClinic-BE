using Application_Layer.Queries.UserQueries.GetEmployees;
using Application_Layer.Queries.UserQueries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Layer.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(string id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        return user is null ? NotFound($"User with ID {id} was not found.") : Ok(user);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-only")]
    public IActionResult AdminOnly()
    {
        return Ok("This is an Admin-only area.");
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var employees = await _mediator.Send(new GetEmployeesQuery());
        return Ok(employees);
    }
}
