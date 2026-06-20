using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/trains")]
    public class AdminTrainsController : ControllerBase
    {
        private readonly ITrainService _trainService;

        public AdminTrainsController(ITrainService trainService)
        {
            _trainService = trainService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminTrainDto>>> GetAll()
        {
            var trains = await _trainService.GetAllTrainsAsync();
            return Ok(trains.Select(ToDto));
        }

        [HttpPost]
        public async Task<ActionResult<AdminTrainDto>> Create([FromBody] AdminTrainDto request)
        {
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

            var created = await _trainService.CreateTrainAsync(train);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminTrainDto>> GetById(int id)
        {
            try
            {
                var train = await _trainService.GetTrainByIdAsync(id);
                return Ok(ToDto(train));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdminTrainDto>> Update(int id, [FromBody] AdminTrainDto request)
        {
            var train = new Train
            {
                Id = id,
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

            try
            {
                var updated = await _trainService.UpdateTrainAsync(train);
                return Ok(ToDto(updated));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
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
            CarriageCount = train.CarriageCount,
            SeatsPerCarriage = train.SeatsPerCarriage,
            Status = train.Status,
            DepartureStation = train.DepartureStation,
            ArrivalStation = train.ArrivalStation,
            DepartureTime = train.DepartureTime,
            ArrivalTime = train.ArrivalTime
        };
    }
}
