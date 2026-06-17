using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/reports")]
    public class AdminReportsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminReportsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet("bookings")]
        public async Task<ActionResult<BookingReport>> GetBookingReport(
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
