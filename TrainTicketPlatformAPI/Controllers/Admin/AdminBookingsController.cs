using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Bookings;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/bookings")]
    public class AdminBookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminBookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings.Select(ToDto));
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
