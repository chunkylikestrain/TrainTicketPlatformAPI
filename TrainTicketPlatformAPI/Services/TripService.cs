using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class TripService : ITripService
    {
        private static readonly TimeSpan MinimumTransferTime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan MaximumTransferTime = TimeSpan.FromHours(3);
        private static readonly TimeSpan NightTravelStart = TimeSpan.FromHours(23);
        private static readonly TimeSpan NightTravelEnd = TimeSpan.FromHours(5);
        private static readonly TimeSpan MaximumNightTransferTime = TimeSpan.FromHours(1);
        private const string SleeperTrainServiceType = "Sleeper train";
        private const int MaximumItinerarySegments = 3;
        private const int MaximumItineraryResults = 60;
        private const int MaximumStartSegmentsPerStarterTrain = 4;
        private const int MaximumItineraryPathsPerStartSegment = 8;
        private const int MaximumItineraryResultsPerStarterTrain = 1;
        private const int MaximumNextSegmentsPerPath = 12;

        private readonly TrainTicketDbContext _db;
        private readonly ILogger<TripService> _logger;
        private readonly IMemoryCache? _cache;

        public TripService(
            TrainTicketDbContext db,
            ILogger<TripService>? logger = null,
            IMemoryCache? cache = null)
        {
            _db = db;
            _logger = logger ?? NullLogger<TripService>.Instance;
            _cache = cache;
        }

        private async Task<List<int>> GetSearchCandidateTripIdsAsync(
            DateTime searchWindowStart,
            DateTime searchWindowEnd,
            DateTime travelDate)
        {
            var operatingDates = new[]
            {
                DateOnly.FromDateTime(travelDate.AddDays(-1)),
                DateOnly.FromDateTime(travelDate),
                DateOnly.FromDateTime(travelDate.AddDays(1))
            };

            return await _db.Trips
                .AsNoTracking()
                .Where(t =>
                    t.TrainRoute.IsActive &&
                    t.DepartureTime < searchWindowEnd &&
                    t.ArrivalTime >= searchWindowStart &&
                    (!t.ExternalOperatingDate.HasValue ||
                        operatingDates.Contains(t.ExternalOperatingDate.Value)))
                .OrderBy(t => t.DepartureTime)
                .Select(t => t.Id)
                .ToListAsync();
        }

        private async Task<ItinerarySearchGraph> PrepareItinerarySearchGraphAsync(IReadOnlyList<int> candidateTripIds)
        {
            var cacheKey = BuildItinerarySearchGraphCacheKey(candidateTripIds);
            if (_cache != null &&
                _cache.TryGetValue(cacheKey, out ItinerarySearchGraph? cachedGraph) &&
                cachedGraph != null)
            {
                _logger.LogInformation(
                    "Itinerary search reused cached graph with {ContextCount} contexts",
                    cachedGraph.Contexts.Count);
                return cachedGraph;
            }

            var trips = await _db.Trips
                .AsNoTracking()
                .AsSplitQuery()
                .Include(t => t.Train)
                    .ThenInclude(train => train.Carriages)
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
                .Where(t => candidateTripIds.Contains(t.Id))
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();

            var contexts = trips
                .Select(BuildItineraryTripContext)
                .Where(context => context.RouteStations.Count > 1)
                .ToList();
            var searchIndex = BuildItinerarySearchIndex(contexts);
            var graph = new ItinerarySearchGraph(contexts, searchIndex);

            _cache?.Set(
                cacheKey,
                graph,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
                    SlidingExpiration = TimeSpan.FromSeconds(45)
                });

            return graph;
        }

        private static string BuildItinerarySearchGraphCacheKey(IReadOnlyList<int> candidateTripIds)
        {
            if (candidateTripIds.Count == 0)
                return "trip-itinerary-search-graph:empty";

            return string.Join(
                ':',
                "trip-itinerary-search-graph",
                candidateTripIds.Count,
                candidateTripIds[0],
                candidateTripIds[^1]);
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
            var searchWindowStart = notBefore ?? travelDate.AddDays(-1);
            var searchWindowEnd = travelDate.AddDays(1);
            var searchStopwatch = Stopwatch.StartNew();
            var stageStopwatch = Stopwatch.StartNew();

            var candidateTripIds = await GetSearchCandidateTripIdsAsync(searchWindowStart, searchWindowEnd, travelDate);
            _logger.LogInformation(
                "Direct trip search found {CandidateTripCount} candidate trip ids for {From} to {To} on {TravelDate} in {ElapsedMilliseconds} ms",
                candidateTripIds.Count,
                from,
                to,
                travelDate,
                stageStopwatch.ElapsedMilliseconds);

            if (candidateTripIds.Count == 0)
                return [];

            stageStopwatch.Restart();
            var trips = await _db.Trips
                .AsNoTracking()
                .AsSplitQuery()
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
                .Where(t => candidateTripIds.Contains(t.Id))
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();
            _logger.LogInformation(
                "Direct trip search loaded {TripCount} trips for {From} to {To} on {TravelDate} in {ElapsedMilliseconds} ms",
                trips.Count,
                from,
                to,
                travelDate,
                stageStopwatch.ElapsedMilliseconds);

            var results = trips
                .Select(trip => TryBuildSearchResult(trip, departure, arrival))
                .Where(result => result != null)
                .Select(result => result!)
                .Where(result => result.DepartureTime.Date == travelDate)
                .Where(result => !notBefore.HasValue || result.DepartureTime >= notBefore.Value)
                .OrderBy(result => result.DepartureTime)
                .ToList();
            _logger.LogInformation(
                "Direct trip search returned {ResultCount} results for {From} to {To} on {TravelDate} in {ElapsedMilliseconds} ms total",
                results.Count,
                from,
                to,
                travelDate,
                searchStopwatch.ElapsedMilliseconds);

            return results;
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
            var searchWindowStart = notBefore ?? travelDate.AddDays(-1);
            var searchWindowEnd = travelDate.AddDays(1);
            var searchStopwatch = Stopwatch.StartNew();
            var stageStopwatch = Stopwatch.StartNew();

            var candidateTripIds = await GetSearchCandidateTripIdsAsync(searchWindowStart, searchWindowEnd, travelDate);
            _logger.LogInformation(
                "Itinerary search found {CandidateTripCount} candidate trip ids for {From} to {To} on {TravelDate} in {ElapsedMilliseconds} ms",
                candidateTripIds.Count,
                from,
                to,
                travelDate,
                stageStopwatch.ElapsedMilliseconds);

            if (candidateTripIds.Count == 0)
                return [];

            stageStopwatch.Restart();
            var searchGraph = await PrepareItinerarySearchGraphAsync(candidateTripIds);
            _logger.LogInformation(
                "Itinerary search prepared graph with {ContextCount} contexts for {From} to {To} on {TravelDate} in {ElapsedMilliseconds} ms",
                searchGraph.Contexts.Count,
                from,
                to,
                travelDate,
                stageStopwatch.ElapsedMilliseconds);

            stageStopwatch.Restart();
            var startSegments = BuildCandidatesFromQuery(searchGraph.Contexts, departure)
                .Where(segment => TripSegmentResolver.StationMatches(segment.DepartureStation, departure))
                .Where(segment => segment.DepartureTime.Date == travelDate)
                .Where(segment => !notBefore.HasValue || segment.DepartureTime >= notBefore.Value)
                .OrderBy(segment => segment.DepartureTime)
                .ThenBy(segment => segment.ArrivalTime)
                .ToList();
            startSegments = LimitStartSegmentVariants(startSegments, arrival).ToList();
            _logger.LogInformation(
                "Itinerary search built {StartSegmentCount} start segments in {ElapsedMilliseconds} ms",
                startSegments.Count,
                stageStopwatch.ElapsedMilliseconds);

            stageStopwatch.Restart();
            var itineraries = new List<List<ItinerarySegmentCandidate>>();

            foreach (var start in startSegments)
            {
                var startItineraries = new List<List<ItinerarySegmentCandidate>>();
                BuildItineraryPaths(start, searchGraph.SearchIndex, arrival, [start], startItineraries);
                itineraries.AddRange(startItineraries.Take(MaximumItineraryPathsPerStartSegment));
            }
            _logger.LogInformation(
                "Itinerary search built {RawItineraryCount} raw itineraries in {ElapsedMilliseconds} ms",
                itineraries.Count,
                stageStopwatch.ElapsedMilliseconds);

            stageStopwatch.Restart();
            var deduplicatedItineraries = DeduplicatePracticalItineraries(
                itineraries.Select(ToItineraryDto));

            var results = LimitStarterTrainVariants(deduplicatedItineraries)
                .Where(PassesOvernightComfortRules)
                .OrderBy(itinerary => itinerary.DepartureTime)
                .ThenBy(itinerary => itinerary.ArrivalTime)
                .ThenBy(itinerary => itinerary.TransferCount)
                .Take(MaximumItineraryResults)
                .ToList();
            _logger.LogInformation(
                "Itinerary search returned {ResultCount} results for {From} to {To} on {TravelDate} in {FinalizeMilliseconds} ms finalize, {TotalMilliseconds} ms total",
                results.Count,
                from,
                to,
                travelDate,
                stageStopwatch.ElapsedMilliseconds,
                searchStopwatch.ElapsedMilliseconds);

            return results;
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

            if (seats.Count == 0)
            {
                await EnsureSeatsForTrainAsync(trip.TrainId, carriages.Values);

                seats = await _db.Seats
                    .AsNoTracking()
                    .Where(s => s.TrainId == trip.TrainId)
                    .OrderBy(s => s.Coach)
                    .ThenBy(s => s.Number)
                    .ToListAsync();
            }

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

        private async Task EnsureSeatsForTrainAsync(
            int trainId,
            IEnumerable<TrainCarriage> carriages)
        {
            if (await _db.Seats.AnyAsync(s => s.TrainId == trainId))
                return;

            var carriageList = carriages
                .Where(c => c.SeatCount > 0)
                .OrderBy(c => c.Position)
                .ToList();

            var seats = new List<Seat>();
            foreach (var carriage in carriageList)
            {
                foreach (var number in GetSeatNumbersForCarriage(carriage))
                {
                    seats.Add(new Seat
                    {
                        TrainId = trainId,
                        Coach = carriage.Coach,
                        Number = number,
                        ClassType = GetSeatClassType(carriage, int.Parse(number)),
                        IsAvailable = true
                    });
                }
            }

            if (seats.Count == 0)
                return;

            _db.Seats.AddRange(seats);
            await _db.SaveChangesAsync();
        }

        private static string GetSeatClassType(TrainCarriage carriage, int seatNumber)
        {
            if (carriage.LayoutType.Equals("InternationalSleeper", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("Sleeper", StringComparison.OrdinalIgnoreCase))
                return "Sleeper";

            if (carriage.LayoutType.Equals("Couchette", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("SixBerthCouchette", StringComparison.OrdinalIgnoreCase))
                return "Couchette";

            if (carriage.ClassType.Equals("Class 1/2", StringComparison.OrdinalIgnoreCase))
                return seatNumber <= Math.Max(1, carriage.SeatCount / 3) ? "Class 1" : "Class 2";

            return carriage.ClassType;
        }

        private static IReadOnlyList<string> GetSeatNumbersForCarriage(TrainCarriage carriage)
        {
            if (carriage.LayoutType.Equals("InternationalSleeper", StringComparison.OrdinalIgnoreCase))
                return InternationalSleeperBerths;

            if (carriage.LayoutType.Equals("Sleeper", StringComparison.OrdinalIgnoreCase))
                return DomesticSleeperBerths;

            if (carriage.LayoutType.Equals("Couchette", StringComparison.OrdinalIgnoreCase))
                return FourBerthCouchetteBerths;

            if (carriage.LayoutType.Equals("SixBerthCouchette", StringComparison.OrdinalIgnoreCase))
                return SixBerthCouchetteBerths;

            return Enumerable.Range(1, carriage.SeatCount)
                .Select(number => number.ToString())
                .ToArray();
        }

        private static readonly string[] InternationalSleeperBerths =
        [
            "11", "13", "15",
            "21", "23", "25",
            "31", "33", "35",
            "41", "45",
            "51", "55",
            "61", "63", "65",
            "71", "73", "75",
            "81", "83", "85"
        ];

        private static readonly string[] DomesticSleeperBerths =
        [
            "11", "13", "15",
            "21", "23", "25",
            "31", "33", "35",
            "41", "43", "45",
            "51", "53", "55",
            "61", "63", "65",
            "71", "73", "75",
            "81", "83", "85",
            "91", "93", "95",
            "101", "103", "105"
        ];

        private static readonly string[] FourBerthCouchetteBerths =
        [
            "11", "15",
            "21", "22", "25", "26",
            "31", "32", "35", "36",
            "41", "42", "45", "46",
            "51", "52", "55", "56",
            "61", "62", "65", "66",
            "71", "72", "75", "76",
            "81", "82", "85", "86"
        ];

        private static readonly string[] SixBerthCouchetteBerths =
        [
            "11", "15",
            "21", "22", "23", "24", "25", "26",
            "31", "32", "33", "34", "35", "36",
            "41", "42", "43", "44", "45", "46",
            "51", "52", "53", "54", "55", "56",
            "61", "62", "63", "64", "65", "66",
            "71", "72", "73", "74", "75", "76",
            "81", "82", "83", "84", "85", "86"
        ];

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
                        callingPattern);
                }
            }
        }

        private static ItineraryTripContext BuildItineraryTripContext(Trip trip)
        {
            var callingPattern = TripTimetablePlanner.Build(trip);
            return new(
                trip,
                TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute),
                callingPattern,
                callingPattern.ToDictionary(stop => stop.StopOrder));
        }

        private static ItinerarySearchIndex BuildItinerarySearchIndex(IReadOnlyList<ItineraryTripContext> contexts)
        {
            var departuresByStationId = new Dictionary<int, List<IndexedDepartureStop>>();

            foreach (var context in contexts)
            {
                for (var departureIndex = 0; departureIndex < context.RouteStations.Count - 1; departureIndex++)
                {
                    var departureStop = context.RouteStations[departureIndex];
                    var plannedDeparture = context.CallingPatternByStopOrder[departureStop.Order];
                    var departureTime = plannedDeparture.DepartureTime ?? plannedDeparture.ArrivalTime ?? context.Trip.DepartureTime;

                    if (!departuresByStationId.TryGetValue(departureStop.StationId, out var stationDepartures))
                    {
                        stationDepartures = [];
                        departuresByStationId[departureStop.StationId] = stationDepartures;
                    }

                    stationDepartures.Add(new IndexedDepartureStop(context, departureIndex, departureTime));
                }
            }

            foreach (var stationDepartures in departuresByStationId.Values)
            {
                stationDepartures.Sort((left, right) =>
                {
                    var departureComparison = left.DepartureTime.CompareTo(right.DepartureTime);
                    if (departureComparison != 0)
                        return departureComparison;

                    return left.Context.Trip.ArrivalTime.CompareTo(right.Context.Trip.ArrivalTime);
                });
            }

            return new ItinerarySearchIndex(contexts, departuresByStationId);
        }

        private static IEnumerable<ItinerarySegmentCandidate> BuildCandidatesFromQuery(
            IReadOnlyList<ItineraryTripContext> contexts,
            string departure)
        {
            foreach (var context in contexts)
            {
                for (var departureIndex = 0; departureIndex < context.RouteStations.Count - 1; departureIndex++)
                {
                    var departureStop = context.RouteStations[departureIndex];
                    if (!TripSegmentResolver.StationMatches(departureStop.Station, departure))
                        continue;

                    for (var arrivalIndex = departureIndex + 1; arrivalIndex < context.RouteStations.Count; arrivalIndex++)
                    {
                        var candidate = BuildSegmentCandidate(context, departureIndex, arrivalIndex);
                        if (candidate != null)
                            yield return candidate;
                    }
                }
            }
        }

        private static IEnumerable<ItinerarySegmentCandidate> BuildCandidatesFromStation(
            ItinerarySearchIndex searchIndex,
            int departureStationId,
            DateTime previousArrival,
            IReadOnlyList<ItinerarySegmentCandidate> path,
            string destination)
        {
            var usedTripIds = path.Select(segment => segment.Trip.Id).ToHashSet();
            var visitedDepartureStationIds = path.Select(segment => segment.DepartureStation.Id).ToHashSet();

            if (!searchIndex.DeparturesByStationId.TryGetValue(departureStationId, out var indexedDepartures))
                yield break;

            foreach (var indexedDeparture in indexedDepartures)
            {
                var context = indexedDeparture.Context;
                if (usedTripIds.Contains(context.Trip.Id))
                    continue;

                var transferTime = indexedDeparture.DepartureTime - previousArrival;
                if (transferTime < MinimumTransferTime)
                    continue;

                if (transferTime > MaximumTransferTime)
                    yield break;

                if (IsNightTransfer(previousArrival, indexedDeparture.DepartureTime) &&
                    transferTime > MaximumNightTransferTime)
                {
                    continue;
                }

                var departureStop = context.RouteStations[indexedDeparture.RouteStationIndex];
                var destinationIndex = FindDestinationStationIndex(context, indexedDeparture.RouteStationIndex, destination);
                if (destinationIndex.HasValue)
                {
                    var destinationCandidate = BuildSegmentCandidate(context, indexedDeparture.RouteStationIndex, destinationIndex.Value);
                    if (destinationCandidate != null)
                        yield return destinationCandidate;

                    continue;
                }

                for (var arrivalIndex = indexedDeparture.RouteStationIndex + 1; arrivalIndex < context.RouteStations.Count; arrivalIndex++)
                {
                    var arrivalStop = context.RouteStations[arrivalIndex];
                    if (visitedDepartureStationIds.Contains(arrivalStop.StationId))
                        continue;

                    if (IsSameLocalityTransfer(departureStop.Station, arrivalStop.Station) &&
                        !TripSegmentResolver.StationMatches(arrivalStop.Station, destination))
                    {
                        continue;
                    }

                    var candidate = BuildSegmentCandidate(context, indexedDeparture.RouteStationIndex, arrivalIndex);
                    if (candidate != null)
                        yield return candidate;
                }
            }
        }

        private static int? FindDestinationStationIndex(
            ItineraryTripContext context,
            int departureIndex,
            string destination)
        {
            for (var arrivalIndex = departureIndex + 1; arrivalIndex < context.RouteStations.Count; arrivalIndex++)
            {
                if (TripSegmentResolver.StationMatches(context.RouteStations[arrivalIndex].Station, destination))
                    return arrivalIndex;
            }

            return null;
        }

        private static ItinerarySegmentCandidate? BuildSegmentCandidate(
            ItineraryTripContext context,
            int departureIndex,
            int arrivalIndex)
        {
            var departureStop = context.RouteStations[departureIndex];
            var arrivalStop = context.RouteStations[arrivalIndex];
            var plannedDeparture = context.CallingPatternByStopOrder[departureStop.Order];
            var plannedArrival = context.CallingPatternByStopOrder[arrivalStop.Order];
            var departureTime = plannedDeparture.DepartureTime ?? plannedDeparture.ArrivalTime ?? context.Trip.DepartureTime;
            var arrivalTime = plannedArrival.ArrivalTime ?? plannedArrival.DepartureTime ?? context.Trip.ArrivalTime;
            if (arrivalTime <= departureTime)
                return null;

            return new ItinerarySegmentCandidate(
                context.Trip,
                departureStop.Station,
                arrivalStop.Station,
                departureStop.Order,
                arrivalStop.Order,
                departureTime,
                arrivalTime,
                context.CallingPattern);
        }

        private static IEnumerable<ItinerarySegmentCandidate> LimitStartSegmentVariants(
            IEnumerable<ItinerarySegmentCandidate> startSegments,
            string destination)
        {
            return startSegments
                .GroupBy(segment => $"{segment.Trip.Id}|{segment.Trip.TrainId}|{segment.DepartureStation.Id}|{segment.DepartureTime:O}")
                .SelectMany(group =>
                {
                    var directSegments = group
                        .Where(segment => TripSegmentResolver.StationMatches(segment.ArrivalStation, destination))
                        .OrderBy(segment => segment.ArrivalTime)
                        .ToList();
                    var directKeys = directSegments
                        .Select(segment => $"{segment.ArrivalStation.Id}|{segment.ArrivalTime:O}")
                        .ToHashSet(StringComparer.Ordinal);
                    if (directSegments.Count > 0)
                        return directSegments;

                    var transferSegments = group
                        .Where(segment => !directKeys.Contains($"{segment.ArrivalStation.Id}|{segment.ArrivalTime:O}"))
                        .OrderBy(segment => GetTransferStationPriority(segment.ArrivalStation))
                        .ThenBy(segment => segment.ArrivalTime)
                        .ThenBy(segment => segment.ArrivalStopOrder)
                        .Take(MaximumStartSegmentsPerStarterTrain);

                    return directSegments.Concat(transferSegments);
                });
        }

        private static int GetTransferStationPriority(Station station)
        {
            var text = TripSegmentResolver.NormalizeSearchText($"{station.Name} {station.City} {station.Code}");
            if (text.Contains("glowny") ||
                text.Contains("central") ||
                text.Contains("wschodni") ||
                text.Contains("zachodni") ||
                text.Contains("warszawa") ||
                text.Contains("krakow") ||
                text.Contains("poznan") ||
                text.Contains("wroclaw") ||
                text.Contains("gdansk") ||
                text.Contains("gdynia") ||
                text.Contains("katowice") ||
                text.Contains("szczecin") ||
                text.Contains("rzeszow") ||
                text.Contains("koszalin"))
            {
                return 0;
            }

            return 1;
        }

        private static void BuildItineraryPaths(
            ItinerarySegmentCandidate current,
            ItinerarySearchIndex searchIndex,
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

            var nextSegments = BuildCandidatesFromStation(
                    searchIndex,
                    current.ArrivalStation.Id,
                    current.ArrivalTime,
                    path,
                    destination)
                .Take(MaximumNextSegmentsPerPath);

            foreach (var next in nextSegments)
            {
                if (results.Count >= MaximumItineraryPathsPerStartSegment)
                    return;

                path.Add(next);
                if (CanContinueOvernightPath(path, destination))
                    BuildItineraryPaths(next, searchIndex, destination, path, results);
                path.RemoveAt(path.Count - 1);
            }
        }

        private static bool CanContinueOvernightPath(
            IReadOnlyList<ItinerarySegmentCandidate> path,
            string destination)
        {
            if (!IsOvernightPath(path))
                return true;

            var transferCount = path.Count - 1;
            if (transferCount > 1)
                return false;

            if (!CandidateNightTransfersAreReasonable(path))
                return false;

            var reachedDestination = TripSegmentResolver.StationMatches(path.Last().ArrivalStation, destination);
            if (reachedDestination)
                return transferCount == 0 || path.Any(IsSleeperSegment);

            return transferCount == 0;
        }

        private static bool IsOvernightPath(IReadOnlyList<ItinerarySegmentCandidate> path)
            => path.Any(segment => CrossesNightWindow(segment.DepartureTime, segment.ArrivalTime));

        private static bool CandidateNightTransfersAreReasonable(IReadOnlyList<ItinerarySegmentCandidate> path)
        {
            for (var index = 0; index < path.Count - 1; index++)
            {
                var transferStart = path[index].ArrivalTime;
                var transferEnd = path[index + 1].DepartureTime;
                var transferDuration = transferEnd - transferStart;

                if (IsNightTransfer(transferStart, transferEnd) &&
                    transferDuration > MaximumNightTransferTime)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsSleeperSegment(ItinerarySegmentCandidate segment)
            => IsSleeperTrain(segment.Trip.Train);

        private static bool IsSameLocalityTransfer(Station departure, Station arrival)
        {
            if (departure.Id == arrival.Id)
                return true;

            if (departure.LocalityId.HasValue &&
                arrival.LocalityId.HasValue &&
                departure.LocalityId.Value == arrival.LocalityId.Value)
            {
                return true;
            }

            var departureCity = TripSegmentResolver.NormalizeSearchText(departure.City);
            var arrivalCity = TripSegmentResolver.NormalizeSearchText(arrival.City);
            return !string.IsNullOrWhiteSpace(departureCity) &&
                departureCity == arrivalCity;
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
                    ServiceType = GetItineraryServiceType(segment.Trip),
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
                    CallingPattern = TripTimetablePlanner.ToDto(
                        segment.PlannedCallingPattern,
                        segment.DepartureStopOrder,
                        segment.ArrivalStopOrder).ToList()
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

        private static IEnumerable<TripItinerarySearchResultDto> DeduplicatePracticalItineraries(
            IEnumerable<TripItinerarySearchResultDto> itineraries)
        {
            return itineraries
                .GroupBy(BuildPracticalItineraryKey)
                .Select(ChooseBestItineraryVariant);
        }

        private static string BuildPracticalItineraryKey(TripItinerarySearchResultDto itinerary)
        {
            var first = itinerary.Segments.First();
            var last = itinerary.Segments.Last();
            var trainSequence = string.Join(">",
                itinerary.Segments.Select(segment => $"{segment.TripId}:{segment.TrainId}:{segment.TrainName}"));

            return string.Join("|",
                first.DepartureStationId,
                last.ArrivalStationId,
                itinerary.DepartureTime.ToString("O"),
                itinerary.ArrivalTime.ToString("O"),
                itinerary.TransferCount,
                trainSequence,
                itinerary.LowestFare?.ToString("0.00") ?? "fare-unavailable",
                itinerary.Currency);
        }

        private static TripItinerarySearchResultDto ChooseBestItineraryVariant(
            IGrouping<string, TripItinerarySearchResultDto> duplicateGroup)
        {
            return duplicateGroup
                .OrderByDescending(itinerary => itinerary.TotalTransferMinutes)
                .ThenBy(itinerary => itinerary.LowestFare ?? decimal.MaxValue)
                .ThenBy(itinerary => itinerary.ItineraryId)
                .First();
        }

        private static IEnumerable<TripItinerarySearchResultDto> LimitStarterTrainVariants(
            IEnumerable<TripItinerarySearchResultDto> itineraries)
        {
            return itineraries
                .GroupBy(BuildStarterTrainVariantKey)
                .SelectMany(group => group
                    .OrderBy(itinerary => itinerary.TotalDurationMinutes)
                    .ThenBy(itinerary => itinerary.TransferCount)
                    .ThenBy(itinerary => itinerary.ArrivalTime)
                    .ThenBy(itinerary => itinerary.LowestFare ?? decimal.MaxValue)
                    .ThenByDescending(itinerary => itinerary.TotalTransferMinutes)
                    .ThenBy(itinerary => itinerary.ItineraryId)
                    .Take(MaximumItineraryResultsPerStarterTrain));
        }

        private static string BuildStarterTrainVariantKey(TripItinerarySearchResultDto itinerary)
        {
            var first = itinerary.Segments.First();
            var last = itinerary.Segments.Last();

            return string.Join("|",
                first.DepartureStationId,
                last.ArrivalStationId,
                first.TripId,
                first.TrainId,
                first.DepartureTime.ToString("O"));
        }

        private static bool PassesOvernightComfortRules(TripItinerarySearchResultDto itinerary)
        {
            var segments = itinerary.Segments.ToList();
            if (!IsOvernightItinerary(segments))
                return true;

            if (itinerary.TransferCount == 0)
                return true;

            if (itinerary.TransferCount > 1)
                return false;

            if (!segments.Any(IsSleeperSegment))
                return false;

            return NightTransfersAreReasonable(segments);
        }

        private static bool IsOvernightItinerary(IReadOnlyList<TripItinerarySegmentDto> segments)
            => segments.Any(segment => CrossesNightWindow(segment.DepartureTime, segment.ArrivalTime));

        private static bool CrossesNightWindow(DateTime start, DateTime end)
        {
            for (var cursor = start; cursor < end; cursor = cursor.AddMinutes(30))
            {
                if (IsNightTime(cursor))
                    return true;
            }

            return IsNightTime(end);
        }

        private static bool NightTransfersAreReasonable(IReadOnlyList<TripItinerarySegmentDto> segments)
        {
            for (var index = 0; index < segments.Count - 1; index++)
            {
                var transferStart = segments[index].ArrivalTime;
                var transferEnd = segments[index + 1].DepartureTime;
                var transferDuration = transferEnd - transferStart;

                if (IsNightTransfer(transferStart, transferEnd) &&
                    transferDuration > MaximumNightTransferTime)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsNightTransfer(DateTime arrivalTime, DateTime departureTime)
            => CrossesNightWindow(arrivalTime, departureTime);

        private static bool IsNightTime(DateTime time)
        {
            var value = time.TimeOfDay;
            return value >= NightTravelStart || value < NightTravelEnd;
        }

        private static bool IsSleeperSegment(TripItinerarySegmentDto segment)
            => string.Equals(segment.ServiceType, SleeperTrainServiceType, StringComparison.OrdinalIgnoreCase);

        private static string GetItineraryServiceType(Trip trip)
            => IsSleeperTrain(trip.Train) ? SleeperTrainServiceType : string.Empty;

        private static bool IsSleeperTrain(Train train)
            => train.Carriages.Any(carriage =>
                carriage.LayoutType.Equals("InternationalSleeper", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("Sleeper", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("Couchette", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("SixBerthCouchette", StringComparison.OrdinalIgnoreCase));

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
            IReadOnlyList<PlannedTripStop> PlannedCallingPattern)
        {
            public Fare? LowestFare => Trip.Fares.OrderBy(fare => fare.Price).FirstOrDefault();
        }

        private sealed record ItineraryTripContext(
            Trip Trip,
            IReadOnlyList<TripSegmentResolver.RouteSearchStop> RouteStations,
            IReadOnlyList<PlannedTripStop> CallingPattern,
            IReadOnlyDictionary<int, PlannedTripStop> CallingPatternByStopOrder);

        private sealed record IndexedDepartureStop(
            ItineraryTripContext Context,
            int RouteStationIndex,
            DateTime DepartureTime);

        private sealed record ItinerarySearchIndex(
            IReadOnlyList<ItineraryTripContext> Contexts,
            IReadOnlyDictionary<int, List<IndexedDepartureStop>> DeparturesByStationId);

        private sealed record ItinerarySearchGraph(
            IReadOnlyList<ItineraryTripContext> Contexts,
            ItinerarySearchIndex SearchIndex);
    }
}
