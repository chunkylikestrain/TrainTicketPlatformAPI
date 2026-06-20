using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class TripService : ITripService
    {
        private readonly TrainTicketDbContext _db;

        public TripService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<TripSearchResultDto>> SearchTripsAsync(
            string from,
            string to,
            DateTime date)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                throw new InvalidOperationException("Both departure and arrival stations are required");

            var departure = NormalizeSearchText(from);
            var arrival = NormalizeSearchText(to);
            var travelDate = date.Date;

            var trips = await _db.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.DepartureStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.ArrivalStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.RouteStops)
                        .ThenInclude(s => s.Station)
                            .ThenInclude(s => s.Locality)
                .Include(t => t.Fares)
                .Where(t =>
                    t.TrainRoute.IsActive &&
                    t.DepartureTime.Date == travelDate)
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();

            return trips
                .Select(trip => TryBuildSearchResult(trip, departure, arrival))
                .Where(result => result != null)
                .Select(result => result!)
                .OrderBy(result => result.DepartureTime);
        }

        public async Task<TripDetailsDto> GetTripByIdAsync(int tripId)
        {
            var trip = await _db.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.DepartureStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.ArrivalStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.Fares)
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException("Trip not found");

            return ToDetails(trip);
        }

        public async Task<IEnumerable<TripSeatAvailabilityDto>> GetSeatAvailabilityAsync(int tripId)
        {
            var trip = await _db.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException("Trip not found");

            var now = DateTime.UtcNow;
            var bookedSeatIds = await _db.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.TripId == tripId &&
                    b.TravelDate.Date == trip.DepartureTime.Date &&
                    !b.IsCancelled &&
                    (b.BookingStatus == "Confirmed" ||
                     (b.BookingStatus == "PendingPayment" &&
                      (!b.ExpiresAtUtc.HasValue ||
                       b.ExpiresAtUtc.Value > now))))
                .Select(b => b.SeatId)
                .ToListAsync();

            return await _db.Seats
                .AsNoTracking()
                .Where(s => s.TrainId == trip.TrainId)
                .OrderBy(s => s.Coach)
                .ThenBy(s => s.Number)
                .Select(s => new TripSeatAvailabilityDto
                {
                    SeatId = s.Id,
                    Coach = s.Coach,
                    Number = s.Number,
                    ClassType = s.ClassType,
                    IsAvailable = s.IsAvailable && !bookedSeatIds.Contains(s.Id)
                })
                .ToListAsync();
        }

        private static TripSearchResultDto ToSearchResult(Trip trip)
        {
            var lowestFare = trip.Fares.OrderBy(f => f.Price).FirstOrDefault();

            return new TripSearchResultDto
            {
                TripId = trip.Id,
                TrainId = trip.TrainId,
                TrainName = trip.Train.Name,
                DepartureStationCode = trip.TrainRoute.DepartureStation.Code,
                DepartureStationName = trip.TrainRoute.DepartureStation.Name,
                ArrivalStationCode = trip.TrainRoute.ArrivalStation.Code,
                ArrivalStationName = trip.TrainRoute.ArrivalStation.Name,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                Status = trip.Status,
                LowestFare = lowestFare?.Price,
                Currency = lowestFare?.Currency ?? string.Empty
            };
        }

        private static TripSearchResultDto? TryBuildSearchResult(Trip trip, string departure, string arrival)
        {
            var routeStations = BuildOrderedRouteStations(trip.TrainRoute);
            var departureStop = routeStations.FirstOrDefault(stop => StationMatches(stop.Station, departure));
            var arrivalStop = routeStations.FirstOrDefault(stop => StationMatches(stop.Station, arrival));

            if (departureStop == null || arrivalStop == null || departureStop.Order >= arrivalStop.Order)
                return null;

            var result = ToSearchResult(trip);
            result.DepartureStationCode = departureStop.Station.Code;
            result.DepartureStationName = departureStop.Station.Name;
            result.ArrivalStationCode = arrivalStop.Station.Code;
            result.ArrivalStationName = arrivalStop.Station.Name;
            result.DepartureTime = EstimateStopTime(trip, departureStop.Order, routeStations.Count);
            result.ArrivalTime = EstimateStopTime(trip, arrivalStop.Order, routeStations.Count);
            return result;
        }

        private static List<RouteSearchStop> BuildOrderedRouteStations(TrainRoute route)
        {
            var stops = new List<RouteSearchStop>
            {
                new(0, route.DepartureStation)
            };

            stops.AddRange(route.RouteStops
                .OrderBy(stop => stop.StopOrder)
                .Select((stop, index) => new RouteSearchStop(index + 1, stop.Station)));

            stops.Add(new RouteSearchStop(stops.Count, route.ArrivalStation));
            return stops;
        }

        private static DateTime EstimateStopTime(Trip trip, int stopOrder, int stopCount)
        {
            if (stopOrder <= 0)
                return trip.DepartureTime;

            if (stopOrder >= stopCount - 1)
                return trip.ArrivalTime;

            var totalStopsBetweenTermini = Math.Max(1, stopCount - 1);
            var tripMinutes = (trip.ArrivalTime - trip.DepartureTime).TotalMinutes;
            var minutesFromOrigin = tripMinutes * stopOrder / totalStopsBetweenTermini;
            return trip.DepartureTime.AddMinutes(minutesFromOrigin);
        }

        private static bool StationMatches(Station station, string query)
        {
            return NormalizeSearchText(station.Code) == query ||
                NormalizeSearchText(station.Name) == query ||
                NormalizeSearchText(station.City) == query ||
                (station.Locality != null &&
                 NormalizeSearchText(station.Locality.Name) == query);
        }

        private static string NormalizeSearchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;

                builder.Append(character switch
                {
                    'Ł' or 'ł' => 'l',
                    _ => char.ToLowerInvariant(character)
                });
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private sealed record RouteSearchStop(int Order, Station Station);

        private static TripDetailsDto ToDetails(Trip trip)
        {
            return new TripDetailsDto
            {
                TripId = trip.Id,
                TrainId = trip.TrainId,
                TrainName = trip.Train.Name,
                DepartureStationCode = trip.TrainRoute.DepartureStation.Code,
                DepartureStationName = trip.TrainRoute.DepartureStation.Name,
                ArrivalStationCode = trip.TrainRoute.ArrivalStation.Code,
                ArrivalStationName = trip.TrainRoute.ArrivalStation.Name,
                DistanceKm = trip.TrainRoute.DistanceKm,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                Status = trip.Status,
                Fares = trip.Fares
                    .OrderBy(f => f.Price)
                    .Select(f => new FareDto
                    {
                        ClassType = f.ClassType,
                        Price = f.Price,
                        Currency = f.Currency
                    })
                    .ToList()
            };
        }
    }
}
