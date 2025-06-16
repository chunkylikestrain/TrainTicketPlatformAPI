using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;
namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainsController : ControllerBase
    {
        private readonly ITrainService _trainService;
        public TrainsController(ITrainService trainService)
            => _trainService = trainService;

        // GET: api/Trains/search?from=A&to=B&date=2025-06-20
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Train>>> Search(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateTime date)
        {
            var list = await _trainService.SearchTrainsAsync(from, to, date);
            return Ok(list);
        }

        // GET: api/Trains
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Train>>> GetAll()
            => Ok(await _trainService.GetAllTrainsAsync());

        // GET: api/Trains/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Train>> GetById(int id)
        {
            try
            {
                var train = await _trainService.GetTrainByIdAsync(id);
                return Ok(train);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/Trains
        [HttpPost]
        public async Task<ActionResult<Train>> Create(Train train)
        {
            var created = await _trainService.CreateTrainAsync(train);
            return CreatedAtAction(nameof(GetById),
                                   new { id = created.Id },
                                   created);
        }

        // PUT: api/Trains/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Train train)
        {
            if (id != train.Id) return BadRequest("ID mismatch");
            try
            {
                var updated = await _trainService.UpdateTrainAsync(train);
                return Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/Trains/5
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
    }
}
