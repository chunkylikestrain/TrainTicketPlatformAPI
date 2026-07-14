using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Stations;
using TrainTicketPlatformAPI.Security;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly IStationService _stationService;

        public StationsController(IStationService stationService)
        {
            _stationService = stationService;
        }

        // GET: api/Stations?query=Warsaw
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicyNames.PublicSearch)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StationDto>>> GetAll([FromQuery] string? query)
        {
            var stations = await _stationService.GetStationsAsync(query);
            return Ok(stations);
        }

        // GET: api/Stations/5
        [AllowAnonymous]
        [EnableRateLimiting(RateLimitPolicyNames.PublicRead)]
        [HttpGet("{id}")]
        public async Task<ActionResult<StationDto>> GetById(int id)
        {
            try
            {
                var station = await _stationService.GetStationByIdAsync(id);
                return Ok(station);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
