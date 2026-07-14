using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Security;
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
        [EnableRateLimiting(RateLimitPolicyNames.PublicSearch)]
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TripSearchResultDto>>> Search(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan? time)
        {
            try
            {
                var trips = await _tripService.SearchTripsAsync(from, to, date, time);
                return Ok(trips);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Trips/itineraries?from=WAW&to=GDN&date=2026-07-01
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicyNames.PublicSearch)]
        [HttpGet("itineraries")]
        public async Task<ActionResult<IEnumerable<TripItinerarySearchResultDto>>> SearchItineraries(
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] DateTime date,
            [FromQuery] TimeSpan? time)
        {
            try
            {
                var itineraries = await _tripService.SearchItinerariesAsync(from, to, date, time);
                return Ok(itineraries);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Trips/5
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicyNames.PublicRead)]
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
        [EnableRateLimiting(RateLimitPolicyNames.PublicRead)]
        [HttpGet("{id}/seats")]
        public async Task<ActionResult<IEnumerable<TripSeatAvailabilityDto>>> GetSeats(
            int id,
            [FromQuery] int? fromStationId,
            [FromQuery] int? toStationId)
        {
            try
            {
                var seats = await _tripService.GetSeatAvailabilityAsync(id, fromStationId, toStationId);
                return Ok(seats);
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
