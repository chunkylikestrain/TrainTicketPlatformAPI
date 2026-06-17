using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
            => _userService = userService;

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users.Select(ToResponse));
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                return Ok(ToResponse(user));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> Create(User user)
        {
            try
            {
                var created = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetById),
                                       new { id = created.Id },
                                       ToResponse(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, User user)
        {
            if (id != user.Id)
                return BadRequest("ID mismatch");

            try
            {
                var updated = await _userService.UpdateUserAsync(user);
                return Ok(ToResponse(updated));
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

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
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

        private static UserResponseDto ToResponse(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role
        };
    }
}
