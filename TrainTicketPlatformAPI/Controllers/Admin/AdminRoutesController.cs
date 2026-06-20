using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

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
                .OrderBy(r => r.Code)
                .ToListAsync();

            return Ok(routes.Select(ToDto));
        }

        [HttpPost]
        public async Task<ActionResult<AdminRouteDto>> Create(AdminRouteDto request)
        {
            await EnsureStationsExistAsync(request.DepartureStationId, request.ArrivalStationId);

            var route = new TrainRoute
            {
                Code = request.Code.Trim(),
                DepartureStationId = request.DepartureStationId,
                ArrivalStationId = request.ArrivalStationId,
                DistanceKm = request.DistanceKm,
                EstimatedDurationMinutes = request.EstimatedDurationMinutes,
                OperatingDays = request.OperatingDays,
                IntermediateStops = request.IntermediateStops,
                IsActive = request.IsActive
            };

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

            await EnsureStationsExistAsync(request.DepartureStationId, request.ArrivalStationId);

            route.Code = request.Code.Trim();
            route.DepartureStationId = request.DepartureStationId;
            route.ArrivalStationId = request.ArrivalStationId;
            route.DistanceKm = request.DistanceKm;
            route.EstimatedDurationMinutes = request.EstimatedDurationMinutes;
            route.OperatingDays = request.OperatingDays;
            route.IntermediateStops = request.IntermediateStops;
            route.IsActive = request.IsActive;

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

        private async Task EnsureStationsExistAsync(int departureStationId, int arrivalStationId)
        {
            var count = await _db.Stations.CountAsync(s => s.Id == departureStationId || s.Id == arrivalStationId);
            if (count < 2)
                throw new KeyNotFoundException("Departure or arrival station not found");
        }

        private async Task<TrainRoute> LoadRouteAsync(int id)
        {
            return await _db.TrainRoutes
                .Include(r => r.DepartureStation)
                .Include(r => r.ArrivalStation)
                .FirstOrDefaultAsync(r => r.Id == id)
                ?? throw new KeyNotFoundException("Route not found");
        }

        private static AdminRouteDto ToDto(TrainRoute route) => new()
        {
            Id = route.Id,
            Code = route.Code,
            DepartureStationId = route.DepartureStationId,
            ArrivalStationId = route.ArrivalStationId,
            DepartureStationName = route.DepartureStation.Name,
            ArrivalStationName = route.ArrivalStation.Name,
            DistanceKm = route.DistanceKm,
            EstimatedDurationMinutes = route.EstimatedDurationMinutes,
            OperatingDays = route.OperatingDays,
            IntermediateStops = route.IntermediateStops,
            IsActive = route.IsActive
        };
    }
}
