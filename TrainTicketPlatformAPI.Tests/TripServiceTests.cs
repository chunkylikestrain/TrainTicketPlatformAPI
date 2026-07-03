using Microsoft.EntityFrameworkCore;
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
        public async Task SearchTripsAsync_ExcludesTripsDepartingBeforeRequestedTime()
        {
            var db = NewDb("Trips_SearchAfterTime");
            await SeedTripGraphAsync(db);
            var svc = new TripService(db);

            var results = (await svc.SearchTripsAsync(
                "WAW",
                "Krakow",
                new DateTime(2026, 7, 1),
                new TimeSpan(9, 0, 0))).ToList();

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task SearchItinerariesAsync_ReturnsDirectItineraryShape()
        {
            var db = NewDb("Trips_ItineraryDirect");
            await SeedTripGraphAsync(db);
            var svc = new TripService(db);

            var results = (await svc.SearchItinerariesAsync(
                "WAW",
                "Krakow",
                new DateTime(2026, 7, 1))).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].TransferCount, Is.EqualTo(0));
            Assert.That(results[0].LowestFare, Is.EqualTo(49.99m));
            Assert.That(results[0].Segments.Count(), Is.EqualTo(1));
            Assert.That(results[0].Segments.First().TripId, Is.EqualTo(1));
            Assert.That(results[0].Segments.First().DepartureStationCode, Is.EqualTo("WAW"));
            Assert.That(results[0].Segments.First().ArrivalStationCode, Is.EqualTo("KRK"));
        }

        [Test]
        public async Task SearchItinerariesAsync_ExcludesItinerariesDepartingBeforeRequestedTime()
        {
            var db = NewDb("Trips_ItineraryAfterTime");
            await SeedTripGraphAsync(db);
            var svc = new TripService(db);

            var results = (await svc.SearchItinerariesAsync(
                "WAW",
                "Krakow",
                new DateTime(2026, 7, 1),
                new TimeSpan(9, 0, 0))).ToList();

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task SearchItinerariesAsync_ReturnsOneTransferItinerary()
        {
            var db = NewDb("Trips_ItineraryTransfer");
            await SeedTripGraphAsync(db);
            var destination = new Station
            {
                Id = 4,
                Code = "GDN",
                Name = "Gdansk Glowny",
                City = "Gdansk"
            };
            var transfer = await db.Stations.FindAsync(2)
                ?? throw new InvalidOperationException("Transfer station missing");
            var train = new Train
            {
                Id = 2,
                Name = "IC 202",
                DepartureStation = transfer.Name,
                ArrivalStation = destination.Name,
                DepartureTime = new DateTime(2026, 7, 1, 11, 30, 0),
                ArrivalTime = new DateTime(2026, 7, 1, 15, 0, 0)
            };
            var route = new TrainRoute
            {
                Id = 2,
                DepartureStationId = transfer.Id,
                DepartureStation = transfer,
                ArrivalStationId = destination.Id,
                ArrivalStation = destination,
                DistanceKm = 520m,
                IsActive = true
            };
            var trip = new Trip
            {
                Id = 2,
                TrainId = train.Id,
                Train = train,
                TrainRouteId = route.Id,
                TrainRoute = route,
                DepartureTime = train.DepartureTime,
                ArrivalTime = train.ArrivalTime,
                Status = "Scheduled"
            };
            db.Stations.Add(destination);
            db.Trains.Add(train);
            db.TrainRoutes.Add(route);
            db.Trips.Add(trip);
            db.Fares.Add(new Fare
            {
                Id = 3,
                TripId = trip.Id,
                Trip = trip,
                ClassType = "Economy",
                Price = 60m,
                Currency = "USD"
            });
            await db.SaveChangesAsync();
            var svc = new TripService(db);

            var result = (await svc.SearchItinerariesAsync(
                "WAW",
                "Gdansk",
                new DateTime(2026, 7, 1))).Single();

            Assert.That(result.TransferCount, Is.EqualTo(1));
            Assert.That(result.Segments.Count(), Is.EqualTo(2));
            Assert.That(result.Segments.First().ArrivalStationCode, Is.EqualTo("KRK"));
            Assert.That(result.Segments.Last().DepartureStationCode, Is.EqualTo("KRK"));
            Assert.That(result.TotalTransferMinutes, Is.EqualTo(30));
            Assert.That(result.LowestFare, Is.EqualTo(109.99m));
        }

        [Test]
        public async Task SearchItinerariesAsync_DeduplicatesSameTrainSequenceWithDifferentTransferStations()
        {
            var db = NewDb("Trips_ItineraryDuplicateTrainSequence");
            var origin = new Station { Id = 10, Code = "KRK", Name = "Krakow Glowny", City = "Krakow" };
            var firstTransfer = new Station { Id = 11, Code = "WAC", Name = "Warszawa Centralna", City = "Warsaw" };
            var secondTransfer = new Station { Id = 12, Code = "WAZ", Name = "Warszawa Zachodnia", City = "Warsaw" };
            var destination = new Station { Id = 13, Code = "GDY", Name = "Gdynia Glowna", City = "Gdynia" };

            var firstTrain = new Train
            {
                Id = 10,
                Name = "EIP 3508",
                DepartureStation = origin.Name,
                ArrivalStation = secondTransfer.Name,
                DepartureTime = new DateTime(2026, 7, 1, 8, 0, 0),
                ArrivalTime = new DateTime(2026, 7, 1, 9, 30, 0)
            };
            var secondTrain = new Train
            {
                Id = 11,
                Name = "IC 56 Wawel",
                DepartureStation = firstTransfer.Name,
                ArrivalStation = destination.Name,
                DepartureTime = new DateTime(2026, 7, 1, 9, 30, 0),
                ArrivalTime = new DateTime(2026, 7, 1, 12, 40, 0)
            };

            var firstRoute = new TrainRoute
            {
                Id = 10,
                DepartureStationId = origin.Id,
                DepartureStation = origin,
                ArrivalStationId = secondTransfer.Id,
                ArrivalStation = secondTransfer,
                DistanceKm = 300m,
                IsActive = true,
                RouteStops =
                [
                    new TrainRouteStop
                    {
                        Id = 10,
                        StationId = firstTransfer.Id,
                        Station = firstTransfer,
                        StopOrder = 1,
                        ArrivalOffsetMinutes = 60,
                        DepartureOffsetMinutes = 62
                    }
                ]
            };
            var secondRoute = new TrainRoute
            {
                Id = 11,
                DepartureStationId = firstTransfer.Id,
                DepartureStation = firstTransfer,
                ArrivalStationId = destination.Id,
                ArrivalStation = destination,
                DistanceKm = 420m,
                IsActive = true,
                RouteStops =
                [
                    new TrainRouteStop
                    {
                        Id = 11,
                        StationId = secondTransfer.Id,
                        Station = secondTransfer,
                        StopOrder = 1,
                        ArrivalOffsetMinutes = 30,
                        DepartureOffsetMinutes = 32
                    }
                ]
            };
            var firstTrip = new Trip
            {
                Id = 10,
                TrainId = firstTrain.Id,
                Train = firstTrain,
                TrainRouteId = firstRoute.Id,
                TrainRoute = firstRoute,
                DepartureTime = firstTrain.DepartureTime,
                ArrivalTime = firstTrain.ArrivalTime,
                Status = "Scheduled"
            };
            var secondTrip = new Trip
            {
                Id = 11,
                TrainId = secondTrain.Id,
                Train = secondTrain,
                TrainRouteId = secondRoute.Id,
                TrainRoute = secondRoute,
                DepartureTime = secondTrain.DepartureTime,
                ArrivalTime = secondTrain.ArrivalTime,
                Status = "Scheduled"
            };

            db.Stations.AddRange(origin, firstTransfer, secondTransfer, destination);
            db.Trains.AddRange(firstTrain, secondTrain);
            db.TrainRoutes.AddRange(firstRoute, secondRoute);
            db.Trips.AddRange(firstTrip, secondTrip);
            db.Fares.AddRange(
                new Fare
                {
                    Id = 10,
                    TripId = firstTrip.Id,
                    Trip = firstTrip,
                    ClassType = "Economy",
                    Price = 90m,
                    Currency = "PLN"
                },
                new Fare
                {
                    Id = 11,
                    TripId = secondTrip.Id,
                    Trip = secondTrip,
                    ClassType = "Economy",
                    Price = 47m,
                    Currency = "PLN"
                });
            await db.SaveChangesAsync();
            var svc = new TripService(db);

            var results = (await svc.SearchItinerariesAsync(
                "Krakow",
                "Gdynia",
                new DateTime(2026, 7, 1))).ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            var result = results.Single();
            Assert.That(result.TransferCount, Is.EqualTo(1));
            Assert.That(result.Segments.Select(segment => segment.TrainName), Is.EqualTo(new[] { "EIP 3508", "IC 56 Wawel" }));
            Assert.That(result.Segments.First().ArrivalStationCode, Is.EqualTo("WAZ"));
            Assert.That(result.Segments.Last().DepartureStationCode, Is.EqualTo("WAZ"));
            Assert.That(result.TotalTransferMinutes, Is.EqualTo(32));
        }

        [Test]
        public async Task SearchItinerariesAsync_ExcludesTooTightTransfers()
        {
            var db = NewDb("Trips_ItineraryTightTransfer");
            await SeedTripGraphAsync(db);
            var destination = new Station
            {
                Id = 4,
                Code = "GDN",
                Name = "Gdansk Glowny",
                City = "Gdansk"
            };
            var transfer = await db.Stations.FindAsync(2)
                ?? throw new InvalidOperationException("Transfer station missing");
            var train = new Train
            {
                Id = 2,
                Name = "IC 202",
                DepartureStation = transfer.Name,
                ArrivalStation = destination.Name,
                DepartureTime = new DateTime(2026, 7, 1, 11, 5, 0),
                ArrivalTime = new DateTime(2026, 7, 1, 15, 0, 0)
            };
            var route = new TrainRoute
            {
                Id = 2,
                DepartureStationId = transfer.Id,
                DepartureStation = transfer,
                ArrivalStationId = destination.Id,
                ArrivalStation = destination,
                DistanceKm = 520m,
                IsActive = true
            };
            var trip = new Trip
            {
                Id = 2,
                TrainId = train.Id,
                Train = train,
                TrainRouteId = route.Id,
                TrainRoute = route,
                DepartureTime = train.DepartureTime,
                ArrivalTime = train.ArrivalTime,
                Status = "Scheduled"
            };
            db.Stations.Add(destination);
            db.Trains.Add(train);
            db.TrainRoutes.Add(route);
            db.Trips.Add(trip);
            await db.SaveChangesAsync();
            var svc = new TripService(db);

            var results = (await svc.SearchItinerariesAsync(
                "WAW",
                "Gdansk",
                new DateTime(2026, 7, 1))).ToList();

            Assert.That(results, Is.Empty);
        }

        [Test]
        public async Task SearchTripsAsync_ReturnsCallingPatternWithGeneratedStopTimes()
        {
            var db = NewDb("Trips_SearchCallingPattern");
            await SeedTripGraphAsync(db);
            var svc = new TripService(db);

            var result = (await svc.SearchTripsAsync(
                "WAW",
                "Krakow",
                new DateTime(2026, 7, 1))).Single();
            var stops = result.CallingPattern.ToList();

            Assert.That(stops.Count, Is.EqualTo(3));
            Assert.That(stops[0].StationCode, Is.EqualTo("WAW"));
            Assert.That(stops[0].DepartureTime, Is.EqualTo(new DateTime(2026, 7, 1, 8, 0, 0)));
            Assert.That(stops[1].StationCode, Is.EqualTo("KAT"));
            Assert.That(stops[1].StopType, Is.EqualTo("Major"));
            Assert.That(stops[1].DwellMinutes, Is.EqualTo(8));
            Assert.That(stops[1].ArrivalTime, Is.EqualTo(new DateTime(2026, 7, 1, 9, 26, 0)));
            Assert.That(stops[1].DepartureTime, Is.EqualTo(new DateTime(2026, 7, 1, 9, 34, 0)));
            Assert.That(stops[2].StationCode, Is.EqualTo("KRK"));
            Assert.That(stops[2].ArrivalTime, Is.EqualTo(new DateTime(2026, 7, 1, 11, 0, 0)));
        }

        [Test]
        public async Task SearchTripsAsync_DoesNotDuplicateImportedEndpointStops()
        {
            var db = NewDb("Trips_SearchImportedEndpointStops");
            await SeedTripGraphAsync(db);
            var route = await db.TrainRoutes
                .Include(item => item.RouteStops)
                .SingleAsync();
            var departure = await db.Stations.SingleAsync(station => station.Code == "WAW");
            var intermediate = await db.Stations.SingleAsync(station => station.Code == "KAT");
            var arrival = await db.Stations.SingleAsync(station => station.Code == "KRK");

            db.TrainRouteStops.RemoveRange(route.RouteStops);
            db.TrainRouteStops.AddRange(
                new TrainRouteStop
                {
                    TrainRouteId = route.Id,
                    StationId = departure.Id,
                    Station = departure,
                    StopOrder = 0,
                    DepartureOffsetMinutes = 0
                },
                new TrainRouteStop
                {
                    TrainRouteId = route.Id,
                    StationId = intermediate.Id,
                    Station = intermediate,
                    StopOrder = 1,
                    ArrivalOffsetMinutes = 86,
                    DepartureOffsetMinutes = 94
                },
                new TrainRouteStop
                {
                    TrainRouteId = route.Id,
                    StationId = arrival.Id,
                    Station = arrival,
                    StopOrder = 2,
                    ArrivalOffsetMinutes = 180
                });
            await db.SaveChangesAsync();
            var svc = new TripService(db);

            var result = (await svc.SearchTripsAsync(
                "WAW",
                "Krakow",
                new DateTime(2026, 7, 1))).Single();
            var stops = result.CallingPattern.ToList();

            Assert.That(stops.Select(stop => stop.StationCode), Is.EqualTo(new[] { "WAW", "KAT", "KRK" }));
            Assert.That(stops.Select(stop => stop.StopOrder), Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public async Task SearchTripsAsync_ReturnsOvernightTripWhenSegmentDepartsOnSearchDate()
        {
            var db = NewDb("Trips_SearchOvernightSegment");
            await SeedTripGraphAsync(db);
            var trip = await db.Trips.SingleAsync();
            trip.DepartureTime = new DateTime(2026, 6, 30, 23, 30, 0);
            trip.ArrivalTime = new DateTime(2026, 7, 1, 3, 30, 0);

            var route = await db.TrainRoutes
                .Include(item => item.RouteStops)
                .SingleAsync();
            route.RouteStops.Single().ArrivalOffsetMinutes = 90;
            route.RouteStops.Single().DepartureOffsetMinutes = 100;
            await db.SaveChangesAsync();
            var svc = new TripService(db);

            var results = (await svc.SearchTripsAsync(
                "Katowice",
                "Krakow",
                new DateTime(2026, 7, 1))).ToList();

            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].DepartureStationCode, Is.EqualTo("KAT"));
            Assert.That(results[0].DepartureTime, Is.EqualTo(new DateTime(2026, 7, 1, 1, 10, 0)));
            Assert.That(results[0].ArrivalTime, Is.EqualTo(new DateTime(2026, 7, 1, 3, 30, 0)));
        }

        [Test]
        public async Task SearchTripsAsync_ReturnsDisruptionBannerFields()
        {
            var db = NewDb("Trips_SearchDisruptionFields");
            await SeedTripGraphAsync(db);
            var trip = await db.Trips.FindAsync(1);
            trip!.DelayMinutes = 35;
            trip.Platform = "5";
            trip.Track = "8";
            trip.OriginalPlatform = "2";
            trip.OriginalTrack = "4";
            await db.SaveChangesAsync();
            var svc = new TripService(db);

            var result = (await svc.SearchTripsAsync(
                "WAW",
                "Krakow",
                new DateTime(2026, 7, 1))).Single();

            Assert.That(result.HasDisruption, Is.True);
            Assert.That(result.HasPlatformChange, Is.True);
            Assert.That(result.DelayMinutes, Is.EqualTo(35));
            Assert.That(result.Platform, Is.EqualTo("5"));
            Assert.That(result.OriginalPlatform, Is.EqualTo("2"));
            Assert.That(result.DisruptionSeverity, Is.EqualTo("Major"));
            Assert.That(result.DisruptionMessage, Is.EqualTo("This train is delayed by 35 minutes."));
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
