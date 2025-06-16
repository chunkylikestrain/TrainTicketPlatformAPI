using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class TrainServiceTests
    {
        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        private void SeedTrain(TrainTicketDbContext db, int id = 1)
        {
            db.Trains.Add(new Train
            {
                Id = id,
                Name = $"T{id}",
                DepartureStation = "StationA",
                ArrivalStation = "StationB",
                DepartureTime = DateTime.UtcNow.AddHours(-5),
                ArrivalTime = DateTime.UtcNow.AddHours(-3),
                Price = 99.99m
            });
        }

        [Test]
        public async Task GetAllTrainsAsync_Returns_AllTrains()
        {
            // Arrange
            var db = NewDb("GetAllTrains");
            SeedTrain(db, 1);
            SeedTrain(db, 2);
            await db.SaveChangesAsync();
            var svc = new TrainService(db);

            // Act
            var all = await svc.GetAllTrainsAsync();
            var list = all.ToList();

            // Assert
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list, Has.Exactly(1).Matches<Train>(t => t.Id == 1));
            Assert.That(list, Has.Exactly(1).Matches<Train>(t => t.Id == 2));
        }

        [Test]
        public async Task GetTrainByIdAsync_Returns_Train_WhenExists()
        {
            // Arrange
            var db = NewDb("GetByIdExists");
            SeedTrain(db, 42);
            await db.SaveChangesAsync();
            var svc = new TrainService(db);

            // Act
            var train = await svc.GetTrainByIdAsync(42);

            // Assert
            Assert.That(train.Id, Is.EqualTo(42));
            Assert.That(train.Name, Is.EqualTo("T42"));
        }

        [Test]
        public void GetTrainByIdAsync_Throws_WhenNotFound()
        {
            // Arrange
            var db = NewDb("GetByIdMissing");
            var svc = new TrainService(db);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.GetTrainByIdAsync(123)
            );
        }

        [Test]
        public async Task CreateTrainAsync_AddsNewTrain()
        {
            // Arrange
            var db = NewDb("CreateTrain");
            var svc = new TrainService(db);
            var toCreate = new Train
            {
                Name = "NewExpress",
                DepartureStation = "Home",
                ArrivalStation = "Away",
                DepartureTime = DateTime.UtcNow,
                ArrivalTime = DateTime.UtcNow.AddHours(2),
                Price = 45.50m
            };

            // Act
            var created = await svc.CreateTrainAsync(toCreate);

            // Assert
            Assert.That(created.Id, Is.GreaterThan(0));
            var fetched = await db.Trains.FindAsync(created.Id);
            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched!.Name, Is.EqualTo("NewExpress"));
        }

        [Test]
        public async Task UpdateTrainAsync_UpdatesFields_WhenExists()
        {
            // Arrange
            var db = NewDb("UpdateTrain");
            SeedTrain(db, 7);
            await db.SaveChangesAsync();
            var svc = new TrainService(db);

            // Act
            var toUpdate = new Train
            {
                Id = 7,
                Name = "UpdatedName",
                DepartureStation = "X",
                ArrivalStation = "Y",
                DepartureTime = DateTime.UtcNow.AddHours(-1),
                ArrivalTime = DateTime.UtcNow.AddHours(+1),
                Price = 123.45m
            };
            var updated = await svc.UpdateTrainAsync(toUpdate);

            // Assert
            Assert.That(updated.Id, Is.EqualTo(7));
            Assert.That(updated.Name, Is.EqualTo("UpdatedName"));
            Assert.That(updated.Price, Is.EqualTo(123.45m));
        }

        [Test]
        public void UpdateTrainAsync_Throws_WhenNotFound()
        {
            // Arrange
            var db = NewDb("UpdateMissing");
            var svc = new TrainService(db);
            var fake = new Train { Id = 99 };

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.UpdateTrainAsync(fake)
            );
        }

        [Test]
        public async Task DeleteTrainAsync_RemovesTrain_WhenExists()
        {
            // Arrange
            var db = NewDb("DeleteTrain");
            SeedTrain(db, 55);
            await db.SaveChangesAsync();
            var svc = new TrainService(db);

            // Act
            await svc.DeleteTrainAsync(55);

            // Assert
            var still = await db.Trains.FindAsync(55);
            Assert.That(still, Is.Null);
        }

        [Test]
        public void DeleteTrainAsync_Throws_WhenNotFound()
        {
            // Arrange
            var db = NewDb("DeleteMissing");
            var svc = new TrainService(db);

            // Act & Assert
            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.DeleteTrainAsync(500)
            );
        }
    }
}
