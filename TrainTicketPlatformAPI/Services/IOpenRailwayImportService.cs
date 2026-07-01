using TrainTicketPlatformAPI.Contracts.OpenRailway;

namespace TrainTicketPlatformAPI.Services
{
    public interface IOpenRailwayImportService
    {
        Task<OpenRailwayImportPreviewDto> PreviewRouteAsync(
            int scheduleId,
            int orderId,
            CancellationToken cancellationToken);

        Task<OpenRailwayImportRouteResultDto> ImportRouteAsync(
            int scheduleId,
            int orderId,
            DateOnly? operatingDate,
            CancellationToken cancellationToken);

        Task<OpenRailwayImportDateResultDto> ImportRoutesForDateAsync(
            DateOnly date,
            OpenRailwayImportDateRequest request,
            CancellationToken cancellationToken);

        Task<OpenRailwaySeedSnapshotDto> ExportSeedSnapshotAsync(
            DateOnly? operatingDate,
            CancellationToken cancellationToken);
    }
}
