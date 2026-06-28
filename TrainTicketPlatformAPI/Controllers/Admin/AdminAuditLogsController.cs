using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Admin;
using TrainTicketPlatformAPI.Contracts.Common;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/audit-logs")]
    public class AdminAuditLogsController : ControllerBase
    {
        private readonly TrainTicketDbContext _db;

        public AdminAuditLogsController(TrainTicketDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResponse<AdminAuditLogDto>>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] string? entityType = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _db.AdminAuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim();
                query = query.Where(log =>
                    log.AdminEmail.Contains(normalized) ||
                    log.Action.Contains(normalized) ||
                    log.EntityType.Contains(normalized) ||
                    log.EntityId.Contains(normalized) ||
                    log.Summary.Contains(normalized) ||
                    log.Path.Contains(normalized));
            }

            if (!string.IsNullOrWhiteSpace(entityType))
            {
                var normalizedEntityType = entityType.Trim();
                query = query.Where(log => log.EntityType == normalizedEntityType);
            }

            if (from.HasValue)
                query = query.Where(log => log.CreatedAtUtc >= from.Value);

            if (to.HasValue)
                query = query.Where(log => log.CreatedAtUtc <= to.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(log => log.CreatedAtUtc)
                .ThenByDescending(log => log.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(log => ToDto(log))
                .ToListAsync();

            return Ok(new PagedResponse<AdminAuditLogDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        private static AdminAuditLogDto ToDto(AdminAuditLog log) => new()
        {
            Id = log.Id,
            CreatedAtUtc = log.CreatedAtUtc,
            AdminUserId = log.AdminUserId,
            AdminEmail = log.AdminEmail,
            Action = log.Action,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            HttpMethod = log.HttpMethod,
            Path = log.Path,
            StatusCode = log.StatusCode,
            Summary = log.Summary,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent
        };
    }
}
