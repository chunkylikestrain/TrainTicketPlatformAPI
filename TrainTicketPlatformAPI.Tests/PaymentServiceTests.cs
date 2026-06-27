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
            db.Users.Add(new User
            {
                Id = 42,
                Email = "loyal-passenger@example.com",
                PasswordHash = "hash",
                Phone = "555-4242",
                Role = "Passenger"
            });
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

        private async Task SeedLoyaltyPointsAsync(TrainTicketDbContext db, int points = 1000)
        {
            var account = new LoyaltyAccount
            {
                UserId = 42,
                RedeemablePoints = points,
                RedeemableValuePln = points / 100m,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
                UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
            };

            db.LoyaltyAccounts.Add(account);
            db.LoyaltyTransactions.Add(new LoyaltyTransaction
            {
                LoyaltyAccount = account,
                Type = "Adjustment",
                Status = "Available",
                Points = points,
                SourceAmount = 0m,
                Currency = "PLN",
                Reference = "TEST-POINTS",
                Description = "Test points",
                TransactionDateUtc = DateTime.UtcNow.AddDays(-1),
                ValidFromUtc = DateTime.UtcNow.AddDays(-1),
                ExpiresAtUtc = DateTime.UtcNow.AddYears(1)
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
        public async Task CreatePaymentIntentAsync_AppliesLoyaltyRedemption()
        {
            var db = NewDb("PayIntent_LoyaltySingle");
            await SeedPaymentGraphAsync(db);
            await SeedLoyaltyPointsAsync(db);
            var svc = new PaymentService(db, new LoyaltyService(db));

            var intent = await svc.CreatePaymentIntentAsync(1, redeemLoyaltyPoints: 200);

            Assert.Multiple(() =>
            {
                Assert.That(intent.OriginalAmount, Is.EqualTo(49.99m));
                Assert.That(intent.Amount, Is.EqualTo(47.99m));
                Assert.That(intent.LoyaltyPointsRedeemed, Is.EqualTo(200));
                Assert.That(intent.LoyaltyDiscountAmount, Is.EqualTo(2m));
            });

            var booking = await db.Bookings.FindAsync(1);
            Assert.That(booking!.LoyaltyPointsRedeemed, Is.EqualTo(200));
            Assert.That(booking.LoyaltyDiscountAmount, Is.EqualTo(2m));
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
        public async Task CreatePaymentIntentForOrderAsync_UsesStoredDiscountedTicketAmounts()
        {
            var db = NewDb("PayIntent_OrderDiscountedTotal");
            await SeedBookingOrderAsync(db);
            var bookings = db.Bookings.OrderBy(booking => booking.Id).ToList();
            bookings[0].BaseAmount = 100m;
            bookings[0].Amount = 100m;
            bookings[0].Currency = "PLN";
            bookings[1].BaseAmount = 100m;
            bookings[1].Amount = 63m;
            bookings[1].Currency = "PLN";
            await db.SaveChangesAsync();
            var svc = new PaymentService(db);

            var intent = await svc.CreatePaymentIntentForOrderAsync(1);

            Assert.That(intent.Amount, Is.EqualTo(163m));
            Assert.That(intent.Currency, Is.EqualTo("PLN"));
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
        public async Task ConfirmPaymentAsync_AwardsLoyaltyPoints_ForLoggedInTicketPurchase()
        {
            var db = NewDb("Pay_LoyaltySingleTicket");
            await SeedPaymentGraphAsync(db);
            var svc = new PaymentService(db, new LoyaltyService(db));

            await svc.ConfirmPaymentAsync("pi_1", PaymentService.SuccessToken);

            var account = db.LoyaltyAccounts.Single(account => account.UserId == 42);
            var transaction = db.LoyaltyTransactions.Single();

            Assert.Multiple(() =>
            {
                Assert.That(account.RedeemablePoints, Is.EqualTo(249));
                Assert.That(account.RedeemableValuePln, Is.EqualTo(2.49m));
                Assert.That(transaction.Type, Is.EqualTo("TicketPurchase"));
                Assert.That(transaction.Status, Is.EqualTo("Available"));
                Assert.That(transaction.Points, Is.EqualTo(249));
                Assert.That(transaction.SourceAmount, Is.EqualTo(49.99m));
                Assert.That(transaction.Currency, Is.EqualTo("USD"));
                Assert.That(transaction.BookingId, Is.EqualTo(1));
                Assert.That(transaction.BookingOrderId, Is.Null);
            });
        }

        [Test]
        public async Task ConfirmPaymentAsync_RedeemsPointsAndAwardsPointsOnCashAmount()
        {
            var db = NewDb("Pay_LoyaltyRedeemSingleTicket");
            await SeedPaymentGraphAsync(db);
            await SeedLoyaltyPointsAsync(db);
            var svc = new PaymentService(db, new LoyaltyService(db));

            await svc.CreatePaymentIntentAsync(1, redeemLoyaltyPoints: 200);
            var payment = await svc.ConfirmPaymentAsync("pi_1", PaymentService.SuccessToken);

            var account = db.LoyaltyAccounts.Single(account => account.UserId == 42);
            var redemption = db.LoyaltyTransactions.Single(transaction => transaction.Type == "Redemption");
            var purchase = db.LoyaltyTransactions.Single(transaction => transaction.Type == "TicketPurchase");

            Assert.Multiple(() =>
            {
                Assert.That(payment.Amount, Is.EqualTo(47.99m));
                Assert.That(payment.LoyaltyPointsRedeemed, Is.EqualTo(200));
                Assert.That(payment.LoyaltyDiscountAmount, Is.EqualTo(2m));
                Assert.That(redemption.Points, Is.EqualTo(-200));
                Assert.That(redemption.SourceAmount, Is.EqualTo(2m));
                Assert.That(purchase.Points, Is.EqualTo(239));
                Assert.That(purchase.SourceAmount, Is.EqualTo(47.99m));
                Assert.That(account.RedeemablePoints, Is.EqualTo(1039));
                Assert.That(account.RedeemableValuePln, Is.EqualTo(10.39m));
            });
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
        public async Task ConfirmPaymentAsync_AwardsLoyaltyPoints_ForEachOrderTicket()
        {
            var db = NewDb("Pay_LoyaltyOrderTickets");
            await SeedBookingOrderAsync(db);
            var svc = new PaymentService(db, new LoyaltyService(db));

            await svc.ConfirmPaymentAsync("pi_order_1", PaymentService.SuccessToken);

            var account = db.LoyaltyAccounts.Single(account => account.UserId == 42);
            var transactions = db.LoyaltyTransactions
                .OrderBy(transaction => transaction.BookingId)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(transactions, Has.Count.EqualTo(2));
                Assert.That(transactions.Select(transaction => transaction.Type), Is.All.EqualTo("TicketPurchase"));
                Assert.That(transactions.Select(transaction => transaction.Status), Is.All.EqualTo("Available"));
                Assert.That(transactions.Select(transaction => transaction.Points), Is.All.EqualTo(249));
                Assert.That(transactions.Select(transaction => transaction.BookingOrderId), Is.All.EqualTo(1));
                Assert.That(transactions.Select(transaction => transaction.BookingId), Is.EquivalentTo(new[] { 1, 2 }));
                Assert.That(account.RedeemablePoints, Is.EqualTo(498));
                Assert.That(account.RedeemableValuePln, Is.EqualTo(4.98m));
            });
        }

        [Test]
        public async Task ConfirmPaymentAsync_RedeemsPointsAcrossOrderTickets()
        {
            var db = NewDb("Pay_LoyaltyRedeemOrderTickets");
            await SeedBookingOrderAsync(db);
            await SeedLoyaltyPointsAsync(db);
            var svc = new PaymentService(db, new LoyaltyService(db));

            var intent = await svc.CreatePaymentIntentForOrderAsync(1, redeemLoyaltyPoints: 200);
            var payment = await svc.ConfirmPaymentAsync("pi_order_1", PaymentService.SuccessToken);

            var account = db.LoyaltyAccounts.Single(account => account.UserId == 42);
            var order = await db.BookingOrders.FindAsync(1);
            var bookings = db.Bookings.OrderBy(booking => booking.Id).ToList();
            var redemption = db.LoyaltyTransactions.Single(transaction => transaction.Type == "Redemption");
            var purchases = db.LoyaltyTransactions
                .Where(transaction => transaction.Type == "TicketPurchase")
                .OrderBy(transaction => transaction.BookingId)
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(intent.OriginalAmount, Is.EqualTo(99.98m));
                Assert.That(intent.Amount, Is.EqualTo(97.98m));
                Assert.That(payment.Amount, Is.EqualTo(97.98m));
                Assert.That(payment.LoyaltyPointsRedeemed, Is.EqualTo(200));
                Assert.That(order!.LoyaltyPointsRedeemed, Is.EqualTo(200));
                Assert.That(order.LoyaltyDiscountAmount, Is.EqualTo(2m));
                Assert.That(bookings.Select(booking => booking.LoyaltyPointsRedeemed), Is.All.EqualTo(100));
                Assert.That(bookings.Select(booking => booking.LoyaltyDiscountAmount), Is.All.EqualTo(1m));
                Assert.That(bookings.Select(booking => booking.Amount), Is.All.EqualTo(48.99m));
                Assert.That(redemption.Points, Is.EqualTo(-200));
                Assert.That(purchases.Select(transaction => transaction.Points), Is.All.EqualTo(244));
                Assert.That(account.RedeemablePoints, Is.EqualTo(1288));
            });
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
