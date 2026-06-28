using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Middleware
{
    public class AdminAuditMiddleware
    {
        private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Post,
            HttpMethods.Put,
            HttpMethods.Patch,
            HttpMethods.Delete
        };

        private readonly RequestDelegate _next;

        public AdminAuditMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var shouldAudit = ShouldAudit(context);

            await _next(context);

            if (!shouldAudit || context.Response.StatusCode >= StatusCodes.Status400BadRequest)
                return;

            var auditService = context.RequestServices.GetRequiredService<IAdminAuditService>();
            var path = context.Request.Path.Value ?? string.Empty;
            var route = ParseAdminRoute(path);
            var user = context.User;

            var log = new AdminAuditLog
            {
                CreatedAtUtc = DateTime.UtcNow,
                AdminUserId = TryGetAdminUserId(user),
                AdminEmail = GetAdminEmail(user),
                Action = BuildAction(context.Request.Method, route.EntityType, route.TailAction),
                EntityType = route.EntityType,
                EntityId = route.EntityId,
                HttpMethod = context.Request.Method.ToUpperInvariant(),
                Path = path,
                StatusCode = context.Response.StatusCode,
                Summary = BuildSummary(context.Request.Method, route.EntityType, route.EntityId, route.TailAction),
                IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
                UserAgent = context.Request.Headers.UserAgent.ToString()
            };

            await auditService.RecordAsync(log, context.RequestAborted);
        }

        private static bool ShouldAudit(HttpContext context)
        {
            var path = context.Request.Path;
            return context.User.IsInRole("Admin") &&
                path.StartsWithSegments("/api/admin", StringComparison.OrdinalIgnoreCase) &&
                !path.StartsWithSegments("/api/admin/audit-logs", StringComparison.OrdinalIgnoreCase) &&
                MutatingMethods.Contains(context.Request.Method);
        }

        private static int? TryGetAdminUserId(ClaimsPrincipal user)
        {
            var subject = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(subject, out var userId) ? userId : null;
        }

        private static string GetAdminEmail(ClaimsPrincipal user)
            => user.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? user.FindFirstValue(ClaimTypes.Email)
                ?? "admin";

        private static (string EntityType, string EntityId, string TailAction) ParseAdminRoute(string path)
        {
            var segments = path
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            var adminIndex = segments.FindIndex(segment => segment.Equals("admin", StringComparison.OrdinalIgnoreCase));
            var entityType = adminIndex >= 0 && segments.Count > adminIndex + 1
                ? segments[adminIndex + 1]
                : "admin";

            var remaining = adminIndex >= 0
                ? segments.Skip(adminIndex + 2).ToList()
                : [];

            var entityId = remaining.FirstOrDefault(segment => segment.All(char.IsDigit)) ?? string.Empty;
            var tailAction = remaining.LastOrDefault(segment => !segment.All(char.IsDigit)) ?? string.Empty;

            return (entityType, entityId, tailAction);
        }

        private static string BuildAction(string method, string entityType, string tailAction)
        {
            if (!string.IsNullOrWhiteSpace(tailAction))
                return ToTitle($"{tailAction} {entityType}");

            return method.ToUpperInvariant() switch
            {
                "POST" => ToTitle($"create {entityType}"),
                "PUT" => ToTitle($"update {entityType}"),
                "PATCH" => ToTitle($"update {entityType}"),
                "DELETE" => ToTitle($"delete {entityType}"),
                _ => ToTitle($"{method} {entityType}")
            };
        }

        private static string BuildSummary(string method, string entityType, string entityId, string tailAction)
        {
            var target = string.IsNullOrWhiteSpace(entityId)
                ? entityType
                : $"{entityType} #{entityId}";

            var action = !string.IsNullOrWhiteSpace(tailAction)
                ? tailAction.Replace('-', ' ')
                : method.ToUpperInvariant() switch
                {
                    "POST" => "created",
                    "PUT" => "updated",
                    "PATCH" => "updated",
                    "DELETE" => "deleted",
                    _ => method.ToLowerInvariant()
                };

            return $"Admin {action} {target}.";
        }

        private static string ToTitle(string value)
            => string.Join(' ', value
                .Replace('-', ' ')
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
    }
}
