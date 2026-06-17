using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TripsController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripsController(ITripService tripService)
        {
            _tripService = tripService;
        }

        // GET: api/Trips/search?from=WAW&to=KRK&date=2026-07-01
        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TripSearchResultDto>>> Search(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateTime date)
        {
            try
            {
                var trips = await _tripService.SearchTripsAsync(from, to, date);
                return Ok(trips);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Trips/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<TripDetailsDto>> GetById(int id)
        {
            try
            {
                var trip = await _tripService.GetTripByIdAsync(id);
                return Ok(trip);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: api/Trips/5/seats
        [AllowAnonymous]
        [HttpGet("{id}/seats")]
        public async Task<ActionResult<IEnumerable<TripSeatAvailabilityDto>>> GetSeats(int id)
        {
            try
            {
                var seats = await _tripService.GetSeatAvailabilityAsync(id);
                return Ok(seats);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
