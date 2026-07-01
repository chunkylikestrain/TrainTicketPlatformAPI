using TrainTicketPlatformAPI.Contracts.OpenRailway;

namespace TrainTicketPlatformAPI.Services
{
    public interface IOpenRailwayClient
    {
        Task<OpenRailwayDataVersionDto> GetDataVersionAsync(CancellationToken cancellationToken);
        Task<OpenRailwayStationsResponseDto> SearchStationsAsync(
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken);
        Task<OpenRailwayRouteIdsResponseDto> GetRouteIdsAsync(
            DateOnly date,
            CancellationToken cancellationToken);
        Task<OpenRailwayRouteDto> GetRouteAsync(
            int scheduleId,
            int orderId,
            CancellationToken cancellationToken);
    }
}
