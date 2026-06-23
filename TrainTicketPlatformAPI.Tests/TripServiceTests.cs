using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    [TestFixture]
    public class TripServiceTests
    {
        private TrainTicketDbContext NewDb(string name)
            => TestHelpers.GetInMemoryDb(name);

        private static async Task SeedTripGraphAsync(TrainTicketDbContext db)
        {
            var departure = new Station
            {
                Id = 1,
                Code = "WAW",
                Name = "Warsaw Central",
                City = "Warsaw"
            };
            var arrival = new Station
            {
                Id = 2,
                Code = "KRK",
                Name = "Krakow Glowny",
                City = "Krakow"
            };
            var intermediate = new Station
            {
                Id = 3,
                Code = "KAT",
                Name = "Katowice",
                City = "Katowice"
            };

            var train = new Train
            {
                Id = 1,
                Name = "IC 101",
                DepartureStation = "Warsaw",
                ArrivalStation = "Krakow",
                DepartureTime = new DateTime(2026, 7, 1, 8, 0, 0),
                ArrivalTime = new DateTime(2026, 7, 1, 11, 0, 0)
            };

            var route = new TrainRoute
            {
                Id = 1,
                DepartureStationId = departure.Id,
                ArrivalStationId = arrival.Id,
                DepartureStation = departure,
                ArrivalStation = arrival,
                DistanceKm = 290m,
                IsActive = true,
                RouteStops =
                [
                    new TrainRouteStop
                    {
                        Id = 1,
                        StationId = intermediate.Id,
                        Station = intermediate,
                        StopOrder = 1
                    }
                ]
            };

            var trip = new Trip
            {
                Id = 1,
                TrainId = train.Id,
                Train = train,
                TrainRouteId = route.Id,
                TrainRoute = route,
                DepartureTime = new DateTime(2026, 7, 1, 8, 0, 0),
                ArrivalTime = new DateTime(2026, 7, 1, 11, 0, 0),
                Status = "Scheduled"
            };

            db.Stations.AddRange(departure, arrival, intermediate);
            db.Trains.Add(train);
            db.TrainRoutes.Add(route);
            db.Trips.Add(trip);
            db.Fares.AddRange(
                new Fare
                {
                    Id = 1,
                    TripId = trip.Id,
                    Trip = trip,
                    ClassType = "Economy",
                    Price = 49.99m,
                    Currency = "USD"
                },
                new Fare
                {
                    Id = 2,
                    TripId = trip.Id,
                    Trip = trip,
                    ClassType = "First",
                    Price = 89.99m,
                    Currency = "USD"
                });
            db.Seats.AddRange(
                new Seat
                {
                    Id = 1,
                    TrainId = train.Id,
                    Train = train,
                    Coach = "A",
                    Number = "1",
                    ClassType = "Economy",
                    IsAvailable = true
                },
                new Seat
                {
                    Id = 2,
                    TrainId = train.Id,
                    Train = train,
                    Coach = "A",
                    Number = "2",
                    ClassType = "Economy",
                    IsAvailable = true
                });
            db.Bookings.Add(new Booking
            {
                Id = 1,
                UserId = 42,
                TrainId = train.Id,
                Train = train,
                TripId = trip.Id,
                Trip = trip,
                SeatId = 1,
                BookingDate = DateTime.UtcNow,
                TravelDate = trip.DepartureTime.Date,
                PaymentStatus = "Successful",
                IsCancelled = false
            });

            await db.SaveChangesAsync();
        }

        [Test]
        public async Task SearchTripsAsync_Returns_MatchingTripWithLowestFare()
        {
            var db = NewDb("Trips_Search");
            await SeedTripGraphAsync(db);
            var svc = new TripService(db);

            var results = (await svc.SearchTripsAsync(
                "WAW",
                "Krakow",
                new DateTime(2026, 7, 1))).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].TripId, Is.EqualTo(1));
            Assert.That(results[0].TrainName, Is.EqualTo("IC 101"));
            Assert.That(results[0].DepartureStationCode, Is.EqualTo("WAW"));
            Assert.That(results[0].ArrivalStationCode, Is.EqualTo("KRK"));
            Assert.That(results[0].LowestFare, Is.EqualTo(49.99m));
        }

        [Test]
        public async Task SearchTripsAsync_Matches_ByLocalityName()
        {
            var db = NewDb("Trips_SearchByLocality");
            await SeedTripGraphAsync(db);

            var country = new Country { Id = 1, Code = "PL", Name = "Poland" };
            var region = new StateRegion
            {
                Id = 1,
                CountryId = country.Id,
                Country = country,
                Code = "MZ",
                Name = "Mazowieckie"
            };
            var departureLocality = new Locality
            {
                Id = 1,
                StateRegionId = region.Id,
                StateRegion = region,
                Name = "Warsaw",
                Type = "City"
            };
            var arrivalLocality = new Locality
            {
                Id = 2,
                StateRegionId = region.Id,
                StateRegion = region,
                Name = "Krakow",
                Type = "City"
            };

            db.Countries.Add(country);
            db.StateRegions.Add(region);
            db.Localities.AddRange(departureLocality, arrivalLocality);

            var departureStation = await db.Stations.FindAsync(1);
            var arrivalStation = await db.Stations.FindAsync(2);
            departureStation!.City = "";
            departureStation!.CountryId = country.Id;
            departureStation.StateRegionId = region.Id;
            departureStation.LocalityId = departureLocality.Id;
            arrivalStation!.City = "";
            arrivalStation!.CountryId = country.Id;
            arrivalStation.StateRegionId = region.Id;
            arrivalStation.LocalityId = arrivalLocality.Id;
            await db.SaveChangesAsync();

            var svc = new TripService(db);

            var results = (await svc.SearchTripsAsync(
                "Warsaw",
                "Krakow",
                new DateTime(2026, 7, 1))).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].TripId, Is.EqualTo(1));
        }

        [Test]
        public async Task SearchTripsAsync_Matches_StationNamesWithoutDiacritics()
        {
            var db = NewDb("Trips_SearchWithoutDiacritics");
            await SeedTripGraphAsync(db);

            var departureStation = await db.Stations.FindAsync(1);
            var arrivalStation = await db.Stations.FindAsync(2);
            departureStation!.Name = "Warszawa Centralna";
            arrivalStation!.Name = "Kraków Główny";
            await db.SaveChangesAsync();

            var svc = new TripService(db);

            var results = (await svc.SearchTripsAsync(
                "Warszawa Centralna",
                "Krakow Glowny",
                new DateTime(2026, 7, 1))).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].ArrivalStationName, Is.EqualTo("Kraków Główny"));
        }

        [Test]
        public async Task GetSeatAvailabilityAsync_MarksBookedSeatUnavailable()
        {
            var db = NewDb("Trips_SeatAvailability");
            await SeedTripGraphAsync(db);
            var svc = new TripService(db);

            var seats = (await svc.GetSeatAvailabilityAsync(1)).ToList();

            Assert.That(seats.Count, Is.EqualTo(2));
            Assert.That(seats.Single(s => s.SeatId == 1).IsAvailable, Is.False);
            Assert.That(seats.Single(s => s.SeatId == 2).IsAvailable, Is.True);
        }

        [Test]
        public async Task GetSeatAvailabilityAsync_AllowsSameSeatOnNonOverlappingSegment()
        {
            var db = NewDb("Trips_SeatAvailabilityBySegment");
            await SeedTripGraphAsync(db);
            var existingBooking = await db.Bookings.FindAsync(1);
            existingBooking!.SegmentDepartureStationId = 1;
            existingBooking.SegmentArrivalStationId = 3;
            existingBooking.SegmentDepartureOrder = 0;
            existingBooking.SegmentArrivalOrder = 1;
            existingBooking.BookingStatus = "Confirmed";
            await db.SaveChangesAsync();
            var svc = new TripService(db);

            var firstLegSeats = (await svc.GetSeatAvailabilityAsync(1, 1, 3)).ToList();
            var secondLegSeats = (await svc.GetSeatAvailabilityAsync(1, 3, 2)).ToList();

            Assert.That(firstLegSeats.Single(s => s.SeatId == 1).IsAvailable, Is.False);
            Assert.That(secondLegSeats.Single(s => s.SeatId == 1).IsAvailable, Is.True);
        }

        [Test]
        public void GetTripByIdAsync_Throws_WhenTripMissing()
        {
            var db = NewDb("Trips_Missing");
            var svc = new TripService(db);

            Assert.ThrowsAsync<KeyNotFoundException>(
                () => svc.GetTripByIdAsync(999));
        }
    }
}
