using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Contracts.Common;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Security;
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
        public async Task<ActionResult<PagedResponse<UserResponseDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            [FromQuery] string? search = null,
            [FromQuery] string? role = null)
        {
            var users = await _userService.GetAllUsersAsync();
            var query = users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();
                query = query.Where(u =>
                    u.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                    u.Phone.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var normalizedRole = role.Trim();
                query = query.Where(u =>
                    u.Role.Equals(normalizedRole, StringComparison.OrdinalIgnoreCase));
            }

            var response = ToPagedResponse(
                query.OrderBy(u => u.Id).Select(ToResponse),
                page,
                pageSize);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> Update(int id, [FromBody] AdminUserUpdateRequest request)
        {
            try
            {
                var updated = await _userService.UpdateUserAsync(new User
                {
                    Id = id,
                    Email = request.Email,
                    Phone = request.Phone,
                    Role = request.Role,
                    DisplayName = request.DisplayName,
                    Status = request.Status
                });

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (DangerousActionGuard.RequireHeader(this, DangerousActionGuard.Delete) is { } headerError)
                return headerError;

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

        private static PagedResponse<T> ToPagedResponse<T>(
            IEnumerable<T> source,
            int page,
            int pageSize)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var totalCount = source.Count();
            var items = source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResponse<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        private static UserResponseDto ToResponse(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role,
            DisplayName = user.DisplayName,
            Status = user.Status
        };
    }
}
