using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class BookingHoldExpiryService : IBookingHoldExpiryService
    {
        private readonly TrainTicketDbContext _db;

        public BookingHoldExpiryService(TrainTicketDbContext db)
            => _db = db;

        public async Task<int> ExpireStaleHoldsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var staleBookings = await _db.Bookings
                .Where(b =>
                    !b.IsCancelled &&
                    b.BookingStatus == "PendingPayment" &&
                    b.ExpiresAtUtc.HasValue &&
                    b.ExpiresAtUtc.Value <= now)
                .ToListAsync(cancellationToken);

            if (staleBookings.Count == 0)
                return 0;

            foreach (var booking in staleBookings)
                booking.BookingStatus = "Expired";

            var staleOrderIds = staleBookings
                .Where(booking => booking.BookingOrderId.HasValue)
                .Select(booking => booking.BookingOrderId!.Value)
                .Distinct()
                .ToList();
            var staleOrders = staleOrderIds.Count == 0
                ? new List<BookingOrder>()
                : await _db.BookingOrders
                    .Where(order =>
                        staleOrderIds.Contains(order.Id) &&
                        order.BookingStatus == "PendingPayment")
                    .ToListAsync(cancellationToken);

            foreach (var order in staleOrders)
                order.BookingStatus = "Expired";

            await _db.SaveChangesAsync(cancellationToken);
            return staleBookings.Count + staleOrders.Count;
        }
    }
}
