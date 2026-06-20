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
    [Route("api/admin/schedules")]
    public class AdminSchedulesController : ControllerBase
    {
        private readonly TrainTicketDbContext _db;

        public AdminSchedulesController(TrainTicketDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminTripDto>>> GetAll()
        {
            var trips = await _db.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .Include(t => t.TrainRoute).ThenInclude(r => r.DepartureStation)
                .Include(t => t.TrainRoute).ThenInclude(r => r.ArrivalStation)
                .Include(t => t.Fares)
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();

            return Ok(trips.Select(ToDto));
        }

        [HttpPost]
        public async Task<ActionResult<AdminTripDto>> Create(AdminTripDto request)
        {
            if (!await _db.Trains.AnyAsync(t => t.Id == request.TrainId))
                throw new KeyNotFoundException("Train not found");

            if (!await _db.TrainRoutes.AnyAsync(r => r.Id == request.TrainRouteId))
                throw new KeyNotFoundException("Route not found");

            var trip = new Trip
            {
                TrainId = request.TrainId,
                TrainRouteId = request.TrainRouteId,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                Platform = request.Platform,
                Track = request.Track,
                Status = request.Status
            };

            trip.Fares.Add(new Fare { ClassType = "Class 1", Price = request.Class1Price, Currency = "PLN" });
            trip.Fares.Add(new Fare { ClassType = "Class 2", Price = request.Class2Price, Currency = "PLN" });

            _db.Trips.Add(trip);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = trip.Id }, ToDto(await LoadTripAsync(trip.Id)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminTripDto>> GetById(int id)
        {
            return Ok(ToDto(await LoadTripAsync(id)));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdminTripDto>> Update(int id, AdminTripDto request)
        {
            var trip = await _db.Trips.Include(t => t.Fares).FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException("Schedule not found");

            trip.TrainId = request.TrainId;
            trip.TrainRouteId = request.TrainRouteId;
            trip.DepartureTime = request.DepartureTime;
            trip.ArrivalTime = request.ArrivalTime;
            trip.Platform = request.Platform;
            trip.Track = request.Track;
            trip.Status = request.Status;

            UpsertFare(trip, "Class 1", request.Class1Price);
            UpsertFare(trip, "Class 2", request.Class2Price);

            await _db.SaveChangesAsync();
            return Ok(ToDto(await LoadTripAsync(id)));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var trip = await _db.Trips.FindAsync(id)
                ?? throw new KeyNotFoundException("Schedule not found");

            if (await _db.Bookings.AnyAsync(b => b.TripId == id))
                return BadRequest("Cannot delete a schedule with bookings");

            _db.Trips.Remove(trip);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static void UpsertFare(Trip trip, string classType, decimal price)
        {
            var fare = trip.Fares.FirstOrDefault(f => f.ClassType == classType);
            if (fare == null)
                trip.Fares.Add(new Fare { ClassType = classType, Price = price, Currency = "PLN" });
            else
                fare.Price = price;
        }

        private async Task<Trip> LoadTripAsync(int id)
        {
            return await _db.Trips
                .Include(t => t.Train)
                .Include(t => t.TrainRoute).ThenInclude(r => r.DepartureStation)
                .Include(t => t.TrainRoute).ThenInclude(r => r.ArrivalStation)
                .Include(t => t.Fares)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new KeyNotFoundException("Schedule not found");
        }

        private static AdminTripDto ToDto(Trip trip)
        {
            var class1 = trip.Fares.FirstOrDefault(f => f.ClassType == "Class 1")?.Price ?? 0;
            var class2 = trip.Fares.FirstOrDefault(f => f.ClassType == "Class 2")?.Price ?? 0;
            var route = $"{trip.TrainRoute.DepartureStation.Name} -> {trip.TrainRoute.ArrivalStation.Name}";

            return new AdminTripDto
            {
                Id = trip.Id,
                TrainId = trip.TrainId,
                TrainCode = string.IsNullOrWhiteSpace(trip.Train.Code) ? trip.Train.Name : trip.Train.Code,
                TrainRouteId = trip.TrainRouteId,
                RouteCode = trip.TrainRoute.Code,
                Route = route,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                Platform = trip.Platform,
                Track = trip.Track,
                Status = trip.Status,
                Class1Price = class1,
                Class2Price = class2
            };
        }
    }
}
