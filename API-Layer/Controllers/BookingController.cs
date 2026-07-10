using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application_Layer.Commands.BookingCommands.CreateBooking;
using Application_Layer.Commands.BookingCommands.UpdateBooking;
using Application_Layer.Commands.BookingCommands.CancelBooking;
using Application_Layer.Commands.BookingCommands.AssignEmployee;
using Application_Layer.Queries.BookingQueries.GetBookingById;
using Application_Layer.Queries.BookingQueries.GetAvailableTimeSlots;
using Application_Layer.Queries.BookingQueries.GetAllBookings;
using Application_Layer.Queries.BookingQueries.GetBookingsReport;
using Application_Layer.Queries.BookingQueries.GetEmployeeBookings;
using Application_Layer.Common;
using Application_Layer.DTOs;
using System.Text;

namespace API_Layer.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    [Authorize]
    public class BookingController : BaseApiController
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("report")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BookingsReportDTO>> GetReport(
            [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        {
            var report = await _mediator.Send(new GetBookingsReportQuery(from, to), ct);
            return Ok(report);
        }

        [HttpGet("report/csv")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReportCsv(
            [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        {
            var report = await _mediator.Send(new GetBookingsReportQuery(from, to), ct);
            var csv = BookingsReportCsvBuilder.Build(report);

            // UTF-8 BOM så svensk Excel läser åäö korrekt
            var bytes = Encoding.UTF8.GetPreamble()
                .Concat(Encoding.UTF8.GetBytes(csv))
                .ToArray();

            var fileName = $"bokningar_{report.From:yyyy-MM-dd}_{report.To:yyyy-MM-dd}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpPost]
        public async Task<ActionResult<BookingDTO>> CreateBooking([FromBody] CreateBookingDTO createDto)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            createDto.UserId = userId;
            var command = new CreateBookingCommand(createDto);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBookingById(Guid id)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var query = new GetBookingByIdQuery(id, userId, User.IsInRole("Employee"));
            var result = await _mediator.Send(query);
            return HandleResult(result);
        }

        [HttpGet("me")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUserId()
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var query = new GetBookingsByUserIdQuery(userId);
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] UpdateBookingDTO updateDto)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var command = new UpdateBookingCommand(id, userId, User.IsInRole("Employee"), updateDto);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            if (!TryGetCurrentUserId(out var userId)) return Unauthorized();

            var command = new CancelBookingCommand(id, userId, User.IsInRole("Employee"));
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<ActionResult<List<DateTime>>> GetAvailableTimeSlots(
            [FromQuery] Guid serviceId,
            [FromQuery] string employeeId,
            [FromQuery] DateTime date)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                return BadRequest("employeeId is required.");

            var query = new GetAvailableTimeSlotsQuery(serviceId, employeeId, date);
            var timeSlots = await _mediator.Send(query);
            return Ok(timeSlots);
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<BookingDTO>>> GetBookingsByUserIdForEmployee(string userId)
        {
            var query = new GetBookingsByUserIdQuery(userId);
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<BookingDTO>>> GetAllBookings()
        {
            var query = new GetAllBookingsQuery();
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }

        [HttpPut("{id}/employee")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<BookingDTO>> AssignEmployee(Guid id, [FromBody] AssignEmployeeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeId))
            {
                return BadRequest("EmployeeId is required.");
            }

            var command = new AssignEmployeeCommand
            {
                BookingId = id,
                EmployeeId = request.EmployeeId
            };

            var result = await _mediator.Send(command);
            return HandleResult(result, onFailure: error => NotFound(error));
        }

        [HttpGet("assigned")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<ActionResult<List<BookingDTO>>> GetMyAssigned([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!TryGetCurrentUserId(out var employeeId)) return Unauthorized();

            var start = from ?? DateTime.UtcNow.Date;
            var end = to ?? start.AddDays(7);

            var query = new GetEmployeeBookingsQuery
            {
                EmployeeId = employeeId,
                From = start,
                To = end
            };
            var bookings = await _mediator.Send(query);
            return Ok(bookings);
        }
    }

    public class AssignEmployeeRequest
    {
        public string EmployeeId { get; set; } = string.Empty;
    }
}
