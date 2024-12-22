using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application_Layer.Commands.BookingCommands.CreateBooking;
using Application_Layer.Commands.BookingCommands.UpdateBooking;
using Application_Layer.Commands.BookingCommands.CancelBooking;
using Application_Layer.Queries.BookingQueries.GetBookingById;
using Application_Layer.Queries.BookingQueries.GetAvailableTimeSlots;
using Application_Layer.Queries.BookingQueries.GetAllBookings;
using Application_Layer.DTOs;
using System.Security.Claims;

namespace API_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("CreateBooking")]
        public async Task<ActionResult<BookingDTO>> Create([FromBody] CreateBookingDTO createDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            createDto.UserId = userId;
            var command = new CreateBookingCommand(createDto);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Booking);
        }

        [HttpGet("GetBookingByBookingId/{id}")]
        public async Task<ActionResult<BookingDTO>> GetBookingById(Guid id)
        {
            var query = new GetBookingByIdQuery(id);
            var booking = await _mediator.Send(query);
            
            // Verify the user owns this booking or is an employee
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (booking.UserId != userId && !User.IsInRole("Employee"))
            {
                return Forbid();
            }
            
            return Ok(booking);
        }

        [HttpGet("GetBookingsByUserId/MyBookings")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var query = new GetBookingsByUserIdQuery(userId);
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        [HttpPut("UpdateBookingById/{id}")]
        public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] UpdateBookingDTO updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Verify the user owns this booking or is an employee
            var booking = await _mediator.Send(new GetBookingByIdQuery(id));
            if (booking.UserId != userId && !User.IsInRole("Employee"))
            {
                return Forbid();
            }

            var command = new UpdateBookingCommand(id, updateDto);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("CancelBooking/{id}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Verify the user owns this booking or is an employee
            var booking = await _mediator.Send(new GetBookingByIdQuery(id));
            if (booking.UserId != userId && !User.IsInRole("Employee"))
            {
                return Forbid();
            }

            var command = new CancelBookingCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("available-time-slots")]
        [AllowAnonymous]
        public async Task<ActionResult<List<DateTime>>> GetAvailableTimeSlots([FromQuery] Guid serviceId, [FromQuery] DateTime date)
        {
            var query = new GetAvailableTimeSlotsQuery(serviceId, date);
            var timeSlots = await _mediator.Send(query);
            return Ok(timeSlots);
        }

        [HttpGet("GetBookingsByUserIdForEmployee/{userId}")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUserIdForEmployee(string userId)
        {
            var query = new GetBookingsByUserIdQuery(userId);
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        [HttpGet("GetAllBookings")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<List<BookingDTO>>> GetAllBookings()
        {
            var query = new GetAllBookingsQuery();
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }
    }
}
