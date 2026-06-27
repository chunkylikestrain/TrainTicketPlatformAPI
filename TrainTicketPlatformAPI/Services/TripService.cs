using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class TripService : ITripService
    {
        private static readonly TimeSpan MinimumTransferTime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan MaximumTransferTime = TimeSpan.FromHours(3);
        private const int MaximumItinerarySegments = 3;
        private const int MaximumItineraryResults = 60;

        private readonly TrainTicketDbContext _db;

        public TripService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<TripSearchResultDto>> SearchTripsAsync(
            string from,
            string to,
            DateTime date,
            TimeSpan? time = null)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                throw new InvalidOperationException("Both departure and arrival stations are required");

            var departure = TripSegmentResolver.NormalizeSearchText(from);
            var arrival = TripSegmentResolver.NormalizeSearchText(to);
            var travelDate = date.Date;
            var notBefore = BuildNotBefore(travelDate, time);

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
                .Where(result => !notBefore.HasValue || result.DepartureTime >= notBefore.Value)
                .OrderBy(result => result.DepartureTime);
        }

        public async Task<IEnumerable<TripItinerarySearchResultDto>> SearchItinerariesAsync(
            string from,
            string to,
            DateTime date,
            TimeSpan? time = null)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                throw new InvalidOperationException("Both departure and arrival stations are required");

            var departure = TripSegmentResolver.NormalizeSearchText(from);
            var arrival = TripSegmentResolver.NormalizeSearchText(to);
            var travelDate = date.Date;
            var notBefore = BuildNotBefore(travelDate, time);

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

            var candidates = trips
                .SelectMany(BuildSegmentCandidates)
                .OrderBy(segment => segment.DepartureTime)
                .ThenBy(segment => segment.ArrivalTime)
                .ToList();

            var startSegments = candidates
                .Where(segment => TripSegmentResolver.StationMatches(segment.DepartureStation, departure))
                .Where(segment => !notBefore.HasValue || segment.DepartureTime >= notBefore.Value)
                .ToList();
            var itineraries = new List<List<ItinerarySegmentCandidate>>();

            foreach (var start in startSegments)
                BuildItineraryPaths(start, candidates, arrival, [start], itineraries);

            return itineraries
                .Select(ToItineraryDto)
                .GroupBy(itinerary => itinerary.ItineraryId)
                .Select(group => group.First())
                .OrderBy(itinerary => itinerary.DepartureTime)
                .ThenBy(itinerary => itinerary.ArrivalTime)
                .ThenBy(itinerary => itinerary.TransferCount)
                .Take(MaximumItineraryResults)
                .ToList();
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
            var callingPattern = TripTimetablePlanner.Build(trip);

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
                Platform = trip.Platform,
                Track = trip.Track,
                Status = trip.Status,
                DelayMinutes = trip.DelayMinutes,
                CancellationReason = trip.CancellationReason,
                OriginalPlatform = trip.OriginalPlatform,
                OriginalTrack = trip.OriginalTrack,
                DisruptionMessage = GetDisruptionMessage(trip),
                DisruptionSeverity = GetDisruptionSeverity(trip),
                HasPlatformChange = HasPlatformChange(trip),
                HasDisruption = HasDisruption(trip),
                LowestFare = lowestFare?.Price,
                Currency = lowestFare?.Currency ?? string.Empty,
                CallingPattern = TripTimetablePlanner.ToDto(callingPattern)
            };
        }

        private static TripSearchResultDto? TryBuildSearchResult(Trip trip, string departure, string arrival)
        {
            var routeStations = TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute);
            var callingPattern = TripTimetablePlanner.Build(trip);
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
            var plannedDeparture = callingPattern.Single(stop => stop.StopOrder == departureStop.Order);
            var plannedArrival = callingPattern.Single(stop => stop.StopOrder == arrivalStop.Order);
            result.DepartureTime = plannedDeparture.DepartureTime ?? plannedDeparture.ArrivalTime ?? trip.DepartureTime;
            result.ArrivalTime = plannedArrival.ArrivalTime ?? plannedArrival.DepartureTime ?? trip.ArrivalTime;
            result.CallingPattern = TripTimetablePlanner.ToDto(
                callingPattern,
                departureStop.Order,
                arrivalStop.Order);
            return result;
        }

        private static IEnumerable<ItinerarySegmentCandidate> BuildSegmentCandidates(Trip trip)
        {
            var routeStations = TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute);
            var callingPattern = TripTimetablePlanner.Build(trip);

            for (var departureIndex = 0; departureIndex < routeStations.Count - 1; departureIndex++)
            {
                var departureStop = routeStations[departureIndex];
                var plannedDeparture = callingPattern.Single(stop => stop.StopOrder == departureStop.Order);
                var departureTime = plannedDeparture.DepartureTime ?? plannedDeparture.ArrivalTime ?? trip.DepartureTime;

                for (var arrivalIndex = departureIndex + 1; arrivalIndex < routeStations.Count; arrivalIndex++)
                {
                    var arrivalStop = routeStations[arrivalIndex];
                    var plannedArrival = callingPattern.Single(stop => stop.StopOrder == arrivalStop.Order);
                    var arrivalTime = plannedArrival.ArrivalTime ?? plannedArrival.DepartureTime ?? trip.ArrivalTime;
                    if (arrivalTime <= departureTime)
                        continue;

                    yield return new ItinerarySegmentCandidate(
                        trip,
                        departureStop.Station,
                        arrivalStop.Station,
                        departureStop.Order,
                        arrivalStop.Order,
                        departureTime,
                        arrivalTime,
                        TripTimetablePlanner.ToDto(callingPattern, departureStop.Order, arrivalStop.Order).ToList());
                }
            }
        }

        private static void BuildItineraryPaths(
            ItinerarySegmentCandidate current,
            IReadOnlyList<ItinerarySegmentCandidate> candidates,
            string destination,
            List<ItinerarySegmentCandidate> path,
            List<List<ItinerarySegmentCandidate>> results)
        {
            if (TripSegmentResolver.StationMatches(current.ArrivalStation, destination))
            {
                results.Add([.. path]);
                return;
            }

            if (path.Count >= MaximumItinerarySegments)
                return;

            var nextSegments = candidates.Where(next =>
                next.DepartureStation.Id == current.ArrivalStation.Id &&
                !path.Any(segment => segment.Trip.Id == next.Trip.Id) &&
                !path.Any(segment => segment.DepartureStation.Id == next.ArrivalStation.Id) &&
                next.DepartureTime - current.ArrivalTime >= MinimumTransferTime &&
                next.DepartureTime - current.ArrivalTime <= MaximumTransferTime);

            foreach (var next in nextSegments)
            {
                path.Add(next);
                BuildItineraryPaths(next, candidates, destination, path, results);
                path.RemoveAt(path.Count - 1);
            }
        }

        private static DateTime? BuildNotBefore(DateTime travelDate, TimeSpan? time)
        {
            if (!time.HasValue)
                return null;

            return travelDate.Date.Add(time.Value);
        }

        private static TripItinerarySearchResultDto ToItineraryDto(IReadOnlyList<ItinerarySegmentCandidate> path)
        {
            var first = path.First();
            var last = path.Last();
            var fare = path
                .Select(segment => segment.LowestFare)
                .Where(value => value != null)
                .Sum(value => value!.Price);
            var currency = path.Select(segment => segment.LowestFare?.Currency)
                .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

            var segments = path.Select((segment, index) =>
            {
                var transferAfterMinutes = index < path.Count - 1
                    ? (int)Math.Round((path[index + 1].DepartureTime - segment.ArrivalTime).TotalMinutes)
                    : 0;

                return new TripItinerarySegmentDto
                {
                    SegmentIndex = index,
                    TripId = segment.Trip.Id,
                    TrainId = segment.Trip.TrainId,
                    TrainName = segment.Trip.Train.Name,
                    DepartureStationId = segment.DepartureStation.Id,
                    DepartureStationCode = segment.DepartureStation.Code,
                    DepartureStationName = segment.DepartureStation.Name,
                    ArrivalStationId = segment.ArrivalStation.Id,
                    ArrivalStationCode = segment.ArrivalStation.Code,
                    ArrivalStationName = segment.ArrivalStation.Name,
                    DepartureStopOrder = segment.DepartureStopOrder,
                    ArrivalStopOrder = segment.ArrivalStopOrder,
                    DepartureTime = segment.DepartureTime,
                    ArrivalTime = segment.ArrivalTime,
                    DurationMinutes = (int)Math.Round((segment.ArrivalTime - segment.DepartureTime).TotalMinutes),
                    TransferAfterMinutes = transferAfterMinutes,
                    Platform = segment.Trip.Platform,
                    Track = segment.Trip.Track,
                    Status = segment.Trip.Status,
                    DelayMinutes = segment.Trip.DelayMinutes,
                    HasDisruption = HasDisruption(segment.Trip),
                    LowestFare = segment.LowestFare?.Price,
                    Currency = segment.LowestFare?.Currency ?? string.Empty,
                    CallingPattern = segment.CallingPattern
                };
            }).ToList();

            return new TripItinerarySearchResultDto
            {
                ItineraryId = string.Join("_", path.Select(segment =>
                    $"{segment.Trip.Id}-{segment.DepartureStation.Id}-{segment.ArrivalStation.Id}")),
                TransferCount = path.Count - 1,
                DepartureTime = first.DepartureTime,
                ArrivalTime = last.ArrivalTime,
                TotalDurationMinutes = (int)Math.Round((last.ArrivalTime - first.DepartureTime).TotalMinutes),
                TotalTransferMinutes = segments.Sum(segment => segment.TransferAfterMinutes),
                LowestFare = path.All(segment => segment.LowestFare != null) ? fare : null,
                Currency = currency,
                Segments = segments
            };
        }

        private static TripDetailsDto ToDetails(Trip trip)
        {
            var routeStops = TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute);
            var callingPattern = TripTimetablePlanner.Build(trip);

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
                Platform = trip.Platform,
                Track = trip.Track,
                Status = trip.Status,
                DelayMinutes = trip.DelayMinutes,
                CancellationReason = trip.CancellationReason,
                OriginalPlatform = trip.OriginalPlatform,
                OriginalTrack = trip.OriginalTrack,
                DisruptionMessage = GetDisruptionMessage(trip),
                DisruptionSeverity = GetDisruptionSeverity(trip),
                HasPlatformChange = HasPlatformChange(trip),
                HasDisruption = HasDisruption(trip),
                Fares = trip.Fares
                    .OrderBy(f => f.Price)
                    .Select(f => new FareDto
                    {
                        ClassType = f.ClassType,
                        Price = f.Price,
                        Currency = f.Currency
                    })
                    .ToList(),
                CallingPattern = TripTimetablePlanner.ToDto(callingPattern)
            };
        }

        private static bool HasPlatformChange(Trip trip)
        {
            var originalPlatform = string.IsNullOrWhiteSpace(trip.OriginalPlatform)
                ? trip.Platform
                : trip.OriginalPlatform;
            var originalTrack = string.IsNullOrWhiteSpace(trip.OriginalTrack)
                ? trip.Track
                : trip.OriginalTrack;

            return !string.Equals(originalPlatform, trip.Platform, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(originalTrack, trip.Track, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasDisruption(Trip trip)
        {
            return trip.DelayMinutes > 0 ||
                HasPlatformChange(trip) ||
                !string.Equals(trip.Status, "Scheduled", StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(trip.CancellationReason) ||
                !string.IsNullOrWhiteSpace(trip.DisruptionMessage);
        }

        private static string GetDisruptionSeverity(Trip trip)
        {
            if (!string.IsNullOrWhiteSpace(trip.DisruptionSeverity))
                return trip.DisruptionSeverity;

            if (string.Equals(trip.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                return "Critical";

            if (trip.DelayMinutes >= 30)
                return "Major";

            return HasDisruption(trip) ? "Notice" : string.Empty;
        }

        private static string GetDisruptionMessage(Trip trip)
        {
            if (!string.IsNullOrWhiteSpace(trip.DisruptionMessage))
                return trip.DisruptionMessage;

            if (string.Equals(trip.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(trip.CancellationReason)
                    ? "This train has been cancelled."
                    : $"This train has been cancelled: {trip.CancellationReason}";
            }

            if (trip.DelayMinutes > 0)
                return $"This train is delayed by {trip.DelayMinutes} minutes.";

            if (HasPlatformChange(trip))
            {
                var platform = string.IsNullOrWhiteSpace(trip.Platform) ? "-" : trip.Platform;
                var track = string.IsNullOrWhiteSpace(trip.Track) ? "-" : trip.Track;
                return $"Platform changed. Please use platform {platform}, track {track}.";
            }

            return string.Empty;
        }

        private sealed record ItinerarySegmentCandidate(
            Trip Trip,
            Station DepartureStation,
            Station ArrivalStation,
            int DepartureStopOrder,
            int ArrivalStopOrder,
            DateTime DepartureTime,
            DateTime ArrivalTime,
            IReadOnlyList<TripCallingPatternStopDto> CallingPattern)
        {
            public Fare? LowestFare => Trip.Fares.OrderBy(fare => fare.Price).FirstOrDefault();
        }
    }
}
