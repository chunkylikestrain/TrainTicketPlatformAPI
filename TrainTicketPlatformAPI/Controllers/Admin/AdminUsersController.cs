using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminUsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users.Select(ToResponse));
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
