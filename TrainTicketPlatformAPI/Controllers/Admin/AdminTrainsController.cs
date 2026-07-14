using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Security;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/trains")]
    public class AdminTrainsController : ControllerBase
    {
        private const string LocomotiveLayoutType = "Locomotive";
        private const string RestaurantLayoutType = "Restaurant";

        private readonly TrainTicketDbContext _db;
        private readonly ITrainService _trainService;

        public AdminTrainsController(TrainTicketDbContext db, ITrainService trainService)
        {
            _db = db;
            _trainService = trainService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminTrainDto>>> GetAll()
        {
            var trains = await _db.Trains
                .Include(t => t.Carriages)
                .OrderBy(t => t.Code)
                .ToListAsync();

            return Ok(trains.Select(ToDto));
        }

        [HttpGet("rolling-stock-options")]
        public async Task<ActionResult<IEnumerable<AdminRollingStockOptionDto>>> GetRollingStockOptions()
        {
            var options = await _db.RollingStockOptions
                .OrderBy(o => o.Category)
                .ThenBy(o => o.Series)
                .Select(o => new AdminRollingStockOptionDto
                {
                    Id = o.Id,
                    Category = o.Category,
                    Series = o.Series,
                    DisplayName = o.DisplayName,
                    Manufacturer = o.Manufacturer,
                    MaxSpeed = o.MaxSpeed,
                    FleetCount = o.FleetCount,
                    UnitCount = o.UnitCount,
                    Notes = o.Notes,
                    Status = o.Status
                })
                .ToListAsync();

            return Ok(options);
        }

        [HttpPost]
        public async Task<ActionResult<AdminTrainDto>> Create([FromBody] AdminTrainDto request)
        {
            if (await _db.Trains.AnyAsync(t => t.Code == request.Code.Trim()))
                return Conflict("A train with this code already exists.");

            var train = new Train
            {
                Code = request.Code.Trim(),
                Name = request.Name,
                Type = request.Type,
                CarriageCount = request.CarriageCount,
                SeatsPerCarriage = request.SeatsPerCarriage,
                Status = request.Status,
                DepartureStation = request.DepartureStation,
                ArrivalStation = request.ArrivalStation,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime
            };

            train.Carriages = BuildCarriages(request);
            ApplyCapacitySummary(train);

            _db.Trains.Add(train);
            await _db.SaveChangesAsync();

            await SyncSeatsForTrainAsync(train);

            var created = await LoadTrainAsync(train.Id) ?? train;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminTrainDto>> GetById(int id)
        {
            var train = await LoadTrainAsync(id);
            return train == null ? NotFound() : Ok(ToDto(train));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdminTrainDto>> Update(int id, [FromBody] AdminTrainDto request)
        {
            var train = await LoadTrainAsync(id);
            if (train == null)
                return NotFound();

            var code = request.Code.Trim();
            if (await _db.Trains.AnyAsync(t => t.Id != id && t.Code == code))
                return Conflict("A train with this code already exists.");

            var hasBookings = await _db.Bookings.AnyAsync(b => b.TrainId == id);
            if (hasBookings)
                return BadRequest("Cannot change the consist for a train that already has bookings.");

            train.Code = code;
            train.Name = request.Name;
            train.Type = request.Type;
            train.Status = request.Status;
            train.DepartureStation = request.DepartureStation;
            train.ArrivalStation = request.ArrivalStation;
            train.DepartureTime = request.DepartureTime;
            train.ArrivalTime = request.ArrivalTime;

            _db.TrainCarriages.RemoveRange(train.Carriages);
            train.Carriages = BuildCarriages(request);
            ApplyCapacitySummary(train);

            await _db.SaveChangesAsync();
            await SyncSeatsForTrainAsync(train);

            var updated = await LoadTrainAsync(id);
            return Ok(ToDto(updated!));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (DangerousActionGuard.RequireHeader(this, DangerousActionGuard.Delete) is { } headerError)
                return headerError;

            try
            {
                await _trainService.DeleteTrainAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static AdminTrainDto ToDto(Train train) => new()
        {
            Id = train.Id,
            Code = train.Code,
            Name = train.Name,
            Type = train.Type,
            Locomotive = train.Carriages
                .OrderBy(c => c.Position)
                .FirstOrDefault(c => c.LayoutType == LocomotiveLayoutType)?.VehicleType ?? string.Empty,
            CarriageCount = train.CarriageCount,
            SeatsPerCarriage = train.SeatsPerCarriage,
            Status = train.Status,
            DepartureStation = train.DepartureStation,
            ArrivalStation = train.ArrivalStation,
            DepartureTime = train.DepartureTime,
            ArrivalTime = train.ArrivalTime,
            Carriages = train.Carriages
                .Where(c => c.LayoutType != LocomotiveLayoutType)
                .OrderBy(c => c.Position)
                .ThenBy(c => c.Coach)
                .Select(c => new AdminTrainCarriageDto
                {
                    Id = c.Id,
                    Coach = c.Coach,
                    Position = c.Position,
                    ClassType = c.ClassType,
                    LayoutType = c.LayoutType,
                    VehicleType = c.VehicleType,
                    SeatCount = c.SeatCount,
                    HasBikeSpace = c.HasBikeSpace,
                    HasAccessibleSpace = c.HasAccessibleSpace,
                    HasFamilyCompartment = c.HasFamilyCompartment,
                    HasDiningSection = c.HasDiningSection,
                    Notes = c.Notes
                })
                .ToList()
        };

        private async Task<Train?> LoadTrainAsync(int id) =>
            await _db.Trains
                .Include(t => t.Carriages)
                .FirstOrDefaultAsync(t => t.Id == id);

        private static List<TrainCarriage> BuildCarriages(AdminTrainDto request)
        {
            var carriages = new List<TrainCarriage>();
            var locomotive = request.Locomotive.Trim();
            if (!string.IsNullOrWhiteSpace(locomotive))
            {
                carriages.Add(new TrainCarriage
                {
                    Coach = "Loco",
                    Position = 0,
                    ClassType = LocomotiveLayoutType,
                    LayoutType = LocomotiveLayoutType,
                    VehicleType = locomotive,
                    SeatCount = 0,
                    Notes = "Locomotive"
                });
            }

            carriages.AddRange(request.Carriages
                .OrderBy(c => c.Position)
                .Select((c, index) => new TrainCarriage
                {
                    Coach = c.Coach.Trim(),
                    Position = index + 1,
                    ClassType = c.ClassType,
                    LayoutType = c.LayoutType,
                    VehicleType = c.VehicleType,
                    SeatCount = c.HasDiningSection || c.LayoutType == RestaurantLayoutType ? 0 : c.SeatCount,
                    HasBikeSpace = c.HasBikeSpace,
                    HasAccessibleSpace = c.HasAccessibleSpace,
                    HasFamilyCompartment = c.HasFamilyCompartment,
                    HasDiningSection = c.HasDiningSection || c.LayoutType == RestaurantLayoutType,
                    Notes = c.Notes
                }));

            return carriages;
        }

        private static void ApplyCapacitySummary(Train train)
        {
            var passengerCarriages = train.Carriages.Where(IsPassengerCarriage).ToList();
            train.CarriageCount = train.Carriages.Count(c => c.LayoutType != LocomotiveLayoutType);
            train.SeatsPerCarriage = passengerCarriages.Count == 0
                ? 0
                : passengerCarriages.Max(c => c.SeatCount);
        }

        private async Task SyncSeatsForTrainAsync(Train train)
        {
            var existingSeats = await _db.Seats.Where(s => s.TrainId == train.Id).ToListAsync();
            _db.Seats.RemoveRange(existingSeats);

            var seats = train.Carriages
                .Where(IsPassengerCarriage)
                .SelectMany(carriage => GetSeatNumbersForCarriage(carriage).Select(number => new Seat
                {
                    TrainId = train.Id,
                    Coach = carriage.Coach,
                    Number = number,
                    ClassType = GetSeatClassType(carriage, int.Parse(number)),
                    IsAvailable = true
                }));

            _db.Seats.AddRange(seats);
            await _db.SaveChangesAsync();
        }

        private static bool IsPassengerCarriage(TrainCarriage carriage) =>
            carriage.LayoutType != LocomotiveLayoutType &&
            carriage.LayoutType != RestaurantLayoutType &&
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
    }
}
