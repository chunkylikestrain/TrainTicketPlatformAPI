using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Bookings;
using TrainTicketPlatformAPI.Contracts.Common;
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
        public async Task<ActionResult<PagedResponse<BookingDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? bookingStatus = null,
            [FromQuery] string? paymentStatus = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            var query = bookings.AsQueryable();

            if (!string.IsNullOrWhiteSpace(bookingStatus))
            {
                var normalizedBookingStatus = bookingStatus.Trim();
                query = query.Where(b =>
                    b.BookingStatus.Equals(normalizedBookingStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                var normalizedPaymentStatus = paymentStatus.Trim();
                query = query.Where(b =>
                    b.PaymentStatus.Equals(normalizedPaymentStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (from.HasValue)
                query = query.Where(b => b.BookingDate >= from.Value);

            if (to.HasValue)
                query = query.Where(b => b.BookingDate <= to.Value);

            var response = ToPagedResponse(
                query
                    .OrderByDescending(b => b.BookingDate)
                    .Select(ToDto),
                page,
                pageSize);

            return Ok(response);
        }

        private static PagedResponse<T> ToPagedResponse<T>(
            IEnumerable<T> source,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var totalCount = source.Count();
            var items = source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResponse<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        private static BookingDto ToDto(Booking booking) => new()
        {
            Id = booking.Id,
            UserId = booking.UserId,
            TrainId = booking.TrainId,
            TripId = booking.TripId,
            SeatId = booking.SeatId,
            BookingReference = booking.BookingReference,
            TicketNumber = booking.TicketNumber,
            GuestEmail = booking.GuestEmail,
            PassengerName = booking.PassengerName,
            BookingDate = booking.BookingDate,
            TravelDate = booking.TravelDate,
            ExpiresAtUtc = booking.ExpiresAtUtc,
            BookingStatus = booking.BookingStatus,
            PaymentStatus = booking.PaymentStatus,
            IsCancelled = booking.IsCancelled,
            CancellationDate = booking.CancellationDate,
            ConfirmedAtUtc = booking.ConfirmedAtUtc,
            RefundedAtUtc = booking.RefundedAtUtc
        };
    }
}
