using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Admin;
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

        [HttpPost("{id}/cancel-refund")]
        public async Task<ActionResult<BookingDto>> CancelAndRefund(int id, [FromBody] AdminCancelBookingRequest request)
        {
            try
            {
                var booking = await _bookingService.AdminCancelAndRefundAsync(id, request.Reason);
                return Ok(ToDto(booking));
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
            CancellationReason = booking.CancellationReason,
            ConfirmedAtUtc = booking.ConfirmedAtUtc,
            RefundedAtUtc = booking.RefundedAtUtc,
            TicketIssuedAtUtc = booking.TicketIssuedAtUtc,
            HasTicketArtifact = !string.IsNullOrWhiteSpace(booking.TicketQrPayload),
            TicketEmailStatus = booking.TicketEmailStatus,
            TicketEmailSentAtUtc = booking.TicketEmailSentAtUtc,
            TicketEmailRecipient = booking.TicketEmailRecipient,
            TrainName = string.IsNullOrWhiteSpace(booking.Train.Code) ? booking.Train.Name : booking.Train.Code,
            Route = booking.Trip?.TrainRoute == null
                ? $"{booking.Train.DepartureStation} -> {booking.Train.ArrivalStation}"
                : $"{booking.Trip.TrainRoute.DepartureStation.Name} -> {booking.Trip.TrainRoute.ArrivalStation.Name}",
            SeatLabel = $"Coach {booking.Seat.Coach}, seat {booking.Seat.Number}",
            Amount = booking.Amount > 0m ? booking.Amount : booking.Trip?.Fares
                .OrderByDescending(f => f.ClassType == booking.Seat.ClassType)
                .ThenBy(f => f.Price)
                .FirstOrDefault()?.Price ?? 0m
        };
    }
}
