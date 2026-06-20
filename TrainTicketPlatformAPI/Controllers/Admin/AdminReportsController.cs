using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/reports")]
    public class AdminReportsController : ControllerBase
    {
        private readonly TrainTicketDbContext _db;
        private readonly IBookingService _bookingService;

        public AdminReportsController(TrainTicketDbContext db, IBookingService bookingService)
        {
            _db = db;
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

        [HttpGet("revenue")]
        public async Task<ActionResult<AdminRevenueReportDto>> GetRevenueReport(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            if (to < from)
                return BadRequest("'to' must be on or after 'from'.");

            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1).AddTicks(-1);
            var bookingReport = await _bookingService.GenerateBookingReportAsync(fromDate, toDate);

            var payments = await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Trip)
                        .ThenInclude(t => t!.TrainRoute)
                            .ThenInclude(r => r.DepartureStation)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Trip)
                        .ThenInclude(t => t!.TrainRoute)
                            .ThenInclude(r => r.ArrivalStation)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Train)
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            var successfulPayments = payments
                .Where(p => p.Status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var refundedPayments = payments
                .Where(p => p.Status.Equals("Refunded", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var grossRevenue = successfulPayments.Sum(p => p.Amount);
            var refunds = refundedPayments.Sum(p => p.Amount);
            var paidBookings = successfulPayments.Select(p => p.BookingId).Distinct().Count();

            var dailyRevenue = Enumerable.Range(0, (toDate.Date - fromDate).Days + 1)
                .Select(offset =>
                {
                    var date = fromDate.AddDays(offset);
                    var dayPayments = payments.Where(p => p.PaymentDate.Date == date).ToList();
                    var daySuccessful = dayPayments
                        .Where(p => p.Status.Equals("Successful", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    var dayRefunded = dayPayments
                        .Where(p => p.Status.Equals("Refunded", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    return new AdminRevenueDailyPointDto
                    {
                        Date = date,
                        Revenue = daySuccessful.Sum(p => p.Amount),
                        Refunds = dayRefunded.Sum(p => p.Amount),
                        Bookings = daySuccessful.Select(p => p.BookingId).Distinct().Count()
                    };
                })
                .ToList();

            var routeBreakdown = successfulPayments
                .GroupBy(p => GetRouteLabel(p.Booking))
                .Select(group => new AdminRevenueRouteDto
                {
                    Route = group.Key,
                    Revenue = group.Sum(p => p.Amount),
                    PaidBookings = group.Select(p => p.BookingId).Distinct().Count()
                })
                .OrderByDescending(r => r.Revenue)
                .Take(6)
                .ToList();

            var recentActivity = payments
                .Take(8)
                .Select(p => new AdminRevenueActivityDto
                {
                    BookingReference = p.Booking.BookingReference,
                    TicketNumber = p.Booking.TicketNumber,
                    PassengerName = p.Booking.PassengerName
                        ?? p.Booking.User?.Email
                        ?? p.Booking.GuestEmail
                        ?? "Passenger",
                    Route = GetRouteLabel(p.Booking),
                    Date = p.PaymentDate,
                    Amount = p.Amount,
                    Status = p.Status
                })
                .ToList();

            return Ok(new AdminRevenueReportDto
            {
                From = fromDate,
                To = toDate,
                GrossRevenue = grossRevenue,
                Refunds = refunds,
                NetRevenue = grossRevenue - refunds,
                TotalBookings = bookingReport.TotalBookings,
                PaidBookings = paidBookings,
                RefundedBookings = refundedPayments.Select(p => p.BookingId).Distinct().Count(),
                AverageOrderValue = paidBookings == 0 ? 0m : grossRevenue / paidBookings,
                DailyRevenue = dailyRevenue,
                RouteBreakdown = routeBreakdown,
                RecentActivity = recentActivity
            });
        }

        private static string GetRouteLabel(Booking booking)
        {
            if (booking.Trip?.TrainRoute != null)
            {
                return $"{booking.Trip.TrainRoute.DepartureStation.Name} -> {booking.Trip.TrainRoute.ArrivalStation.Name}";
            }

            return $"{booking.Train.DepartureStation} -> {booking.Train.ArrivalStation}";
        }
    }
}
