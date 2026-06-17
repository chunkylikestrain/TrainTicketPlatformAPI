using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Data
{
    public static class DevelopmentSeedData
    {
        public const string AdminEmail = "admin@trainticket.dev";
        public const string PassengerEmail = "passenger@trainticket.dev";
        public const string DefaultPassword = "Password123!";
        public static readonly DateTime MainTripDepartureUtc = new(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc);

        public static async Task SeedAsync(
            TrainTicketDbContext db,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            if (!configuration.GetValue("SeedData:UseDevelopmentSeedData", true))
                return;

            var country = await EnsureCountryAsync(db, cancellationToken);
            var mazowieckie = await EnsureStateRegionAsync(db, country, "MZ", "Mazowieckie", cancellationToken);
            var malopolskie = await EnsureStateRegionAsync(db, country, "MA", "Malopolskie", cancellationToken);
            var pomorskie = await EnsureStateRegionAsync(db, country, "PM", "Pomorskie", cancellationToken);

            var warsaw = await EnsureLocalityAsync(db, mazowieckie, "Warsaw", "City", cancellationToken);
            var krakow = await EnsureLocalityAsync(db, malopolskie, "Krakow", "City", cancellationToken);
            var gdansk = await EnsureLocalityAsync(db, pomorskie, "Gdansk", "City", cancellationToken);

            var waw = await EnsureStationAsync(db, country, mazowieckie, warsaw, "WAW", "Warsaw Central", "Warsaw", cancellationToken);
            var krk = await EnsureStationAsync(db, country, malopolskie, krakow, "KRK", "Krakow Glowny", "Krakow", cancellationToken);
            var gdn = await EnsureStationAsync(db, country, pomorskie, gdansk, "GDN", "Gdansk Glowny", "Gdansk", cancellationToken);

            var wawKrk = await EnsureRouteAsync(db, waw, krk, 293m, cancellationToken);
            var krkGdn = await EnsureRouteAsync(db, krk, gdn, 600m, cancellationToken);

            var ic101 = await EnsureTrainAsync(db, "IC 101", "Warsaw", "Krakow", MainTripDepartureUtc, MainTripDepartureUtc.AddHours(3), cancellationToken);
            var ic202 = await EnsureTrainAsync(db, "IC 202", "Krakow", "Gdansk", MainTripDepartureUtc.AddHours(2), MainTripDepartureUtc.AddHours(8), cancellationToken);

            var mainTrip = await EnsureTripAsync(db, ic101, wawKrk, MainTripDepartureUtc, MainTripDepartureUtc.AddHours(3), cancellationToken);
            var secondTrip = await EnsureTripAsync(db, ic202, krkGdn, MainTripDepartureUtc.AddHours(2), MainTripDepartureUtc.AddHours(8), cancellationToken);

            await EnsureFaresAsync(db, mainTrip, cancellationToken);
            await EnsureFaresAsync(db, secondTrip, cancellationToken);
            await EnsureSeatsAsync(db, ic101, cancellationToken);
            await EnsureSeatsAsync(db, ic202, cancellationToken);

            await EnsureUserAsync(db, configuration, "SeedData:AdminPassword", AdminEmail, "Admin", cancellationToken);
            await EnsureUserAsync(db, configuration, "SeedData:PassengerPassword", PassengerEmail, "Passenger", cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task<Country> EnsureCountryAsync(TrainTicketDbContext db, CancellationToken cancellationToken)
        {
            var country = await db.Countries.FirstOrDefaultAsync(c => c.Code == "PL", cancellationToken);
            if (country != null)
                return country;

            country = new Country { Code = "PL", Name = "Poland" };
            db.Countries.Add(country);
            await db.SaveChangesAsync(cancellationToken);
            return country;
        }

        private static async Task<StateRegion> EnsureStateRegionAsync(
            TrainTicketDbContext db,
            Country country,
            string code,
            string name,
            CancellationToken cancellationToken)
        {
            var region = await db.StateRegions
                .FirstOrDefaultAsync(r => r.CountryId == country.Id && r.Code == code, cancellationToken);
            if (region != null)
                return region;

            region = new StateRegion { CountryId = country.Id, Code = code, Name = name };
            db.StateRegions.Add(region);
            await db.SaveChangesAsync(cancellationToken);
            return region;
        }

        private static async Task<Locality> EnsureLocalityAsync(
            TrainTicketDbContext db,
            StateRegion region,
            string name,
            string type,
            CancellationToken cancellationToken)
        {
            var locality = await db.Localities
                .FirstOrDefaultAsync(l =>
                    l.StateRegionId == region.Id &&
                    l.Name == name &&
                    l.Type == type,
                    cancellationToken);
            if (locality != null)
                return locality;

            locality = new Locality { StateRegionId = region.Id, Name = name, Type = type };
            db.Localities.Add(locality);
            await db.SaveChangesAsync(cancellationToken);
            return locality;
        }

        private static async Task<Station> EnsureStationAsync(
            TrainTicketDbContext db,
            Country country,
            StateRegion region,
            Locality locality,
            string code,
            string name,
            string city,
            CancellationToken cancellationToken)
        {
            var station = await db.Stations.FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
            if (station != null)
                return station;

            station = new Station
            {
                Code = code,
                Name = name,
                City = city,
                CountryId = country.Id,
                StateRegionId = region.Id,
                LocalityId = locality.Id
            };
            db.Stations.Add(station);
            await db.SaveChangesAsync(cancellationToken);
            return station;
        }

        private static async Task<TrainRoute> EnsureRouteAsync(
            TrainTicketDbContext db,
            Station departure,
            Station arrival,
            decimal distanceKm,
            CancellationToken cancellationToken)
        {
            var route = await db.TrainRoutes
                .FirstOrDefaultAsync(r =>
                    r.DepartureStationId == departure.Id &&
                    r.ArrivalStationId == arrival.Id,
                    cancellationToken);
            if (route != null)
                return route;

            route = new TrainRoute
            {
                DepartureStationId = departure.Id,
                ArrivalStationId = arrival.Id,
                DistanceKm = distanceKm,
                IsActive = true
            };
            db.TrainRoutes.Add(route);
            await db.SaveChangesAsync(cancellationToken);
            return route;
        }

        private static async Task<Train> EnsureTrainAsync(
            TrainTicketDbContext db,
            string name,
            string departureStation,
            string arrivalStation,
            DateTime departureTime,
            DateTime arrivalTime,
            CancellationToken cancellationToken)
        {
            var train = await db.Trains.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
            if (train != null)
                return train;

            train = new Train
            {
                Name = name,
                DepartureStation = departureStation,
                ArrivalStation = arrivalStation,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime
            };
            db.Trains.Add(train);
            await db.SaveChangesAsync(cancellationToken);
            return train;
        }

        private static async Task<Trip> EnsureTripAsync(
            TrainTicketDbContext db,
            Train train,
            TrainRoute route,
            DateTime departure,
            DateTime arrival,
            CancellationToken cancellationToken)
        {
            var trip = await db.Trips
                .FirstOrDefaultAsync(t =>
                    t.TrainId == train.Id &&
                    t.TrainRouteId == route.Id &&
                    t.DepartureTime == departure,
                    cancellationToken);
            if (trip != null)
                return trip;

            trip = new Trip
            {
                TrainId = train.Id,
                TrainRouteId = route.Id,
                DepartureTime = departure,
                ArrivalTime = arrival,
                Status = "Scheduled"
            };
            db.Trips.Add(trip);
            await db.SaveChangesAsync(cancellationToken);
            return trip;
        }

        private static async Task EnsureFaresAsync(TrainTicketDbContext db, Trip trip, CancellationToken cancellationToken)
        {
            if (await db.Fares.AnyAsync(f => f.TripId == trip.Id, cancellationToken))
                return;

            db.Fares.AddRange(
                new Fare { TripId = trip.Id, ClassType = "Economy", Price = 49.99m, Currency = "USD" },
                new Fare { TripId = trip.Id, ClassType = "Business", Price = 89.99m, Currency = "USD" });
            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureSeatsAsync(TrainTicketDbContext db, Train train, CancellationToken cancellationToken)
        {
            if (await db.Seats.AnyAsync(s => s.TrainId == train.Id, cancellationToken))
                return;

            db.Seats.AddRange(
                new Seat { TrainId = train.Id, Coach = "A", Number = "1", ClassType = "Economy", IsAvailable = true },
                new Seat { TrainId = train.Id, Coach = "A", Number = "2", ClassType = "Economy", IsAvailable = true },
                new Seat { TrainId = train.Id, Coach = "B", Number = "1", ClassType = "Business", IsAvailable = true },
                new Seat { TrainId = train.Id, Coach = "B", Number = "2", ClassType = "Business", IsAvailable = true });
            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureUserAsync(
            TrainTicketDbContext db,
            IConfiguration configuration,
            string passwordConfigKey,
            string email,
            string role,
            CancellationToken cancellationToken)
        {
            if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
                return;

            var password = configuration[passwordConfigKey] ?? DefaultPassword;
            db.Users.Add(new User
            {
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Phone = role == "Admin" ? "000-ADMIN" : "000-PASSENGER",
                Role = role
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
