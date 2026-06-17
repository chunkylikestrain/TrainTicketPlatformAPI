using TrainTicketPlatformAPI.Contracts.Stations;

namespace TrainTicketPlatformAPI.Services
{
    public interface IStationService
    {
        Task<IEnumerable<StationDto>> GetStationsAsync(string? query);
        Task<StationDto> GetStationByIdAsync(int stationId);
    }
}
