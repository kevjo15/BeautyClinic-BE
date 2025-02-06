using Application_Layer.Commands.NotificationCommands;
using Application_Layer.DTOs;
using Application_Layer.Queries.NotificationQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API_Layer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDTO>>> GetUserNotifications()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var query = new GetUserNotificationsQuery { UserId = userId };
            var notifications = await _mediator.Send(query);

            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new MarkNotificationAsReadCommand 
            { 
                NotificationId = id, 
                UserId = userId 
            };

            try
            {
                await _mediator.Send(command);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var command = new DeleteNotificationCommand 
            { 
                NotificationId = id, 
                UserId = userId 
            };

            try
            {
                await _mediator.Send(command);
                return Ok();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
