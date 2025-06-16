using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
            => _bookingService = bookingService;

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAll()
            => Ok(await _bookingService.GetAllBookingsAsync());

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetById(int id)
        {
            try
            {
                var b = await _bookingService.GetBookingByIdAsync(id);
                return Ok(b);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Bookings/user/42
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetByUser(int userId)
        {
            var list = await _bookingService.GetBookingsByUserAsync(userId);
            return Ok(list);
        }

        // GET: api/Bookings/availability?trainId=1&seatId=5&travelDate=2025-06-20
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
        public async Task<ActionResult<Booking>> Create(Booking booking)
        {
            try
            {
                var created = await _bookingService.CreateBookingAsync(booking);
                return CreatedAtAction(nameof(GetById),
                                       new { id = created.Id },
                                       created);
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
    }
}
