using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class SeatServiceTests
    {
        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        private void SeedTrain(TrainTicketDbContext db, int trainId = 1)
        {
            db.Trains.Add(new Train
            {
                Id = trainId,
                Name = $"T{trainId}",
                DepartureStation = "A",
                ArrivalStation = "B",
                DepartureTime = DateTime.UtcNow.AddHours(-2),
                ArrivalTime = DateTime.UtcNow.AddHours(-1),
                Price = 25.0m
            });
        }

        private Seat MakeSeat(int id, int trainId, bool isAvailable)
            => new Seat
            {
                Id = id,
                TrainId = trainId,
                Coach = "A",
                Number = id.ToString(),
                ClassType = "Economy",
                IsAvailable = isAvailable
            };

        [Test]
        public async Task GetAllSeatsAsync_Returns_AllSeats()
        {
            // Arrange
            var db = NewDb("AllSeats");
            SeedTrain(db, 1);
            db.Seats.Add(MakeSeat(1, 1, true));
            db.Seats.Add(MakeSeat(2, 1, false));
            await db.SaveChangesAsync();

            var svc = new SeatService(db);

            // Act
            var seats = await svc.GetAllSeatsAsync();
            var list = seats.ToList();

            // Assert
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list, Has.Exactly(1).Matches<Seat>(s => s.Id == 1));
            Assert.That(list, Has.Exactly(1).Matches<Seat>(s => s.Id == 2));
        }

        [Test]
        public async Task GetSeatByIdAsync_Returns_Seat_WhenExists()
        {
            // Arrange
            var db = NewDb("GetSeatById");
            SeedTrain(db, 1);
            db.Seats.Add(MakeSeat(5, 1, true));
            await db.SaveChangesAsync();

            var svc = new SeatService(db);

            // Act
            var seat = await svc.GetSeatByIdAsync(5);

            // Assert
            Assert.That(seat.Id, Is.EqualTo(5));
            Assert.That(seat.TrainId, Is.EqualTo(1));
            Assert.That(seat.IsAvailable, Is.True);
        }

        [Test]
        public void GetSeatByIdAsync_Throws_WhenNotFound()
        {
            // Arrange
            var db = NewDb("SeatMissing");
            var svc = new SeatService(db);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.GetSeatByIdAsync(99)
            );
        }

        [Test]
        public async Task GetSeatsByTrainAsync_Returns_OnlyThatTrain()
        {
            // Arrange
            var db = NewDb("SeatsByTrain");
            SeedTrain(db, 1);
            SeedTrain(db, 2);
            db.Seats.Add(MakeSeat(1, 1, true));
            db.Seats.Add(MakeSeat(2, 2, true));
            db.Seats.Add(MakeSeat(3, 1, false));
            await db.SaveChangesAsync();

            var svc = new SeatService(db);

            // Act
            var train1Seats = await svc.GetSeatsByTrainAsync(1);

            // Assert
            Assert.That(train1Seats.Count(), Is.EqualTo(2));
            Assert.That(train1Seats, Has.Exactly(1).Matches<Seat>(s => s.Id == 1));
            Assert.That(train1Seats, Has.Exactly(1).Matches<Seat>(s => s.Id == 3));
        }

        [Test]
        public async Task CreateSeatAsync_AddsNewSeat()
        {
            // Arrange
            var db = NewDb("CreateSeat");
            SeedTrain(db, 1);
            await db.SaveChangesAsync();

            var svc = new SeatService(db);
            var toAdd = new Seat
            {
                TrainId = 1,
                Coach = "B",
                Number = "12",
                ClassType = "First",
                IsAvailable = true
            };

            // Act
            var created = await svc.CreateSeatAsync(toAdd);

            // Assert
            Assert.That(created.Id, Is.GreaterThan(0));
            var fetched = await db.Seats.FindAsync(created.Id);
            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched!.Coach, Is.EqualTo("B"));
        }

        [Test]
        public async Task UpdateSeatAsync_UpdatesFields_WhenExists()
        {
            // Arrange
            var db = NewDb("UpdateSeat");
            SeedTrain(db, 1);
            db.Seats.Add(MakeSeat(8, 1, true));
            await db.SaveChangesAsync();

            var svc = new SeatService(db);
            var toUpdate = new Seat
            {
                Id = 8,
                TrainId = 1,
                Coach = "C",
                Number = "99",
                ClassType = "Business",
                IsAvailable = false
            };

            // Act
            var updated = await svc.UpdateSeatAsync(toUpdate);

            // Assert
            Assert.That(updated.Id, Is.EqualTo(8));
            Assert.That(updated.Coach, Is.EqualTo("C"));
            Assert.That(updated.IsAvailable, Is.False);
        }

        [Test]
        public void UpdateSeatAsync_Throws_WhenNotFound()
        {
            // Arrange
            var db = NewDb("UpdateSeatMissing");
            var svc = new SeatService(db);
            var fake = new Seat { Id = 99 };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.UpdateSeatAsync(fake)
            );
        }

        [Test]
        public async Task DeleteSeatAsync_RemovesSeat_WhenExists()
        {
            // Arrange
            var db = NewDb("DeleteSeat");
            SeedTrain(db, 1);
            db.Seats.Add(MakeSeat(22, 1, true));
            await db.SaveChangesAsync();

            var svc = new SeatService(db);

            // Act
            await svc.DeleteSeatAsync(22);

            // Assert
            var gone = await db.Seats.FindAsync(22);
            Assert.That(gone, Is.Null);
        }

        [Test]
        public void DeleteSeatAsync_Throws_WhenNotFound()
        {
            // Arrange
            var db = NewDb("DeleteSeatMissing");
            var svc = new SeatService(db);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.DeleteSeatAsync(777)
            );
        }
    }
}
