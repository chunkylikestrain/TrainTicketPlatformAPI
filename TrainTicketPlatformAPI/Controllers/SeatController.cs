using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;
        public SeatsController(ISeatService seatService)
            => _seatService = seatService;

        // GET: api/Seats
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seat>>> GetAll()
            => Ok(await _seatService.GetAllSeatsAsync());

        // GET: api/Seats/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Seat>> GetById(int id)
        {
            try
            {
                var seat = await _seatService.GetSeatByIdAsync(id);
                return Ok(seat);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Seats/train/3
        [HttpGet("train/{trainId}")]
        public async Task<ActionResult<IEnumerable<Seat>>> GetByTrain(int trainId)
            => Ok(await _seatService.GetSeatsByTrainAsync(trainId));

        // POST: api/Seats
        [HttpPost]
        public async Task<ActionResult<Seat>> Create(Seat seat)
        {
            try
            {
                var created = await _seatService.CreateSeatAsync(seat);
                return CreatedAtAction(nameof(GetById),
                                       new { id = created.Id },
                                       created);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest("Train not found");
            }
        }

        // PUT: api/Seats/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Seat seat)
        {
            if (id != seat.Id)
                return BadRequest("ID mismatch");

            try
            {
                var updated = await _seatService.UpdateSeatAsync(seat);
                return Ok(updated);
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

        // DELETE: api/Seats/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
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
    }
}
