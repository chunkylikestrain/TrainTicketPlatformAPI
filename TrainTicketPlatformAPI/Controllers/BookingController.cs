using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Contracts.Bookings;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
            => _bookingService = bookingService;

        // GET: api/Bookings
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings.Select(ToDto));
        }

        // GET: api/Bookings/me
        [HttpGet("me")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetMine()
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return Forbid();

            var bookings = await _bookingService.GetBookingsByUserAsync(currentUserId.Value);
            return Ok(bookings.Select(ToDto));
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetById(int id)
        {
            try
            {
                var b = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessUserResource(b.UserId))
                    return Forbid();

                return Ok(ToDto(b));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Bookings/user/42
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetByUser(int userId)
        {
            if (!CanAccessUserResource(userId))
                return Forbid();

            var list = await _bookingService.GetBookingsByUserAsync(userId);
            return Ok(list.Select(ToDto));
        }

        // GET: api/Bookings/availability?trainId=1&seatId=5&travelDate=2025-06-20
        [AllowAnonymous]
        [HttpGet("availability")]
        public async Task<ActionResult<bool>> CheckAvailability(
            [FromQuery] int trainId,
            [FromQuery] int seatId,
            [FromQuery] DateTime travelDate)
        {
            var available = await _bookingService
                .CheckSeatAvailabilityAsync(trainId, seatId, travelDate);
            return Ok(available);
        }

        // POST: api/Bookings
        [HttpPost]
        public async Task<ActionResult<BookingDto>> Create([FromBody] CreateBookingRequest request)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (!currentUserId.HasValue)
                    return Forbid();

                var booking = new Booking
                {
                    UserId = currentUserId.Value,
                    TrainId = request.TrainId,
                    TripId = request.TripId,
                    SeatId = request.SeatId,
                    TravelDate = request.TravelDate,
                    PaymentStatus = "Pending"
                };

                var created = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetById),
                                       new { id = created.Id },
                                       ToDto(created));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Bookings/5/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            return await Cancel(id);
        }

        // POST: api/Bookings/5/confirm
        [HttpPost("{id}/confirm")]
        public async Task<ActionResult<BookingDto>> Confirm(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessUserResource(booking.UserId))
                    return Forbid();

                var confirmed = await _bookingService.ConfirmBookingAsync(id);
                return Ok(ToDto(confirmed));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Bookings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Booking booking)
        {
            if (id != booking.Id)
                return BadRequest("ID mismatch");

            try
            {
                var existing = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessUserResource(existing.UserId))
                    return Forbid();

                var updated = await _bookingService.UpdateBookingAsync(booking);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);
                if (!CanAccessUserResource(booking.UserId))
                    return Forbid();

                await _bookingService.CancelBookingAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // GET: api/Bookings/report?from=2025-06-01&to=2025-06-30
        [Authorize(Roles = "Admin")]
        [HttpGet("report")]
        public async Task<ActionResult<BookingReport>> GetReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (to < from)
                return BadRequest("'to' must be on or after 'from'.");

            var report = await _bookingService.GenerateBookingReportAsync(from, to);
            return Ok(report);
        }

        private bool CanAccessUserResource(int userId)
        {
            if (User.IsInRole("Admin"))
                return true;

            var currentUserId = GetCurrentUserId();
            return currentUserId.HasValue && currentUserId.Value == userId;
        }

        private int? GetCurrentUserId()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(subject, out var currentUserId)
                ? currentUserId
                : null;
        }

        private static BookingDto ToDto(Booking booking) => new()
        {
            Id = booking.Id,
            UserId = booking.UserId,
            TrainId = booking.TrainId,
            TripId = booking.TripId,
            SeatId = booking.SeatId,
            BookingDate = booking.BookingDate,
            TravelDate = booking.TravelDate,
            PaymentStatus = booking.PaymentStatus,
            IsCancelled = booking.IsCancelled,
            CancellationDate = booking.CancellationDate
        };
    }
}
