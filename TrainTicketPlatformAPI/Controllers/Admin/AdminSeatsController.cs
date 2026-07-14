using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Security;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/seats")]
    public class AdminSeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;

        public AdminSeatsController(ISeatService seatService)
        {
            _seatService = seatService;
        }

        [HttpPost]
        public async Task<ActionResult<AdminSeatDto>> Create([FromBody] AdminSeatDto request)
        {
            try
            {
                var seat = new Seat
                {
                    TrainId = request.TrainId,
                    Coach = request.Coach,
                    Number = request.Number,
                    ClassType = request.ClassType,
                    IsAvailable = request.IsAvailable
                };

                var created = await _seatService.CreateSeatAsync(seat);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDto(created));
            }
            catch (KeyNotFoundException)
            {
                return BadRequest("Train not found");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AdminSeatDto>> GetById(int id)
        {
            try
            {
                var seat = await _seatService.GetSeatByIdAsync(id);
                return Ok(ToDto(seat));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdminSeatDto>> Update(int id, [FromBody] AdminSeatDto request)
        {
            var seat = new Seat
            {
                Id = id,
                TrainId = request.TrainId,
                Coach = request.Coach,
                Number = request.Number,
                ClassType = request.ClassType,
                IsAvailable = request.IsAvailable
            };

            try
            {
                var updated = await _seatService.UpdateSeatAsync(seat);
                return Ok(ToDto(updated));
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (DangerousActionGuard.RequireHeader(this, DangerousActionGuard.Delete) is { } headerError)
                return headerError;

            try
            {
                await _seatService.DeleteSeatAsync(id);
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

        private static AdminSeatDto ToDto(Seat seat) => new()
        {
            Id = seat.Id,
            TrainId = seat.TrainId,
            Coach = seat.Coach,
            Number = seat.Number,
            ClassType = seat.ClassType,
            IsAvailable = seat.IsAvailable
        };
    }
}
