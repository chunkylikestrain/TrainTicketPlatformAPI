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
            db.Seats.Add(new Seat
            {
                Id = 2,
                TrainId = 1,
                Coach = "A",
                Number = "2",
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

        private async Task SeedBookingOrderAsync(TrainTicketDbContext db)
        {
            await SeedPaymentGraphAsync(db);

            var order = new BookingOrder
            {
                Id = 1,
                UserId = 42,
                OrderReference = "ORD-TEST",
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-1),
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(14),
                BookingStatus = "PendingPayment",
                PaymentStatus = "Pending"
            };
            var secondBooking = new Booking
            {
                Id = 2,
                UserId = 42,
                TrainId = 1,
                TripId = 1,
                SeatId = 2,
                PassengerName = "Passenger Two",
                BookingDate = DateTime.UtcNow.AddMinutes(-1),
                TravelDate = DateTime.UtcNow.AddHours(1).Date,
                ExpiresAtUtc = order.ExpiresAtUtc,
                BookingStatus = "PendingPayment",
                PaymentStatus = "Pending"
            };
            var firstBooking = await db.Bookings.FindAsync(1)
                ?? throw new InvalidOperationException("Seed booking missing");
            firstBooking.BookingOrder = order;
            firstBooking.BookingOrderId = order.Id;
            firstBooking.PassengerName = "Passenger One";
            secondBooking.BookingOrder = order;
            order.Bookings.Add(firstBooking);
            order.Bookings.Add(secondBooking);
            db.BookingOrders.Add(order);
            db.Bookings.Add(secondBooking);
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
        public async Task CreatePaymentIntentForOrderAsync_ReturnsOneIntentForTotal()
        {
            var db = NewDb("PayIntent_OrderCreate");
            await SeedBookingOrderAsync(db);
            var svc = new PaymentService(db);

            var intent = await svc.CreatePaymentIntentForOrderAsync(1);

            Assert.That(intent.PaymentIntentId, Is.EqualTo("pi_order_1"));
            Assert.That(intent.BookingOrderId, Is.EqualTo(1));
            Assert.That(intent.BookingIds, Is.EquivalentTo(new[] { 1, 2 }));
            Assert.That(intent.Amount, Is.EqualTo(99.98m));
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
        public async Task ConfirmPaymentAsync_ConfirmsAllOrderBookings_ForOrderIntent()
        {
            var db = NewDb("Pay_OrderTokenSuccess");
            await SeedBookingOrderAsync(db);
            var svc = new PaymentService(db);

            var payment = await svc.ConfirmPaymentAsync("pi_order_1", PaymentService.SuccessToken);

            Assert.That(payment.Status, Is.EqualTo("Successful"));
            Assert.That(payment.BookingOrderId, Is.EqualTo(1));
            Assert.That(payment.BookingId, Is.Null);
            Assert.That(payment.Amount, Is.EqualTo(99.98m));

            var order = await db.BookingOrders.FindAsync(1);
            Assert.That(order!.PaymentStatus, Is.EqualTo("Successful"));
            Assert.That(order.BookingStatus, Is.EqualTo("Confirmed"));

            var bookings = db.Bookings.OrderBy(booking => booking.Id).ToList();
            Assert.That(bookings.Select(booking => booking.PaymentStatus), Is.All.EqualTo("Successful"));
            Assert.That(bookings.Select(booking => booking.BookingStatus), Is.All.EqualTo("Confirmed"));
            Assert.That(bookings.Select(booking => booking.TicketNumber), Is.All.Not.Empty);
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
