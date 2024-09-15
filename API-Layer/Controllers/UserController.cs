using Application_Layer.Commands.UserCommands;
using Application_Layer.Commands.UserCommands.GetUserById;
using Application_Layer.Commands.UserCommands.Login;
using Application_Layer.DTO_s;
using Domain_Layer.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok("This is an Admin-only area.");
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
