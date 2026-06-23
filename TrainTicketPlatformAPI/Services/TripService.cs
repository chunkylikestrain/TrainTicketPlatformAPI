using Microsoft.EntityFrameworkCore;
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

            var departure = TripSegmentResolver.NormalizeSearchText(from);
            var arrival = TripSegmentResolver.NormalizeSearchText(to);
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
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.RouteStops)
                        .ThenInclude(s => s.Station)
                .Include(t => t.Fares)
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException("Trip not found");

            return ToDetails(trip);
        }

        public async Task<IEnumerable<TripSeatAvailabilityDto>> GetSeatAvailabilityAsync(
            int tripId,
            int? segmentDepartureStationId = null,
            int? segmentArrivalStationId = null)
        {
            var trip = await _db.Trips
                .AsNoTracking()
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.DepartureStation)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.ArrivalStation)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.RouteStops)
                        .ThenInclude(s => s.Station)
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException("Trip not found");

            var requestedSegment = TripSegmentResolver.Resolve(
                trip,
                segmentDepartureStationId,
                segmentArrivalStationId);
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
                       b.ExpiresAtUtc.Value > now))) &&
                    (b.SegmentDepartureOrder ?? 0) < requestedSegment.ArrivalOrder &&
                    requestedSegment.DepartureOrder < (b.SegmentArrivalOrder ?? int.MaxValue))
                .Select(b => b.SeatId)
                .ToListAsync();

            var carriages = await _db.TrainCarriages
                .AsNoTracking()
                .Where(c => c.TrainId == trip.TrainId)
                .ToDictionaryAsync(c => c.Coach, StringComparer.OrdinalIgnoreCase, cancellationToken: default);

            var seats = await _db.Seats
                .AsNoTracking()
                .Where(s => s.TrainId == trip.TrainId)
                .OrderBy(s => s.Coach)
                .ThenBy(s => s.Number)
                .ToListAsync();

            return seats
                .OrderBy(s => GetCarriagePosition(s.Coach, carriages))
                .ThenBy(s => ParseSeatNumber(s.Number))
                .ThenBy(s => s.Number)
                .Select(s =>
                {
                    var carriage = carriages.GetValueOrDefault(s.Coach);
                    var fallbackLayout = GetFallbackLayout(s.ClassType, s.Coach);

                    return new TripSeatAvailabilityDto
                    {
                        SeatId = s.Id,
                        Coach = s.Coach,
                        Number = s.Number,
                        ClassType = s.ClassType,
                        IsAvailable = s.IsAvailable && !bookedSeatIds.Contains(s.Id),
                        CarriagePosition = carriage?.Position ?? GetFallbackCoachNumber(s.Coach),
                        CarriageClass = carriage?.ClassType ?? s.ClassType,
                        LayoutType = string.IsNullOrWhiteSpace(carriage?.LayoutType) ? fallbackLayout : carriage.LayoutType,
                        VehicleType = carriage?.VehicleType ?? string.Empty,
                        HasBikeSpace = carriage?.HasBikeSpace ?? false,
                        HasAccessibleSpace = carriage?.HasAccessibleSpace ?? false,
                        HasFamilyCompartment = carriage?.HasFamilyCompartment ?? false,
                        HasDiningSection = carriage?.HasDiningSection ?? false,
                        CarriageNotes = carriage?.Notes ?? string.Empty
                    };
                })
                .ToList();
        }

        private static int GetCarriagePosition(string coach, IReadOnlyDictionary<string, TrainCarriage> carriages)
        {
            if (carriages.TryGetValue(coach, out var carriage))
                return carriage.Position;

            return GetFallbackCoachNumber(coach);
        }

        private static int GetFallbackCoachNumber(string value)
        {
            return int.TryParse(value, out var number) ? number : int.MaxValue;
        }

        private static int ParseSeatNumber(string value)
        {
            return int.TryParse(value, out var number) ? number : int.MaxValue;
        }

        private static string GetFallbackLayout(string classType, string coach)
        {
            if (classType.Contains("1", StringComparison.OrdinalIgnoreCase))
                return "FirstCompartment";

            var coachNumber = GetFallbackCoachNumber(coach);
            return coachNumber == 2 ? "ComboAccessible" : "OpenSecond";
        }

        private static TripSearchResultDto ToSearchResult(Trip trip)
        {
            var lowestFare = trip.Fares.OrderBy(f => f.Price).FirstOrDefault();
            var routeStops = TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute);

            return new TripSearchResultDto
            {
                TripId = trip.Id,
                TrainId = trip.TrainId,
                TrainName = trip.Train.Name,
                DepartureStationId = trip.TrainRoute.DepartureStationId,
                DepartureStationCode = trip.TrainRoute.DepartureStation.Code,
                DepartureStationName = trip.TrainRoute.DepartureStation.Name,
                ArrivalStationId = trip.TrainRoute.ArrivalStationId,
                ArrivalStationCode = trip.TrainRoute.ArrivalStation.Code,
                ArrivalStationName = trip.TrainRoute.ArrivalStation.Name,
                DepartureStopOrder = 0,
                ArrivalStopOrder = routeStops.Count - 1,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                Status = trip.Status,
                LowestFare = lowestFare?.Price,
                Currency = lowestFare?.Currency ?? string.Empty
            };
        }

        private static TripSearchResultDto? TryBuildSearchResult(Trip trip, string departure, string arrival)
        {
            var routeStations = TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute);
            var departureStop = routeStations.FirstOrDefault(stop => TripSegmentResolver.StationMatches(stop.Station, departure));
            var arrivalStop = routeStations.FirstOrDefault(stop => TripSegmentResolver.StationMatches(stop.Station, arrival));

            if (departureStop == null || arrivalStop == null || departureStop.Order >= arrivalStop.Order)
                return null;

            var result = ToSearchResult(trip);
            result.DepartureStationId = departureStop.StationId;
            result.DepartureStationCode = departureStop.Station.Code;
            result.DepartureStationName = departureStop.Station.Name;
            result.ArrivalStationId = arrivalStop.StationId;
            result.ArrivalStationCode = arrivalStop.Station.Code;
            result.ArrivalStationName = arrivalStop.Station.Name;
            result.DepartureStopOrder = departureStop.Order;
            result.ArrivalStopOrder = arrivalStop.Order;
            result.DepartureTime = TripSegmentResolver.EstimateStopTime(trip, departureStop.Order, routeStations.Count);
            result.ArrivalTime = TripSegmentResolver.EstimateStopTime(trip, arrivalStop.Order, routeStations.Count);
            return result;
        }

        private static TripDetailsDto ToDetails(Trip trip)
        {
            var routeStops = TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute);

            return new TripDetailsDto
            {
                TripId = trip.Id,
                TrainId = trip.TrainId,
                TrainName = trip.Train.Name,
                DepartureStationId = trip.TrainRoute.DepartureStationId,
                DepartureStationCode = trip.TrainRoute.DepartureStation.Code,
                DepartureStationName = trip.TrainRoute.DepartureStation.Name,
                ArrivalStationId = trip.TrainRoute.ArrivalStationId,
                ArrivalStationCode = trip.TrainRoute.ArrivalStation.Code,
                ArrivalStationName = trip.TrainRoute.ArrivalStation.Name,
                DepartureStopOrder = 0,
                ArrivalStopOrder = routeStops.Count - 1,
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
