using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class StationServiceTests
    {
        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        [Test]
        public async Task GetStationsAsync_Filters_ByCodeNameOrCity()
        {
            var db = NewDb("Stations_Filter");
            db.Stations.AddRange(
                new Station { Id = 1, Code = "WAW", Name = "Central", City = "Warsaw" },
                new Station { Id = 2, Code = "KRK", Name = "Glowny", City = "Krakow" });
            await db.SaveChangesAsync();

            var svc = new StationService(db);

            var result = (await svc.GetStationsAsync("war")).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Code, Is.EqualTo("WAW"));
        }

        [Test]
        public async Task GetStationsAsync_Filters_ByLocalityName()
        {
            var db = NewDb("Stations_FilterByLocality");
            var country = new Country { Id = 1, Code = "PL", Name = "Poland" };
            var region = new StateRegion
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Code = "MZ",
                Name = "Mazowieckie"
            };
            var locality = new Locality
            {
                Id = 1,
                StateRegionId = region.Id,
                StateRegion = region,
                Name = "Smallville",
                Type = "Village"
            };

            db.Countries.Add(country);
            db.StateRegions.Add(region);
            db.Localities.Add(locality);
            db.Stations.Add(new Station
            {
                Id = 1,
                Code = "SMV",
                Name = "Smallville Halt",
                City = "",
                CountryId = country.Id,
                Country = country,
                StateRegionId = region.Id,
                StateRegion = region,
                LocalityId = locality.Id,
                Locality = locality
            });
            await db.SaveChangesAsync();

            var svc = new StationService(db);

            var result = (await svc.GetStationsAsync("smallville")).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].LocalityName, Is.EqualTo("Smallville"));
            Assert.That(result[0].LocalityType, Is.EqualTo("Village"));
            Assert.That(result[0].StateRegionName, Is.EqualTo("Mazowieckie"));
            Assert.That(result[0].CountryName, Is.EqualTo("Poland"));
        }

        [Test]
        public void GetStationByIdAsync_Throws_WhenStationMissing()
        {
            var db = NewDb("Stations_Missing");
            var svc = new StationService(db);

            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.GetStationByIdAsync(99));
        }
    }
}
