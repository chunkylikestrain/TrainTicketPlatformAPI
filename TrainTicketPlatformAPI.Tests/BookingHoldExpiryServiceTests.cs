using System;
using System.Threading.Tasks;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class BookingHoldExpiryServiceTests
    {
        private static TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        [Test]
        public async Task ExpireStaleHoldsAsync_ExpiresOnlyElapsedPendingHolds()
        {
            var db = NewDb(nameof(ExpireStaleHoldsAsync_ExpiresOnlyElapsedPendingHolds));
            db.Bookings.AddRange(
                CreateBooking(1, "PendingPayment", DateTime.UtcNow.AddMinutes(-1), isCancelled: false),
                CreateBooking(2, "PendingPayment", DateTime.UtcNow.AddMinutes(5), isCancelled: false),
                CreateBooking(3, "PendingPayment", DateTime.UtcNow.AddMinutes(-1), isCancelled: true),
                CreateBooking(4, "Confirmed", DateTime.UtcNow.AddMinutes(-1), isCancelled: false),
                CreateBooking(5, "PendingPayment", expiresAtUtc: null, isCancelled: false));
            await db.SaveChangesAsync();

            var service = new BookingHoldExpiryService(db);

            var expiredCount = await service.ExpireStaleHoldsAsync();

            Assert.That(expiredCount, Is.EqualTo(1));
            Assert.That((await db.Bookings.FindAsync(1))!.BookingStatus, Is.EqualTo("Expired"));
            Assert.That((await db.Bookings.FindAsync(2))!.BookingStatus, Is.EqualTo("PendingPayment"));
            Assert.That((await db.Bookings.FindAsync(3))!.BookingStatus, Is.EqualTo("PendingPayment"));
            Assert.That((await db.Bookings.FindAsync(4))!.BookingStatus, Is.EqualTo("Confirmed"));
            Assert.That((await db.Bookings.FindAsync(5))!.BookingStatus, Is.EqualTo("PendingPayment"));
        }

        [Test]
        public async Task ExpireStaleHoldsAsync_ReturnsZero_WhenNoHoldsHaveElapsed()
        {
            var db = NewDb(nameof(ExpireStaleHoldsAsync_ReturnsZero_WhenNoHoldsHaveElapsed));
            db.Bookings.Add(CreateBooking(1, "PendingPayment", DateTime.UtcNow.AddMinutes(5), isCancelled: false));
            await db.SaveChangesAsync();

            var service = new BookingHoldExpiryService(db);

            var expiredCount = await service.ExpireStaleHoldsAsync();

            Assert.That(expiredCount, Is.Zero);
            Assert.That((await db.Bookings.FindAsync(1))!.BookingStatus, Is.EqualTo("PendingPayment"));
        }

        private static Booking CreateBooking(
            int id,
            string bookingStatus,
            DateTime? expiresAtUtc,
            bool isCancelled)
        {
            return new Booking
            {
                Id = id,
                TrainId = 1,
                SeatId = id,
                BookingDate = DateTime.UtcNow.AddMinutes(-20),
                TravelDate = DateTime.UtcNow.AddDays(1),
                ExpiresAtUtc = expiresAtUtc,
                BookingStatus = bookingStatus,
                PaymentStatus = "Pending",
                IsCancelled = isCancelled
            };
        }
    }
}
