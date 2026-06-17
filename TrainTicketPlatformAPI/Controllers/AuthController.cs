using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Contracts.Auth;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
            => _userService = userService;

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var user = await _userService.RegisterAsync(dto);
                return CreatedAtAction(null, new { id = user.Id }, null);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _userService.LoginAsync(dto);
                return Ok(response);
            }
            catch (KeyNotFoundException)
            {
                return Unauthorized("Invalid credentials");
            }
        }

        // GET: api/Auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserDto>> Me()
        {
            var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(subject, out var userId))
                return Unauthorized();

            try
            {
                var user = await _userService.GetUserByIdAsync(userId);
                return Ok(new CurrentUserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
