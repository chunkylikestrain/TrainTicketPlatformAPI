using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/routes")]
    public class AdminRoutesController : ControllerBase
    {
        private readonly TrainTicketDbContext _db;

        public AdminRoutesController(TrainTicketDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminRouteDto>>> GetAll()
        {
            var routes = await _db.TrainRoutes
                .AsNoTracking()
                .Include(r => r.DepartureStation)
                .Include(r => r.ArrivalStation)
                .Include(r => r.RouteStops)
                    .ThenInclude(s => s.Station)
                .OrderBy(r => r.Code)
                .ToListAsync();

            return Ok(routes.Select(ToDto));
        }

        [HttpPost]
        public async Task<ActionResult<AdminRouteDto>> Create(AdminRouteDto request)
        {
            var stations = await LoadRouteStationsAsync(request);
            var departureStation = stations[request.DepartureStationId];
            var arrivalStation = stations[request.ArrivalStationId];

            var route = new TrainRoute
            {
                Code = BuildRouteCode(request, departureStation, arrivalStation),
                Name = BuildRouteName(request, departureStation, arrivalStation),
                AdminDisplayName = BuildAdminDisplayName(request, departureStation, arrivalStation, stations),
                RouteFingerprint = BuildRouteFingerprint(request, departureStation, arrivalStation, stations),
                DepartureStationId = request.DepartureStationId,
                ArrivalStationId = request.ArrivalStationId,
                DistanceKm = request.DistanceKm,
                EstimatedDurationMinutes = request.EstimatedDurationMinutes,
                OperatingDays = request.OperatingDays,
                IntermediateStops = BuildIntermediateStopsText(request, stations),
                IsActive = request.IsActive
            };

            if (await _db.TrainRoutes.AnyAsync(r => r.Code == route.Code))
                return Conflict($"Route code '{route.Code}' already exists.");

            AddRouteStops(route, request, stations);
            _db.TrainRoutes.Add(route);
            await _db.SaveChangesAsync();

            route = await LoadRouteAsync(route.Id);
            return CreatedAtAction(nameof(GetById), new { id = route.Id }, ToDto(route));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminRouteDto>> GetById(int id)
        {
            var route = await LoadRouteAsync(id);
            return Ok(ToDto(route));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdminRouteDto>> Update(int id, AdminRouteDto request)
        {
            var route = await _db.TrainRoutes.FindAsync(id)
                ?? throw new KeyNotFoundException("Route not found");

            var stations = await LoadRouteStationsAsync(request);
            var departureStation = stations[request.DepartureStationId];
            var arrivalStation = stations[request.ArrivalStationId];

            route.Code = BuildRouteCode(request, departureStation, arrivalStation);
            route.Name = BuildRouteName(request, departureStation, arrivalStation);
            route.AdminDisplayName = BuildAdminDisplayName(request, departureStation, arrivalStation, stations);
            route.RouteFingerprint = BuildRouteFingerprint(request, departureStation, arrivalStation, stations);
            route.DepartureStationId = request.DepartureStationId;
            route.ArrivalStationId = request.ArrivalStationId;
            route.DistanceKm = request.DistanceKm;
            route.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
            route.OperatingDays = request.OperatingDays;
            route.IntermediateStops = BuildIntermediateStopsText(request, stations);
            route.IsActive = request.IsActive;

            if (await _db.TrainRoutes.AnyAsync(r => r.Id != id && r.Code == route.Code))
                return Conflict($"Route code '{route.Code}' already exists.");

            var existingStops = await _db.TrainRouteStops
                .Where(s => s.TrainRouteId == route.Id)
                .ToListAsync();
            _db.TrainRouteStops.RemoveRange(existingStops);

            await _db.SaveChangesAsync();

            AddRouteStops(route, request, stations);

            await _db.SaveChangesAsync();
            return Ok(ToDto(await LoadRouteAsync(route.Id)));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var route = await _db.TrainRoutes.FindAsync(id)
                ?? throw new KeyNotFoundException("Route not found");

            if (await _db.Trips.AnyAsync(t => t.TrainRouteId == id))
                return BadRequest("Cannot delete a route with schedules");

            _db.TrainRoutes.Remove(route);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<Dictionary<int, Station>> LoadRouteStationsAsync(AdminRouteDto request)
        {
            if (request.DepartureStationId == request.ArrivalStationId)
                throw new InvalidOperationException("Departure and arrival stations must be different");

            var requestedStationIds = request.IntermediateStopStationIds
                .Prepend(request.DepartureStationId)
                .Append(request.ArrivalStationId)
                .Distinct()
                .ToList();

            if (request.IntermediateStopStationIds.Any(id => id == request.DepartureStationId || id == request.ArrivalStationId))
                throw new InvalidOperationException("Intermediate stops cannot include the origin or destination station");

            if (request.IntermediateStopStationIds.Count != request.IntermediateStopStationIds.Distinct().Count())
                throw new InvalidOperationException("Intermediate stops cannot contain duplicates");

            var stations = await _db.Stations
                .Where(s => requestedStationIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            if (stations.Count != requestedStationIds.Count)
                throw new KeyNotFoundException("Departure or arrival station not found");

            return stations;
        }

        private async Task<TrainRoute> LoadRouteAsync(int id)
        {
            return await _db.TrainRoutes
                .Include(r => r.DepartureStation)
                .Include(r => r.ArrivalStation)
                .Include(r => r.RouteStops)
                    .ThenInclude(s => s.Station)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Route not found");
        }

        private static AdminRouteDto ToDto(TrainRoute route) => new()
        {
            Id = route.Id,
            Code = route.Code,
            Name = route.Name,
            AdminDisplayName = string.IsNullOrWhiteSpace(route.AdminDisplayName)
                ? route.Name
                : route.AdminDisplayName,
            RouteFingerprint = route.RouteFingerprint,
            DepartureStationId = route.DepartureStationId,
            ArrivalStationId = route.ArrivalStationId,
            DepartureStationName = route.DepartureStation.Name,
            ArrivalStationName = route.ArrivalStation.Name,
            DistanceKm = route.DistanceKm,
            EstimatedDurationMinutes = route.EstimatedDurationMinutes,
            OperatingDays = route.OperatingDays,
            IntermediateStops = route.IntermediateStops,
            IntermediateStopStationIds = route.RouteStops
                .OrderBy(s => s.StopOrder)
                .Select(s => s.StationId)
                .ToList(),
            Stops = route.RouteStops
                .OrderBy(s => s.StopOrder)
                .Select(s => new AdminRouteStopDto
                {
                    StationId = s.StationId,
                    StationCode = s.Station.Code,
                    StationName = s.Station.Name,
                    StopOrder = s.StopOrder,
                    ArrivalOffsetMinutes = s.ArrivalOffsetMinutes,
                    DepartureOffsetMinutes = s.DepartureOffsetMinutes,
                    Platform = s.Platform,
                    Track = s.Track,
                    StopType = s.StopType
                })
                .ToList(),
            IsActive = route.IsActive
        };

        private static string BuildRouteCode(AdminRouteDto request, Station departureStation, Station arrivalStation)
        {
            var requestedCode = request.Code.Trim();
            return string.IsNullOrWhiteSpace(requestedCode)
                ? $"{departureStation.Code}-{arrivalStation.Code}"
                : requestedCode;
        }

        private static string BuildRouteName(AdminRouteDto request, Station departureStation, Station arrivalStation)
        {
            var requestedName = request.Name.Trim();
            return string.IsNullOrWhiteSpace(requestedName)
                ? $"{departureStation.Name} to {arrivalStation.Name}"
                : requestedName;
        }

        private static string BuildRouteFingerprint(
            AdminRouteDto request,
            Station departureStation,
            Station arrivalStation,
            IReadOnlyDictionary<int, Station> stations)
        {
            var orderedStationCodes = request.IntermediateStopStationIds
                .Select(id => stations[id].Code)
                .Prepend(departureStation.Code)
                .Append(arrivalStation.Code)
                .Select(code => code.Trim().ToUpperInvariant());

            return string.Join(">", orderedStationCodes);
        }

        private static string BuildAdminDisplayName(
            AdminRouteDto request,
            Station departureStation,
            Station arrivalStation,
            IReadOnlyDictionary<int, Station> stations)
        {
            var requestedName = request.AdminDisplayName.Trim();
            if (!string.IsNullOrWhiteSpace(requestedName))
                return requestedName;

            var keyStops = request.IntermediateStopStationIds
                .Select(id => stations[id].Name)
                .Take(3)
                .ToList();

            return keyStops.Count == 0
                ? $"{departureStation.Name} to {arrivalStation.Name}"
                : $"{departureStation.Name} to {arrivalStation.Name} via {string.Join(", ", keyStops)}";
        }

        private static string BuildIntermediateStopsText(AdminRouteDto request, Dictionary<int, Station> stations)
        {
            if (request.IntermediateStopStationIds.Count > 0)
            {
                return string.Join(
                    Environment.NewLine,
                    request.IntermediateStopStationIds.Select(id => stations[id].Name));
            }

            return request.IntermediateStops.Trim();
        }

        private static void AddRouteStops(
            TrainRoute route,
            AdminRouteDto request,
            IReadOnlyDictionary<int, Station> stations)
        {
            var intermediateStopStationIds = request.IntermediateStopStationIds;
            for (var i = 0; i < intermediateStopStationIds.Count; i++)
            {
                var station = stations[intermediateStopStationIds[i]];
                var stopOrder = i + 1;
                var totalSegments = Math.Max(1, intermediateStopStationIds.Count + 1);
                var evenOffset = (int)Math.Round(request.EstimatedDurationMinutes * stopOrder / (double)totalSegments);
                var stopType = TripTimetablePlanner.GetStopType(station, isTerminus: false);
                var dwellMinutes = TripTimetablePlanner.GetDwellMinutes(stopType);
                var arrivalOffset = Math.Max(0, evenOffset - dwellMinutes / 2);

                route.RouteStops.Add(new TrainRouteStop
                {
                    StationId = station.Id,
                    StopOrder = stopOrder,
                    ArrivalOffsetMinutes = arrivalOffset,
                    DepartureOffsetMinutes = arrivalOffset + dwellMinutes,
                    Platform = ((station.Id + stopOrder) % 4 + 1).ToString(),
                    Track = ((station.Id + stopOrder + 1) % 6 + 1).ToString(),
                    StopType = stopType
                });
            }
        }
    }
}
