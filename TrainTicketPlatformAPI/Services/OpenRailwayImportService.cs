using Microsoft.Extensions.Options;
using TrainTicketPlatformAPI.Contracts.OpenRailway;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class OpenRailwayImportService : IOpenRailwayImportService
    {
        private readonly IOpenRailwayClient _client;
        private readonly TrainTicketDbContext _db;
        private readonly OpenRailwayOptions _options;

        public OpenRailwayImportService(
            IOpenRailwayClient client,
            TrainTicketDbContext db,
            IOptions<OpenRailwayOptions> options)
        {
            _client = client;
            _db = db;
            _options = options.Value;
        }

        public async Task<OpenRailwayImportPreviewDto> PreviewRouteAsync(
            int scheduleId,
            int orderId,
            CancellationToken cancellationToken)
        {
            var route = await _client.GetRouteAsync(scheduleId, orderId, cancellationToken);
            var trainNumber = FirstNonBlank(
                route.NationalNumber,
                route.InternationalDepartureNumber,
                route.InternationalArrivalNumber,
                route.OrderId.ToString());

            return new OpenRailwayImportPreviewDto
            {
                ExternalSource = _options.ExternalSource,
                ScheduleId = route.ScheduleId,
                OrderId = route.OrderId,
                TrainOrderId = route.TrainOrderId,
                TrainCode = string.IsNullOrWhiteSpace(route.CommercialCategorySymbol)
                    ? trainNumber
                    : $"{route.CommercialCategorySymbol}-{trainNumber}",
                TrainName = string.IsNullOrWhiteSpace(route.Name)
                    ? trainNumber
                    : $"{route.CommercialCategorySymbol} {trainNumber} {route.Name}".Trim(),
                CarrierCode = route.CarrierCode ?? string.Empty,
                Category = route.CommercialCategorySymbol ?? string.Empty,
                OperatingDates = route.OperatingDates ?? [],
                Stops = (route.Stations ?? [])
                    .OrderBy(stop => stop.OrderNumber)
                    .Select(stop => new OpenRailwayImportStopPreviewDto
                    {
                        ExternalStationId = stop.StationId,
                        OrderNumber = stop.OrderNumber,
                        Arrival = FormatStopTime(stop.ArrivalDay, stop.ArrivalTime),
                        Departure = FormatStopTime(stop.DepartureDay, stop.DepartureTime),
                        Platform = FirstNonBlank(stop.DeparturePlatform, stop.ArrivalPlatform),
                        Track = FirstNonBlank(stop.DepartureTrack, stop.ArrivalTrack),
                        StopTypeId = stop.StopTypeId,
                        StopTypeName = stop.StopTypeName ?? string.Empty
                    })
                    .ToList()
            };
        }

        public async Task<OpenRailwayImportRouteResultDto> ImportRouteAsync(
            int scheduleId,
            int orderId,
            DateOnly? operatingDate,
            CancellationToken cancellationToken)
        {
            var route = await _client.GetRouteAsync(scheduleId, orderId, cancellationToken);
            var orderedStops = (route.Stations ?? [])
                .OrderBy(stop => stop.OrderNumber)
                .ToList();

            if (orderedStops.Count < 2)
                throw new InvalidOperationException("Open Railway route import needs at least two stops.");

            var selectedOperatingDate = ResolveOperatingDate(route, operatingDate);
            var stationNames = await ResolveStationNamesAsync(
                orderedStops.Select(stop => stop.StationId).Distinct().ToArray(),
                cancellationToken);

            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var stationMap = new Dictionary<int, Station>();
            var stationsCreated = 0;
            foreach (var externalStationId in orderedStops.Select(stop => stop.StationId).Distinct())
            {
                var station = await UpsertStationAsync(externalStationId, stationNames, cancellationToken);
                if (station.Id == 0)
                    stationsCreated++;

                stationMap[externalStationId] = station;
            }

            await _db.SaveChangesAsync(cancellationToken);

            var firstStop = orderedStops.First();
            var lastStop = orderedStops.Last();
            var departureStation = stationMap[firstStop.StationId];
            var arrivalStation = stationMap[lastStop.StationId];
            var departureTime = BuildDateTime(selectedOperatingDate, firstStop.DepartureDay, firstStop.DepartureTime)
                ?? BuildDateTime(selectedOperatingDate, firstStop.ArrivalDay, firstStop.ArrivalTime)
                ?? selectedOperatingDate.ToDateTime(TimeOnly.MinValue);
            var arrivalTime = BuildDateTime(selectedOperatingDate, lastStop.ArrivalDay, lastStop.ArrivalTime)
                ?? BuildDateTime(selectedOperatingDate, lastStop.DepartureDay, lastStop.DepartureTime)
                ?? departureTime;

            if (arrivalTime < departureTime)
                arrivalTime = departureTime;

            var trainNumber = GetTrainNumber(route);
            var trainCode = BuildTrainCode(route, trainNumber);
            var train = await FindExistingTrainAsync(route, trainCode, cancellationToken);
            var trainCreated = train == null;
            if (train == null)
            {
                train = new Train();
                _db.Trains.Add(train);
            }

            train.Code = trainCode;
            train.Name = BuildTrainName(route, trainNumber);
            train.Type = MapTrainType(route.CommercialCategorySymbol);
            train.Status = "Active";
            train.DepartureStation = departureStation.Name;
            train.ArrivalStation = arrivalStation.Name;
            train.DepartureTime = departureTime;
            train.ArrivalTime = arrivalTime;
            train.ExternalSource = _options.ExternalSource;
            train.ExternalCarrierCode = route.CarrierCode ?? string.Empty;
            train.ExternalCommercialCategorySymbol = route.CommercialCategorySymbol ?? string.Empty;
            train.ExternalNationalNumber = route.NationalNumber ?? string.Empty;
            train.ExternalInternationalArrivalNumber = route.InternationalArrivalNumber ?? string.Empty;
            train.ExternalInternationalDepartureNumber = route.InternationalDepartureNumber ?? string.Empty;

            await _db.SaveChangesAsync(cancellationToken);
            var consistResult = await EnsureDefaultConsistAsync(train, route, departureTime, cancellationToken);

            var routeCode = BuildRouteCode(route, selectedOperatingDate);
            var trainRoute = await _db.TrainRoutes
                .Include(r => r.RouteStops)
                .FirstOrDefaultAsync(r =>
                    r.ExternalSource == _options.ExternalSource &&
                    r.ExternalScheduleId == route.ScheduleId &&
                    r.ExternalOrderId == route.OrderId &&
                    r.ExternalOperatingDate == selectedOperatingDate,
                    cancellationToken);
            var routeCreated = trainRoute == null;
            if (trainRoute == null)
            {
                trainRoute = new TrainRoute();
                _db.TrainRoutes.Add(trainRoute);
            }

            trainRoute.Code = routeCode;
            trainRoute.Name = Truncate($"{departureStation.Name} -> {arrivalStation.Name}", 240);
            trainRoute.DepartureStationId = departureStation.Id;
            trainRoute.ArrivalStationId = arrivalStation.Id;
            trainRoute.EstimatedDurationMinutes = Math.Max(0, (int)Math.Round((arrivalTime - departureTime).TotalMinutes));
            trainRoute.DistanceKm = 0m;
            trainRoute.OperatingDays = "Imported";
            trainRoute.IntermediateStops = Truncate(string.Join(", ", orderedStops
                .Skip(1)
                .Take(Math.Max(0, orderedStops.Count - 2))
                .Select(stop => stationMap[stop.StationId].Name)), 1000);
            trainRoute.IsActive = true;
            trainRoute.ExternalSource = _options.ExternalSource;
            trainRoute.ExternalScheduleId = route.ScheduleId;
            trainRoute.ExternalOrderId = route.OrderId;
            trainRoute.ExternalTrainOrderId = route.TrainOrderId;
            trainRoute.ExternalOperatingDate = selectedOperatingDate;

            var existingStops = trainRoute.RouteStops.ToList();
            if (existingStops.Count > 0)
                _db.TrainRouteStops.RemoveRange(existingStops);

            foreach (var stop in orderedStops)
            {
                var arrivalOffset = BuildOffsetMinutes(selectedOperatingDate, departureTime, stop.ArrivalDay, stop.ArrivalTime);
                var departureOffset = BuildOffsetMinutes(selectedOperatingDate, departureTime, stop.DepartureDay, stop.DepartureTime);
                var isFirst = stop == firstStop;
                var isLast = stop == lastStop;

                trainRoute.RouteStops.Add(new TrainRouteStop
                {
                    StationId = stationMap[stop.StationId].Id,
                    StopOrder = stop.OrderNumber,
                    ArrivalOffsetMinutes = isFirst ? null : arrivalOffset,
                    DepartureOffsetMinutes = isLast ? null : departureOffset,
                    Platform = FirstNonBlank(stop.DeparturePlatform, stop.ArrivalPlatform),
                    Track = FirstNonBlank(stop.DepartureTrack, stop.ArrivalTrack),
                    StopType = MapStopType(stop.StopTypeName, isFirst || isLast),
                    ExternalStationId = stop.StationId,
                    ExternalStopTypeId = stop.StopTypeId,
                    ExternalStopTypeName = stop.StopTypeName ?? string.Empty,
                    ExternalArrivalTrainNumber = stop.ArrivalTrainNumber ?? string.Empty,
                    ExternalDepartureTrainNumber = stop.DepartureTrainNumber ?? string.Empty,
                    ArrivalDayOffset = stop.ArrivalDay,
                    DepartureDayOffset = stop.DepartureDay
                });
            }

            await _db.SaveChangesAsync(cancellationToken);

            var dataVersion = await _client.GetDataVersionAsync(cancellationToken);
            var trip = await _db.Trips.FirstOrDefaultAsync(t =>
                    t.ExternalSource == _options.ExternalSource &&
                    t.ExternalScheduleId == route.ScheduleId &&
                    t.ExternalOrderId == route.OrderId &&
                    t.ExternalOperatingDate == selectedOperatingDate,
                    cancellationToken);
            var tripCreated = trip == null;
            if (trip == null)
            {
                trip = new Trip();
                _db.Trips.Add(trip);
            }

            trip.TrainId = train.Id;
            trip.TrainRouteId = trainRoute.Id;
            trip.DepartureTime = departureTime;
            trip.ArrivalTime = arrivalTime;
            trip.Platform = FirstNonBlank(firstStop.DeparturePlatform, firstStop.ArrivalPlatform);
            trip.Track = FirstNonBlank(firstStop.DepartureTrack, firstStop.ArrivalTrack);
            trip.OriginalPlatform = trip.Platform;
            trip.OriginalTrack = trip.Track;
            trip.Status = "Scheduled";
            trip.ExternalSource = _options.ExternalSource;
            trip.ExternalScheduleId = route.ScheduleId;
            trip.ExternalOrderId = route.OrderId;
            trip.ExternalTrainOrderId = route.TrainOrderId;
            trip.ExternalOperatingDate = selectedOperatingDate;
            trip.ExternalImportedAtUtc = DateTime.UtcNow;
            trip.ExternalRawVersion = FirstNonBlank(
                dataVersion.SchedulesVersion?.ToString(),
                dataVersion.DataVersion?.ToString());

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new OpenRailwayImportRouteResultDto
            {
                ExternalSource = _options.ExternalSource,
                ScheduleId = route.ScheduleId,
                OrderId = route.OrderId,
                OperatingDate = selectedOperatingDate,
                TrainId = train.Id,
                TrainRouteId = trainRoute.Id,
                TripId = trip.Id,
                TrainCreated = trainCreated,
                RouteCreated = routeCreated,
                TripCreated = tripCreated,
                DefaultConsistApplied = consistResult.Applied,
                StationsCreated = stationsCreated,
                CarriagesCreated = consistResult.CarriagesCreated,
                SeatsCreated = consistResult.SeatsCreated,
                StopsWritten = orderedStops.Count,
                TrainCode = train.Code,
                RouteCode = trainRoute.Code,
                RouteName = trainRoute.Name
            };
        }

        public async Task<OpenRailwayImportDateResultDto> ImportRoutesForDateAsync(
            DateOnly date,
            OpenRailwayImportDateRequest request,
            CancellationToken cancellationToken)
        {
            var safeLimit = Math.Clamp(request.Limit, 1, 100);
            var requestedRoutes = request.Routes
                .Where(route => route.ScheduleId > 0 && route.OrderId > 0)
                .GroupBy(route => new { route.ScheduleId, route.OrderId })
                .Select(group => group.First())
                .Take(safeLimit)
                .ToList();

            if (requestedRoutes.Count == 0)
            {
                var availableRoutes = await _client.GetRouteIdsAsync(date, cancellationToken);
                requestedRoutes = (availableRoutes.Routes ?? [])
                    .Where(route => route.ScheduleId > 0 && route.OrderId > 0)
                    .GroupBy(route => new { route.ScheduleId, route.OrderId })
                    .Select(group => new OpenRailwayImportRouteKeyDto
                    {
                        ScheduleId = group.Key.ScheduleId,
                        OrderId = group.Key.OrderId
                    })
                    .Take(safeLimit)
                    .ToList();
            }

            var result = new OpenRailwayImportDateResultDto
            {
                ExternalSource = _options.ExternalSource,
                Date = date,
                DryRun = request.DryRun,
                RequestedCount = requestedRoutes.Count
            };

            foreach (var route in requestedRoutes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var item = new OpenRailwayImportDateItemResultDto
                {
                    ScheduleId = route.ScheduleId,
                    OrderId = route.OrderId
                };

                try
                {
                    if (request.DryRun)
                    {
                        item.Preview = await PreviewRouteAsync(route.ScheduleId, route.OrderId, cancellationToken);
                        item.Status = "Previewed";
                    }
                    else
                    {
                        item.Import = await ImportRouteAsync(route.ScheduleId, route.OrderId, date, cancellationToken);
                        item.Status = "Imported";
                    }

                    result.SucceededCount++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    item.Status = "Failed";
                    item.Error = ex.Message;
                    result.FailedCount++;
                }

                result.Items.Add(item);
            }

            result.SkippedCount = Math.Max(0, result.RequestedCount - result.SucceededCount - result.FailedCount);
            return result;
        }

        public async Task<OpenRailwaySeedSnapshotDto> ExportSeedSnapshotAsync(
            DateOnly? operatingDate,
            CancellationToken cancellationToken)
        {
            var routesQuery = _db.TrainRoutes
                .Include(route => route.DepartureStation)
                .Include(route => route.ArrivalStation)
                .Include(route => route.RouteStops)
                    .ThenInclude(stop => stop.Station)
                .Where(route => route.ExternalSource == _options.ExternalSource);

            if (operatingDate.HasValue)
            {
                routesQuery = routesQuery.Where(route =>
                    route.ExternalOperatingDate == operatingDate.Value);
            }

            var routes = await routesQuery
                .OrderBy(route => route.Code)
                .ToListAsync(cancellationToken);

            var routeIds = routes.Select(route => route.Id).ToHashSet();
            var trips = await _db.Trips
                .Include(trip => trip.Train)
                    .ThenInclude(train => train.Carriages)
                .Include(trip => trip.TrainRoute)
                .Include(trip => trip.Fares)
                .Where(trip =>
                    trip.ExternalSource == _options.ExternalSource &&
                    routeIds.Contains(trip.TrainRouteId))
                .OrderBy(trip => trip.DepartureTime)
                .ToListAsync(cancellationToken);

            var stations = routes
                .SelectMany(route => route.RouteStops.Select(stop => stop.Station))
                .Concat(routes.Select(route => route.DepartureStation))
                .Concat(routes.Select(route => route.ArrivalStation))
                .Where(station => station != null)
                .GroupBy(station => StationSnapshotKey(station))
                .Select(group => group.First())
                .OrderBy(station => station.Name)
                .Select(station => new OpenRailwaySeedStationDto
                {
                    Code = station.Code,
                    Name = station.Name,
                    City = station.City,
                    ExternalStationId = station.ExternalStationId
                })
                .ToList();

            var trains = trips
                .Select(trip => trip.Train)
                .Where(train => train != null)
                .GroupBy(train => train.Code, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(train => train.Code)
                .Select(train => new OpenRailwaySeedTrainDto
                {
                    Code = train.Code,
                    Name = train.Name,
                    Type = train.Type,
                    CarriageCount = train.CarriageCount,
                    SeatsPerCarriage = train.SeatsPerCarriage,
                    Status = train.Status,
                    DepartureStation = train.DepartureStation,
                    ArrivalStation = train.ArrivalStation,
                    DepartureTime = train.DepartureTime,
                    ArrivalTime = train.ArrivalTime,
                    ExternalCarrierCode = train.ExternalCarrierCode,
                    ExternalCommercialCategorySymbol = train.ExternalCommercialCategorySymbol,
                    ExternalNationalNumber = train.ExternalNationalNumber,
                    ExternalInternationalArrivalNumber = train.ExternalInternationalArrivalNumber,
                    ExternalInternationalDepartureNumber = train.ExternalInternationalDepartureNumber,
                    Carriages = train.Carriages
                        .OrderBy(carriage => carriage.Position)
                        .ThenBy(carriage => carriage.Coach)
                        .Select(carriage => new OpenRailwaySeedCarriageDto
                        {
                            Coach = carriage.Coach,
                            Position = carriage.Position,
                            ClassType = carriage.ClassType,
                            LayoutType = carriage.LayoutType,
                            VehicleType = carriage.VehicleType,
                            SeatCount = carriage.SeatCount,
                            HasBikeSpace = carriage.HasBikeSpace,
                            HasAccessibleSpace = carriage.HasAccessibleSpace,
                            HasFamilyCompartment = carriage.HasFamilyCompartment,
                            HasDiningSection = carriage.HasDiningSection,
                            Notes = carriage.Notes
                        })
                        .ToList()
                })
                .ToList();

            return new OpenRailwaySeedSnapshotDto
            {
                ExternalSource = _options.ExternalSource,
                ExportedAtUtc = DateTime.UtcNow,
                Stations = stations,
                Trains = trains,
                Routes = routes
                    .OrderBy(route => route.Code)
                    .Select(route => new OpenRailwaySeedRouteDto
                    {
                        Code = route.Code,
                        Name = route.Name,
                        AdminDisplayName = route.AdminDisplayName,
                        RouteFingerprint = route.RouteFingerprint,
                        DistanceKm = route.DistanceKm,
                        EstimatedDurationMinutes = route.EstimatedDurationMinutes,
                        OperatingDays = route.OperatingDays,
                        IntermediateStops = route.IntermediateStops,
                        IsActive = route.IsActive,
                        ExternalScheduleId = route.ExternalScheduleId,
                        ExternalOrderId = route.ExternalOrderId,
                        ExternalTrainOrderId = route.ExternalTrainOrderId,
                        ExternalOperatingDate = route.ExternalOperatingDate,
                        DepartureExternalStationId = route.DepartureStation.ExternalStationId,
                        ArrivalExternalStationId = route.ArrivalStation.ExternalStationId,
                        Stops = route.RouteStops
                            .OrderBy(stop => stop.StopOrder)
                            .Select(stop => new OpenRailwaySeedRouteStopDto
                            {
                                ExternalStationId = stop.ExternalStationId ?? stop.Station.ExternalStationId,
                                StationCode = stop.Station.Code,
                                StopOrder = stop.StopOrder,
                                ArrivalOffsetMinutes = stop.ArrivalOffsetMinutes,
                                DepartureOffsetMinutes = stop.DepartureOffsetMinutes,
                                Platform = stop.Platform,
                                Track = stop.Track,
                                StopType = stop.StopType,
                                ExternalStopTypeId = stop.ExternalStopTypeId,
                                ExternalStopTypeName = stop.ExternalStopTypeName,
                                ExternalArrivalTrainNumber = stop.ExternalArrivalTrainNumber,
                                ExternalDepartureTrainNumber = stop.ExternalDepartureTrainNumber,
                                ArrivalDayOffset = stop.ArrivalDayOffset,
                                DepartureDayOffset = stop.DepartureDayOffset
                            })
                            .ToList()
                    })
                    .ToList(),
                Trips = trips
                    .OrderBy(trip => trip.DepartureTime)
                    .Select(trip => new OpenRailwaySeedTripDto
                    {
                        TrainCode = trip.Train.Code,
                        RouteCode = trip.TrainRoute.Code,
                        DepartureTime = trip.DepartureTime,
                        ArrivalTime = trip.ArrivalTime,
                        Platform = trip.Platform,
                        Track = trip.Track,
                        Status = trip.Status,
                        DelayMinutes = trip.DelayMinutes,
                        CancellationReason = trip.CancellationReason,
                        OriginalPlatform = trip.OriginalPlatform,
                        OriginalTrack = trip.OriginalTrack,
                        DisruptionMessage = trip.DisruptionMessage,
                        DisruptionSeverity = trip.DisruptionSeverity,
                        ExternalScheduleId = trip.ExternalScheduleId,
                        ExternalOrderId = trip.ExternalOrderId,
                        ExternalTrainOrderId = trip.ExternalTrainOrderId,
                        ExternalOperatingDate = trip.ExternalOperatingDate,
                        ExternalRawVersion = trip.ExternalRawVersion,
                        Fares = trip.Fares
                            .OrderBy(fare => fare.ClassType)
                            .Select(fare => new OpenRailwaySeedFareDto
                            {
                                ClassType = fare.ClassType,
                                Price = fare.Price,
                                Currency = fare.Currency
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        private static string FirstNonBlank(params string?[] values)
            => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

        private static string StationSnapshotKey(Station station)
            => station.ExternalStationId.HasValue
                ? $"external:{station.ExternalStationId.Value}"
                : $"code:{station.Code}";

        private static DateOnly ResolveOperatingDate(OpenRailwayRouteDto route, DateOnly? requestedDate)
        {
            if (requestedDate.HasValue)
                return requestedDate.Value;

            var operatingDate = route.OperatingDates?
                .OrderBy(date => date)
                .FirstOrDefault();

            if (operatingDate.HasValue)
                return operatingDate.Value;

            return DateOnly.FromDateTime(DateTime.Today);
        }

        private async Task<Station> UpsertStationAsync(
            int externalStationId,
            IReadOnlyDictionary<int, string> stationNames,
            CancellationToken cancellationToken)
        {
            var station = await _db.Stations.FirstOrDefaultAsync(s =>
                    s.ExternalSource == _options.ExternalSource &&
                    s.ExternalStationId == externalStationId,
                    cancellationToken);

            var name = stationNames.TryGetValue(externalStationId, out var resolvedName) &&
                !string.IsNullOrWhiteSpace(resolvedName)
                    ? resolvedName.Trim()
                    : $"PLK Station {externalStationId}";
            name = Truncate(name, 200);

            if (station == null)
            {
                var code = await BuildUniqueStationCodeAsync(name, externalStationId, cancellationToken);
                station = new Station
                {
                    Code = code,
                    Name = name,
                    City = name,
                    ExternalSource = _options.ExternalSource,
                    ExternalStationId = externalStationId
                };

                _db.Stations.Add(station);
                return station;
            }

            station.Name = name;
            station.City = string.IsNullOrWhiteSpace(station.City) ? name : station.City;
            return station;
        }

        private async Task<Dictionary<int, string>> ResolveStationNamesAsync(
            IReadOnlyCollection<int> stationIds,
            CancellationToken cancellationToken)
        {
            var missing = stationIds.ToHashSet();
            var names = await _db.Stations
                .Where(station =>
                    station.ExternalSource == _options.ExternalSource &&
                    station.ExternalStationId.HasValue &&
                    missing.Contains(station.ExternalStationId.Value))
                .ToDictionaryAsync(
                    station => station.ExternalStationId!.Value,
                    station => station.Name,
                    cancellationToken);

            missing.ExceptWith(names.Keys);
            if (missing.Count == 0)
                return names;

            const int pageSize = 500;
            var page = 1;
            var totalPages = 1;
            while (missing.Count > 0 && page <= totalPages)
            {
                var response = await _client.SearchStationsAsync(null, page, pageSize, cancellationToken);
                totalPages = Math.Max(1, response.TotalPages);

                foreach (var station in response.Stations ?? [])
                {
                    if (!missing.Contains(station.Id))
                        continue;

                    names[station.Id] = station.Name ?? string.Empty;
                    missing.Remove(station.Id);
                }

                page++;
            }

            return names;
        }

        private async Task<string> BuildUniqueStationCodeAsync(
            string stationName,
            int externalStationId,
            CancellationToken cancellationToken)
        {
            var baseCode = BuildStationCode(stationName, externalStationId);
            var code = baseCode;
            var suffix = 2;
            while (await _db.Stations.AnyAsync(station => station.Code == code, cancellationToken))
            {
                code = $"{baseCode}-{suffix}";
                suffix++;
            }

            return code;
        }

        private static string BuildStationCode(string stationName, int externalStationId)
        {
            var letters = new string((stationName ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Take(12)
                .ToArray())
                .ToUpperInvariant();

            return string.IsNullOrWhiteSpace(letters)
                ? $"PLK{externalStationId}"
                : letters;
        }

        private async Task<Train?> FindExistingTrainAsync(
            OpenRailwayRouteDto route,
            string trainCode,
            CancellationToken cancellationToken)
        {
            var nationalNumber = route.NationalNumber ?? string.Empty;
            var category = route.CommercialCategorySymbol ?? string.Empty;
            var carrier = route.CarrierCode ?? string.Empty;

            var train = await _db.Trains.FirstOrDefaultAsync(t =>
                    t.ExternalSource == _options.ExternalSource &&
                    t.ExternalCarrierCode == carrier &&
                    t.ExternalCommercialCategorySymbol == category &&
                    t.ExternalNationalNumber == nationalNumber &&
                    t.ExternalNationalNumber != string.Empty,
                    cancellationToken);

            return train ?? await _db.Trains.FirstOrDefaultAsync(t => t.Code == trainCode, cancellationToken);
        }

        private async Task<DefaultConsistResult> EnsureDefaultConsistAsync(
            Train train,
            OpenRailwayRouteDto route,
            DateTime departureTime,
            CancellationToken cancellationToken)
        {
            var existingCarriages = await _db.TrainCarriages
                .Where(carriage => carriage.TrainId == train.Id)
                .OrderBy(carriage => carriage.Position)
                .ToListAsync(cancellationToken);

            if (existingCarriages.Count > 0)
            {
                var existingSeatCount = await _db.Seats.CountAsync(seat => seat.TrainId == train.Id, cancellationToken);
                if (existingSeatCount > 0)
                    return new DefaultConsistResult(false, 0, 0);

                var missingSeats = BuildSeatsForCarriages(train.Id, existingCarriages);
                _db.Seats.AddRange(missingSeats);
                await _db.SaveChangesAsync(cancellationToken);

                return new DefaultConsistResult(false, 0, missingSeats.Count);
            }

            var carriages = BuildDefaultCarriages(route, train, departureTime)
                .Select(seed => new TrainCarriage
                {
                    TrainId = train.Id,
                    Coach = seed.Coach,
                    Position = seed.Position,
                    ClassType = seed.ClassType,
                    LayoutType = seed.LayoutType,
                    VehicleType = seed.VehicleType,
                    SeatCount = seed.SeatCount,
                    HasBikeSpace = seed.HasBikeSpace,
                    HasAccessibleSpace = seed.HasAccessibleSpace,
                    HasFamilyCompartment = seed.HasFamilyCompartment,
                    HasDiningSection = seed.HasDiningSection,
                    Notes = seed.Notes
                })
                .ToList();

            if (carriages.Count == 0)
                return new DefaultConsistResult(false, 0, 0);

            var existingSeats = await _db.Seats
                .Where(seat => seat.TrainId == train.Id)
                .ToListAsync(cancellationToken);
            if (existingSeats.Count > 0)
                _db.Seats.RemoveRange(existingSeats);

            _db.TrainCarriages.AddRange(carriages);
            ApplyCapacitySummary(train, carriages);
            await _db.SaveChangesAsync(cancellationToken);

            var seats = BuildSeatsForCarriages(train.Id, carriages);

            _db.Seats.AddRange(seats);
            await _db.SaveChangesAsync(cancellationToken);

            return new DefaultConsistResult(true, carriages.Count, seats.Count);
        }

        private static List<Seat> BuildSeatsForCarriages(int trainId, IEnumerable<TrainCarriage> carriages)
            => carriages
                .Where(IsPassengerCarriage)
                .SelectMany(carriage => GetSeatNumbersForCarriage(carriage).Select(number => new Seat
                {
                    TrainId = trainId,
                    Coach = carriage.Coach,
                    Number = number,
                    ClassType = GetSeatClassType(carriage, int.Parse(number)),
                    IsAvailable = true
                }))
                .ToList();

        private static List<DefaultCarriageSeed> BuildDefaultCarriages(
            OpenRailwayRouteDto route,
            Train train,
            DateTime departureTime)
        {
            var category = (route.CommercialCategorySymbol ?? string.Empty).Trim().ToUpperInvariant();
            var name = $"{route.Name} {train.Name}".Trim();
            var isNightTrain = departureTime.Hour >= 20 ||
                departureTime.Hour < 5 ||
                name.Contains("night", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("noc", StringComparison.OrdinalIgnoreCase);

            return category switch
            {
                "EIP" => BuildEipCarriages(),
                "EIC" => isNightTrain ? BuildPremiumNightCarriages("EIC") : BuildEicCarriages(),
                "TLK" => isNightTrain ? BuildTlkNightCarriages() : BuildTlkCarriages(),
                _ when LooksLikeEmu(name) => BuildIcEmuCarriages(name),
                _ when isNightTrain => BuildPremiumNightCarriages("IC"),
                _ => BuildIcLocoHauledCarriages()
            };
        }

        private static List<DefaultCarriageSeed> BuildEipCarriages() =>
        [
            new("1", 1, "Class 1", "EmuFirstCab", "ED250-1 first-class cab unit", 45, Notes: "First-class Pendolino unit."),
            new("2", 2, "Class 2", "EmuSecondFamily", "ED250-2 second-class family unit", 52, HasFamilyCompartment: true, Notes: "Second-class Pendolino unit with family area."),
            new("3", 3, "Class 2", "EmuDiningAccessible", "ED250-3 accessible dining unit", 12, HasAccessibleSpace: true, HasDiningSection: true, Notes: "Accessible WARS dining unit with wheelchair spaces."),
            new("4", 4, "Class 2", "EmuSecondOpen", "ED250-4 second-class open unit", 58, Notes: "Second-class Pendolino open saloon."),
            new("5", 5, "Class 2", "EmuSecondOpen", "ED250-5 second-class open unit", 58, Notes: "Second-class Pendolino open saloon."),
            new("6", 6, "Class 2", "EmuSecondOpen", "ED250-6 second-class open unit", 58, Notes: "Second-class Pendolino open saloon."),
            new("7", 7, "Class 2", "EmuSecondQuietCab", "ED250-7 quiet second-class cab unit", 61, Notes: "Quiet second-class Pendolino unit.")
        ];

        private static List<DefaultCarriageSeed> BuildEicCarriages() =>
        [
            new("Loco", 0, "Locomotive", "Locomotive", "EU200 or EU44 electric locomotive", 0, Notes: "Premium EIC locomotive."),
            new("1", 1, "Class 1", "FirstCompartment", "A9nouz first-class compartment", 54, Notes: "First-class compartment coach."),
            new("2", 2, "Class 1", "OpenFirst", "A9mnopuz first-class open coach", 54, Notes: "First-class open coach."),
            new("3", 3, "Dining", "Restaurant", "WRnouz WARS restaurant", 0, HasDiningSection: true, Notes: "Restaurant and bar car operated by WARS."),
            new("4", 4, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new("5", 5, "Class 2", "OpenSecondAccessible", "B8bnopuz accessible open coach", 82, HasAccessibleSpace: true, Notes: "Second-class open coach with wheelchair spaces."),
            new("6", 6, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks."),
            new("7", 7, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "Additional second-class open coach.")
        ];

        private static List<DefaultCarriageSeed> BuildIcLocoHauledCarriages() =>
        [
            new("Loco", 0, "Locomotive", "Locomotive", "EU160, EP09, EU07 or EU08 locomotive", 0, Notes: "Standard IC locomotive."),
            new("1", 1, "Class 1/2", "ComboFirstSecond", "AB9nouz first/second combo", 54, Notes: "Mixed first and second-class coach."),
            new("2", 2, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new("3", 3, "Class 2", "SecondFamilyCompartment", "Bmnopux family compartment", 66, HasFamilyCompartment: true, Notes: "Second-class coach with family compartment."),
            new("4", 4, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks and vending area."),
            new("5", 5, "Class 2", "OpenSecondAccessible", "B8bnopuz accessible open coach", 82, HasAccessibleSpace: true, Notes: "Second-class open coach with wheelchair spaces."),
            new("6", 6, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "Additional second-class open coach.")
        ];

        private static List<DefaultCarriageSeed> BuildIcEmuCarriages(string name)
        {
            var series = name.Contains("Dart", StringComparison.OrdinalIgnoreCase) ||
                name.Contains("ED161", StringComparison.OrdinalIgnoreCase)
                    ? "ED161 Dart"
                    : name.Contains("ED74", StringComparison.OrdinalIgnoreCase)
                        ? "ED74"
                        : "ED160 Flirt";

            return
            [
                new("1", 1, "Class 1/2", "EmuFirstSecond", $"{series} cab unit", 62, HasAccessibleSpace: true, Notes: "Mixed first and second-class EMU unit."),
                new("2", 2, "Class 2", "EmuSecondFamily", $"{series} family unit", 76, HasFamilyCompartment: true, Notes: "Second-class EMU unit with family area."),
                new("3", 3, "Class 2", "EmuSecondBike", $"{series} bicycle unit", 72, HasBikeSpace: true, Notes: "Second-class EMU unit with bicycle spaces."),
                new("4", 4, "Class 2", "EmuSecondOpen", $"{series} open unit", 80, Notes: "Second-class EMU open saloon.")
            ];
        }

        private static List<DefaultCarriageSeed> BuildTlkCarriages() =>
        [
            new("Loco", 0, "Locomotive", "Locomotive", "EU07 or SU4210 locomotive", 0, Notes: "TLK locomotive."),
            new("1", 1, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new("2", 2, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks."),
            new("3", 3, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "Second-class open coach."),
            new("4", 4, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Additional second-class compartment coach.")
        ];

        private static List<DefaultCarriageSeed> BuildPremiumNightCarriages(string category) =>
        [
            new("Loco", 0, "Locomotive", "Locomotive", category == "EIC" ? "EU200, EU44 or EU160 locomotive" : "EU160, EP09, SU160 or EP07 locomotive", 0, Notes: "Night train locomotive."),
            new("1", 1, "Sleeper", "InternationalSleeper", "WLAB10mnouz international sleeper coach", 22, Notes: "International sleeper with six 3-berth compartments and two deluxe 2-berth shower compartments."),
            new("2", 2, "Couchette", "Couchette", "Bc couchette coach", 30, HasAccessibleSpace: true, Notes: "4-berth couchette coach with one accessible compartment and seven 4-berth compartments."),
            new("3", 3, "Class 1", "FirstCompartment", "A9nouz first-class compartment", 54, Notes: "First-class seating coach."),
            new("4", 4, "Dining", "Restaurant", "WRnouz WARS restaurant", 0, HasDiningSection: true, Notes: "Restaurant and bar car operated by WARS."),
            new("5", 5, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new("6", 6, "Class 2", "OpenSecondAccessible", "B8bnopuz accessible open coach", 82, HasAccessibleSpace: true, Notes: "Accessible second-class open coach."),
            new("7", 7, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks."),
            new("8", 8, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "Additional second-class open coach.")
        ];

        private static List<DefaultCarriageSeed> BuildTlkNightCarriages() =>
        [
            new("Loco", 0, "Locomotive", "Locomotive", "EU07, EP07 or SU4210 locomotive", 0, Notes: "TLK night train locomotive."),
            new("1", 1, "Sleeper", "Sleeper", "WLAB sleeper coach", 30, Notes: "Sleeper coach used on overnight TLK services."),
            new("2", 2, "Couchette", "Couchette", "Bc couchette coach", 30, HasAccessibleSpace: true, Notes: "4-berth couchette coach with one accessible compartment and seven 4-berth compartments."),
            new("3", 3, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new("4", 4, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks."),
            new("5", 5, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "Second-class open coach."),
            new("6", 6, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Additional second-class compartment coach.")
        ];

        private static bool LooksLikeEmu(string name)
            => name.Contains("ED160", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("ED161", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("ED74", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Dart", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Flirt", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("PesaDART", StringComparison.OrdinalIgnoreCase);

        private static void ApplyCapacitySummary(Train train, IReadOnlyCollection<TrainCarriage> carriages)
        {
            var passengerCarriages = carriages.Where(IsPassengerCarriage).ToList();
            train.CarriageCount = carriages.Count(carriage => carriage.LayoutType != "Locomotive");
            train.SeatsPerCarriage = passengerCarriages.Count == 0
                ? 0
                : passengerCarriages.Max(carriage => carriage.SeatCount);
        }

        private static bool IsPassengerCarriage(TrainCarriage carriage) =>
            carriage.LayoutType != "Locomotive" &&
            carriage.LayoutType != "Restaurant" &&
            !carriage.HasDiningSection &&
            carriage.SeatCount > 0;

        private static string GetSeatClassType(TrainCarriage carriage, int seatNumber)
        {
            if (carriage.LayoutType.Equals("InternationalSleeper", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("Sleeper", StringComparison.OrdinalIgnoreCase))
                return "Sleeper";

            if (carriage.LayoutType.Equals("Couchette", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("SixBerthCouchette", StringComparison.OrdinalIgnoreCase))
                return "Couchette";

            if (IsMixedClassCarriage(carriage))
                return seatNumber <= GetFirstClassSeatCount(carriage) ? "Class 1" : "Class 2";

            return carriage.ClassType == "Class 1/2" ? "Class 2" : carriage.ClassType;
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

        private static bool IsMixedClassCarriage(TrainCarriage carriage) =>
            carriage.ClassType == "Class 1/2" ||
            carriage.LayoutType.Contains("FirstSecond", StringComparison.OrdinalIgnoreCase);

        private static int GetFirstClassSeatCount(TrainCarriage carriage)
        {
            if (carriage.LayoutType.Equals("ComboFirstSecond", StringComparison.OrdinalIgnoreCase))
                return Math.Min(18, carriage.SeatCount);

            if (carriage.LayoutType.Equals("EmuFirstSecond", StringComparison.OrdinalIgnoreCase))
                return Math.Min(16, carriage.SeatCount);

            return Math.Min(18, carriage.SeatCount);
        }

        private static string GetTrainNumber(OpenRailwayRouteDto route)
            => FirstNonBlank(
                route.NationalNumber,
                route.InternationalDepartureNumber,
                route.InternationalArrivalNumber,
                route.OrderId.ToString());

        private static string BuildTrainCode(OpenRailwayRouteDto route, string trainNumber)
            => string.IsNullOrWhiteSpace(route.CommercialCategorySymbol)
                ? trainNumber
                : $"{route.CommercialCategorySymbol}-{trainNumber}";

        private static string BuildTrainName(OpenRailwayRouteDto route, string trainNumber)
            => string.IsNullOrWhiteSpace(route.Name)
                ? trainNumber
                : $"{route.CommercialCategorySymbol} {trainNumber} {route.Name}".Trim();

        private static string BuildRouteCode(OpenRailwayRouteDto route, DateOnly operatingDate)
        {
            var code = $"PLK-{route.ScheduleId}-{route.OrderId}-{operatingDate:yyyyMMdd}";
            if (code.Length <= 32)
                return code;

            return $"PLK-{route.ScheduleId:X}-{route.OrderId:X}-{operatingDate:yyMMdd}";
        }

        private static string MapTrainType(string? category)
        {
            var normalized = (category ?? string.Empty).Trim().ToUpperInvariant();
            return normalized switch
            {
                "EIP" => "Express InterCity Premium",
                "EIC" => "Express InterCity",
                "TLK" => "Twoje Linie Kolejowe",
                _ => "InterCity"
            };
        }

        private static string MapStopType(string? stopTypeName, bool isTerminus)
        {
            if (isTerminus)
                return "Major";

            if (string.IsNullOrWhiteSpace(stopTypeName))
                return "Normal";

            return stopTypeName.Length > 30
                ? stopTypeName[..30]
                : stopTypeName;
        }

        private static DateTime? BuildDateTime(DateOnly operatingDate, int? dayOffset, TimeSpan? time)
        {
            if (!time.HasValue)
                return null;

            return operatingDate
                .AddDays(dayOffset.GetValueOrDefault())
                .ToDateTime(TimeOnly.FromTimeSpan(time.Value));
        }

        private static int? BuildOffsetMinutes(
            DateOnly operatingDate,
            DateTime departureTime,
            int? dayOffset,
            TimeSpan? time)
        {
            var stopTime = BuildDateTime(operatingDate, dayOffset, time);
            if (!stopTime.HasValue)
                return null;

            return Math.Max(0, (int)Math.Round((stopTime.Value - departureTime).TotalMinutes));
        }

        private static string Truncate(string value, int maxLength)
            => value.Length <= maxLength ? value : value[..maxLength];

        private static string? FormatStopTime(int? dayOffset, TimeSpan? time)
        {
            if (!time.HasValue)
                return null;

            var value = time.Value.ToString(@"hh\:mm");
            var days = dayOffset.GetValueOrDefault();
            return days > 0
                ? $"{value} +{days}d"
                : value;
        }

        private sealed record DefaultConsistResult(
            bool Applied,
            int CarriagesCreated,
            int SeatsCreated);

        private sealed record DefaultCarriageSeed(
            string Coach,
            int Position,
            string ClassType,
            string LayoutType,
            string VehicleType,
            int SeatCount,
            bool HasBikeSpace = false,
            bool HasAccessibleSpace = false,
            bool HasFamilyCompartment = false,
            bool HasDiningSection = false,
            string Notes = "");
    }
}
