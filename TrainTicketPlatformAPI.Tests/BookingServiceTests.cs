using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        private void SeedTrain(TrainTicketDbContext db)
        {
            db.Trains.Add(new Train
            {
                Id = 1,
                Name = "T1",
                DepartureStation = "StationA",
                ArrivalStation = "StationB",
                DepartureTime = DateTime.UtcNow.AddHours(-2),
                ArrivalTime = DateTime.UtcNow.AddHours(-1),
                Price = 50.0m
            });
        }

        private void SeedSeat(TrainTicketDbContext db, int seatId, bool isAvailable)
        {
            db.Seats.Add(new Seat
            {
                Id = seatId,
                TrainId = 1,
                Coach = "A",
                Number = seatId.ToString(),
                ClassType = "Economy",
                IsAvailable = isAvailable
            });
        }

        // 1) Creation Tests

        [Test]
        public async Task CreateBookingAsync_ReservesSeat_WhenAvailable()
        {
            var db = NewDb("CreateBookingTest");
            SeedTrain(db);
            SeedSeat(db, 1, true);
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            // pass a fresh Booking object
            var toCreate = new Booking
            {
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            };

            var result = await svc.CreateBookingAsync(toCreate);

            Assert.That(result.Id, Is.GreaterThan(0));
            var seatAfter = await db.Seats.FindAsync(1);
            Assert.That(seatAfter.IsAvailable, Is.False);
        }

        [Test]
        public void CreateBookingAsync_Throws_WhenSeatUnavailable()
        {
            var db = NewDb("UnavailableSeatTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            db.SaveChanges();

            var svc = new BookingService(db);
            var toCreate = new Booking
            {
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                TravelDate = DateTime.UtcNow.AddDays(1)
            };

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.CreateBookingAsync(toCreate)
            );
        }

        // 2) Cancellation Tests

        [Test]
        public void CancelBookingAsync_Throws_WhenTooCloseToTravel()
        {
            var db = NewDb("CancelTooLateTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddMinutes(30),
                PaymentStatus = "Pending"
            });
            db.SaveChanges();

            var svc = new BookingService(db);

            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.CancelBookingAsync(1),
                "Cannot cancel booking within 1 hour of travel date"
            );
        }

        [Test]
        public async Task CancelBookingAsync_MarksCancelled_AndFreesSeat()
        {
            var db = NewDb("CancelBookingTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);
            await svc.CancelBookingAsync(1);

            var b = await db.Bookings.FindAsync(1);
            Assert.That(b.IsCancelled, Is.True);
            Assert.That(b.CancellationDate, Is.Not.Null);
            var seat = await db.Seats.FindAsync(1);
            Assert.That(seat.IsAvailable, Is.True);
        }

        // 3) Rescheduling Tests

        [Test]
        public async Task UpdateBookingAsync_AllowsSeatChange_WhenNewSeatAvailable()
        {
            var db = NewDb("RescheduleSeatTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            SeedSeat(db, 2, true);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);

            // pass *new* Booking object with just the changes
            var toReschedule = new Booking
            {
                Id = 1,
                SeatId = 2,                           // new seat
                TravelDate = DateTime.UtcNow.AddDays(1)   // same date
            };

            var updated = await svc.UpdateBookingAsync(toReschedule);

            Assert.That(updated.SeatId, Is.EqualTo(2));
            var oldSeat = await db.Seats.FindAsync(1);
            var newSeat = await db.Seats.FindAsync(2);
            Assert.That(oldSeat.IsAvailable, Is.True);
            Assert.That(newSeat.IsAvailable, Is.False);
        }

        [Test]
        public void UpdateBookingAsync_Throws_WhenNewSeatUnavailable()
        {
            var db = NewDb("RescheduleToBadSeatTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            SeedSeat(db, 2, false);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(2),
                PaymentStatus = "Pending"
            });
            db.SaveChanges();

            var svc = new BookingService(db);

            var toReschedule = new Booking { Id = 1, SeatId = 2 };
            Assert.ThrowsAsync<InvalidOperationException>(
                () => svc.UpdateBookingAsync(toReschedule),
                "New seat unavailable"
            );
        }

        [Test]
        public async Task UpdateBookingAsync_AllowsDateChange_WhenSeatFreeOnNewDate()
        {
            var db = NewDb("RescheduleDateTest");
            SeedTrain(db);
            SeedSeat(db, 1, false);
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = 1,
                SeatId = 1,
                BookingDate = DateTime.UtcNow.AddDays(-1),
                TravelDate = DateTime.UtcNow.AddDays(1),
                PaymentStatus = "Pending"
            });
            await db.SaveChangesAsync();

            var svc = new BookingService(db);
            var newTravelDate = DateTime.UtcNow.AddDays(2);

            var toReschedule = new Booking
            {
                Id = 1,
                SeatId = 1,
                TravelDate = newTravelDate
            };
            var updated = await svc.UpdateBookingAsync(toReschedule);

            Assert.That(updated.TravelDate.Date, Is.EqualTo(newTravelDate.Date));
            var seat = await db.Seats.FindAsync(1);
            Assert.That(seat.IsAvailable, Is.False);
        }
    }
}

