using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using TrainTicketPlatformAPI.Contracts.OpenRailway;
using TrainTicketPlatformAPI.Security;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/open-railway")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitPolicyNames.AdminImport)]
    public class AdminOpenRailwayImportController : ControllerBase
    {
        private readonly IOpenRailwayClient _client;
        private readonly IOpenRailwayImportService _importService;

        public AdminOpenRailwayImportController(
            IOpenRailwayClient client,
            IOpenRailwayImportService importService)
        {
            _client = client;
            _importService = importService;
        }

        [HttpGet("data-version")]
        public async Task<IActionResult> GetDataVersion(CancellationToken cancellationToken)
        {
            return await TryOpenRailwayAsync(async () =>
            {
                var version = await _client.GetDataVersionAsync(cancellationToken);
                return Ok(version);
            });
        }

        [HttpGet("stations")]
        public async Task<IActionResult> SearchStations(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            return await TryOpenRailwayAsync(async () =>
            {
                var stations = await _client.SearchStationsAsync(search, page, pageSize, cancellationToken);
                return Ok(stations);
            });
        }

        [HttpGet("routes/{date}")]
        public async Task<IActionResult> GetRouteIds(
            DateOnly date,
            [FromQuery] int limit = 100,
            CancellationToken cancellationToken = default)
        {
            return await TryOpenRailwayAsync(async () =>
            {
                var routes = await _client.GetRouteIdsAsync(date, cancellationToken);
                var safeLimit = Math.Clamp(limit, 1, 1_000);
                var allRoutes = routes.Routes ?? [];
                var interCityRoutes = allRoutes
                    .Where(route => OpenRailwayImportRules.IsInterCityCarrier(route.CarrierCode))
                    .ToList();

                return Ok(new
                {
                    routes.GeneratedAt,
                    routes.Date,
                    Count = interCityRoutes.Count,
                    SourceCount = routes.Count,
                    FilteredOutCount = Math.Max(0, routes.Count - interCityRoutes.Count),
                    ReturnedCount = Math.Min(interCityRoutes.Count, safeLimit),
                    Routes = interCityRoutes
                        .Take(safeLimit)
                });
            });
        }

        [HttpGet("routes/{scheduleId:int}/{orderId:int}/preview")]
        public async Task<IActionResult> PreviewRoute(
            int scheduleId,
            int orderId,
            [FromQuery] DateOnly? operatingDate,
            CancellationToken cancellationToken)
        {
            return await TryOpenRailwayAsync(async () =>
            {
                var preview = await _importService.PreviewRouteAsync(scheduleId, orderId, operatingDate, cancellationToken);
                return Ok(preview);
            });
        }

        [HttpPost("routes/{scheduleId:int}/{orderId:int}/import")]
        public async Task<IActionResult> ImportRoute(
            int scheduleId,
            int orderId,
            [FromBody] OpenRailwayImportRouteRequest? request,
            CancellationToken cancellationToken)
        {
            request ??= new OpenRailwayImportRouteRequest();
            if (!IsConfirmedImport(request.ConfirmApply, request.ConfirmationText))
                return ImportConfirmationRequired();

            if (DangerousActionGuard.RequireHeader(this, DangerousActionGuard.Import) is { } headerError)
                return headerError;

            return await TryOpenRailwayAsync(async () =>
            {
                var result = await _importService.ImportRouteAsync(
                    scheduleId,
                    orderId,
                    request.OperatingDate,
                    cancellationToken);

                return Ok(result);
            });
        }

        [HttpPost("routes/{date}/import")]
        public async Task<IActionResult> ImportRoutesForDate(
            DateOnly date,
            [FromBody] OpenRailwayImportDateRequest? request,
            CancellationToken cancellationToken)
        {
            request ??= new OpenRailwayImportDateRequest();
            if (!request.DryRun)
            {
                if (!IsConfirmedImport(request.ConfirmApply, request.ConfirmationText))
                    return ImportConfirmationRequired();

                if (DangerousActionGuard.RequireHeader(this, DangerousActionGuard.Import) is { } headerError)
                    return headerError;
            }

            return await TryOpenRailwayAsync(async () =>
            {
                var result = await _importService.ImportRoutesForDateAsync(
                    date,
                    request,
                    cancellationToken);

                return Ok(result);
            });
        }

        [HttpGet("seed-snapshot")]
        public async Task<IActionResult> ExportSeedSnapshot(
            [FromQuery] DateOnly? operatingDate,
            CancellationToken cancellationToken)
        {
            var snapshot = await _importService.ExportSeedSnapshotAsync(operatingDate, cancellationToken);
            return Ok(snapshot);
        }

        private async Task<IActionResult> TryOpenRailwayAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Title = "Open Railway request failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status502BadGateway
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway, new ProblemDetails
                {
                    Title = "Open Railway request failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status502BadGateway
                });
            }
        }

        private static bool IsConfirmedImport(bool confirmApply, string? confirmationText)
        {
            return confirmApply &&
                string.Equals(confirmationText?.Trim(), "IMPORT", StringComparison.Ordinal);
        }

        private IActionResult ImportConfirmationRequired()
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Import confirmation required",
                Detail = "Set ConfirmApply to true and ConfirmationText to IMPORT before applying Open Railway data.",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
