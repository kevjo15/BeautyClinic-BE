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
    [Route("api/notifications")]
    [ApiController]
    public class NotificationController : BaseApiController
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDTO>>> GetUserNotifications([FromQuery] int limit = 20)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var query = new GetUserNotificationsQuery { UserId = userId, Limit = Math.Clamp(limit, 1, 50) };
            var notifications = await _mediator.Send(query);

            return Ok(notifications);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid id)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var command = new MarkNotificationAsReadCommand 
            { 
                NotificationId = id, 
                UserId = userId 
            };

            var result = await _mediator.Send(command);
            return HandleResult(result, () => Ok(), error => NotFound(error));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(Guid id)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var command = new DeleteNotificationCommand 
            { 
                NotificationId = id, 
                UserId = userId 
            };

            var result = await _mediator.Send(command);
            return HandleResult(result, () => Ok(), error => NotFound(error));
        }
    }
}
