using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        private async Task SeedPaymentGraphAsync(TrainTicketDbContext db)
        {
            db.Trains.Add(new Train
            {
                Id = 1,
                Name = "T1",
                DepartureStation = "A",
                ArrivalStation = "B",
                DepartureTime = DateTime.UtcNow.AddHours(-1),
                ArrivalTime = DateTime.UtcNow.AddHours(+1)
            });
            db.Seats.Add(new Seat
            {
                Id = 1,
                TrainId = 1,
                Coach = "A",
                Number = "1",
                ClassType = "Economy",
                IsAvailable = true
            });
            db.Trips.Add(new Trip
            {
                Id = 1,
                TrainId = 1,
                TrainRouteId = 1,
                DepartureTime = DateTime.UtcNow.AddHours(1),
                ArrivalTime = DateTime.UtcNow.AddHours(3),
                Status = "Scheduled"
            });
            db.Fares.Add(new Fare
            {
                Id = 1,
                TripId = 1,
                ClassType = "Economy",
                Price = 49.99m,
                Currency = "USD"
            });
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                TripId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddMinutes(-1),
                TravelDate = DateTime.UtcNow.AddHours(1).Date,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(14),
                BookingStatus = "PendingPayment",
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();
        }

        [Test]
        public async Task CreatePaymentIntentAsync_ReturnsIntentForBooking()
        {
            var db = NewDb("PayIntent_Create");
            await SeedPaymentGraphAsync(db);
            var svc = new PaymentService(db);

            var intent = await svc.CreatePaymentIntentAsync(1);

            Assert.That(intent.PaymentIntentId, Is.EqualTo("pi_1"));
            Assert.That(intent.BookingId, Is.EqualTo(1));
            Assert.That(intent.Amount, Is.EqualTo(49.99m));
            Assert.That(intent.Currency, Is.EqualTo("USD"));
            Assert.That(intent.TestPaymentMethodTokens, Does.Contain(PaymentService.SuccessToken));
            Assert.That(intent.TestPaymentMethodTokens, Does.Contain(PaymentService.FailToken));
        }

        [Test]
        public async Task ConfirmPaymentAsync_Succeeds_ForSuccessToken()
        {
            var db = NewDb("Pay_TokenSuccess");
            await SeedPaymentGraphAsync(db);
            var svc = new PaymentService(db);

            var payment = await svc.ConfirmPaymentAsync("pi_1", PaymentService.SuccessToken);

            Assert.That(payment.Status, Is.EqualTo("Successful"));
            Assert.That(payment.PaymentIntentId, Is.EqualTo("pi_1"));
            Assert.That(payment.PaymentMethodToken, Is.EqualTo(PaymentService.SuccessToken));
            Assert.That(payment.Amount, Is.EqualTo(49.99m));

            var booking = await db.Bookings.FindAsync(1);
            Assert.That(booking!.PaymentStatus, Is.EqualTo("Successful"));
            Assert.That(booking.BookingStatus, Is.EqualTo("Confirmed"));
        }

        [Test]
        public async Task ConfirmPaymentAsync_Fails_ForFailToken()
        {
            var db = NewDb("Pay_TokenFail");
            await SeedPaymentGraphAsync(db);
            var svc = new PaymentService(db);

            var payment = await svc.ConfirmPaymentAsync("pi_1", PaymentService.FailToken);

            Assert.That(payment.Status, Is.EqualTo("Failed"));
            var booking = await db.Bookings.FindAsync(1);
            Assert.That(booking!.PaymentStatus, Is.EqualTo("Failed"));
            Assert.That(booking.BookingStatus, Is.EqualTo("PendingPayment"));
        }

        [Test]
        public void ConfirmPaymentAsync_Throws_WhenBookingHoldExpired()
        {
            var db = NewDb("Pay_ExpiredHold");
            SeedPaymentGraphAsync(db).GetAwaiter().GetResult();
            var booking = db.Bookings.Find(1)
                          ?? throw new InvalidOperationException("Seed booking missing");
            booking.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
            db.SaveChanges();

            var svc = new PaymentService(db);

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.ConfirmPaymentAsync("pi_1", PaymentService.SuccessToken)
            );
            Assert.That(booking.BookingStatus, Is.EqualTo("Expired"));
        }
    }
}
