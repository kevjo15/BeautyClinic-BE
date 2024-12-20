using Microsoft.AspNetCore.Mvc;
using MediatR;
using System;
using System.Threading.Tasks;
using Application_Layer.Commands.BookingCommands.CreateBooking;
using Application_Layer.DTOs;

namespace API_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(CreateBookingCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBookingById(Guid id)
        {
            var query = new GetBookingByIdQuery(id);
            var booking = await _mediator.Send(query);
            return Ok(booking);
        }

        [HttpGet("user/bookings")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUserId()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var query = new GetBookingsByUserIdQuery(userId);
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(Guid id, UpdateBookingCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            var command = new CancelBookingCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("available-time-slots")]
        public async Task<ActionResult<List<DateTime>>> GetAvailableTimeSlots([FromQuery] GetAvailableTimeSlotsQuery query)
        {
            var timeSlots = await _mediator.Send(query);
            return Ok(timeSlots);
        }

        [HttpGet("user/{userId}/bookings")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUserIdForEmployee(string userId)
        {
            var query = new GetBookingsByUserIdQuery(userId);
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        [HttpGet("all")]
        [Authorize(Roles = "Employee")]
        public async Task<ActionResult<List<BookingDTO>>> GetAllBookings()
        {
            var query = new GetAllBookingsQuery();
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        // Implement endpoints for updating, deleting, and retrieving bookings
        // ...
    }
} 