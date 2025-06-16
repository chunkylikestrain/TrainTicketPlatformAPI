using System;
using System.Threading.Tasks;
using NUnit.Framework;
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

        private void SeedTrainAndSeat(TrainTicketDbContext db)
        {
            db.Trains.Add(new Train
            {
                Id = 1,
                Name = "T1",
                DepartureStation = "A",
                ArrivalStation = "B",
                DepartureTime = DateTime.UtcNow.AddHours(-1),
                ArrivalTime = DateTime.UtcNow.AddHours(+1),
                Price = 100m
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
        }

        private void SeedBooking(TrainTicketDbContext db)
        {
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddHours(-2),
                TravelDate = DateTime.UtcNow.AddHours(+1),
                PaymentStatus = "Pending"
            });
        }

        [Test]
        public async Task ProcessPaymentAsync_Succeeds_ForVisa()
        {
            var db = NewDb("Pay_Visa");
            SeedTrainAndSeat(db);
            SeedBooking(db);
            await db.SaveChangesAsync();

            var svc = new PaymentService(db);
            var amount = 123.45m;
            var card = "4123456789012345";   // Visa starts with '4'

            var payment = await svc.ProcessPaymentAsync(1, amount, card);

            Assert.That(payment.Status, Is.EqualTo("Successful"));
            Assert.That(payment.Amount, Is.EqualTo(amount));
            Assert.That(payment.BookingId, Is.EqualTo(1));
            Assert.That(payment.PaymentDate.Kind, Is.EqualTo(DateTimeKind.Utc));

            var booking = await db.Bookings.FindAsync(1);
            Assert.That(booking.PaymentStatus, Is.EqualTo("Successful"));
        }

        [Test]
        public async Task ProcessPaymentAsync_Succeeds_ForMasterCardOldRange()
        {
            var db = NewDb("Pay_MC_Old");
            SeedTrainAndSeat(db);
            SeedBooking(db);
            await db.SaveChangesAsync();

            var svc = new PaymentService(db);
            var amount = 50m;
            var card = "5123456789012345"; // MasterCard old 51–55

            var payment = await svc.ProcessPaymentAsync(1, amount, card);

            Assert.That(payment.Status, Is.EqualTo("Successful"));
            Assert.That(payment.BookingId, Is.EqualTo(1));
            var booking = await db.Bookings.FindAsync(1);
            Assert.That(booking.PaymentStatus, Is.EqualTo("Successful"));
        }

        [Test]
        public async Task ProcessPaymentAsync_Succeeds_ForMasterCardNewRange()
        {
            var db = NewDb("Pay_MC_New");
            SeedTrainAndSeat(db);
            SeedBooking(db);
            await db.SaveChangesAsync();

            var svc = new PaymentService(db);
            var amount = 75m;
            var card = "2221001234567890"; // MasterCard new 2221–2720

            var payment = await svc.ProcessPaymentAsync(1, amount, card);

            Assert.That(payment.Status, Is.EqualTo("Successful"));
            var booking = await db.Bookings.FindAsync(1);
            Assert.That(booking.PaymentStatus, Is.EqualTo("Successful"));
        }

        [Test]
        public async Task ProcessPaymentAsync_Fails_ForInvalidCard()
        {
            var db = NewDb("Pay_Invalid");
            SeedTrainAndSeat(db);
            SeedBooking(db);
            await db.SaveChangesAsync();

            var svc = new PaymentService(db);
            var payment = await svc.ProcessPaymentAsync(1, 20m, "6011000000000000");

            Assert.That(payment.Status, Is.EqualTo("Failed"));
            var booking = await db.Bookings.FindAsync(1);
            Assert.That(booking.PaymentStatus, Is.EqualTo("Failed"));
        }

        [Test]
        public void ProcessPaymentAsync_Throws_WhenBookingNotFound()
        {
            var db = NewDb("Pay_NoBooking");
            var svc = new PaymentService(db);

            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.ProcessPaymentAsync(999, 10m, "4123456789012345")
            );
        }
    }
}

