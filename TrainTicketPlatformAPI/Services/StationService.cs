using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Stations;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class StationService : IStationService
    {
        private readonly TrainTicketDbContext _db;

        public StationService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<StationDto>> GetStationsAsync(string? query)
        {
            var stations = _db.Stations
                .AsNoTracking()
                .Include(s => s.Country)
                .Include(s => s.StateRegion)
                .Include(s => s.Locality)
                    .ThenInclude(l => l!.StateRegion)
                        .ThenInclude(r => r.Country)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalizedQuery = query.Trim().ToLower();
                stations = stations.Where(s =>
                    s.Code.ToLower().Contains(normalizedQuery) ||
                    s.Name.ToLower().Contains(normalizedQuery) ||
                    s.City.ToLower().Contains(normalizedQuery) ||
                    (s.Locality != null &&
                        s.Locality.Name.ToLower().Contains(normalizedQuery)) ||
                    (s.StateRegion != null &&
                        s.StateRegion.Name.ToLower().Contains(normalizedQuery)) ||
                    (s.Country != null &&
                        s.Country.Name.ToLower().Contains(normalizedQuery)));
            }

            return await stations
                .OrderBy(s => s.Locality != null ? s.Locality.Name : s.City)
                .ThenBy(s => s.Name)
                .Select(s => ToDto(s))
                .ToListAsync();
        }

        public async Task<StationDto> GetStationByIdAsync(int stationId)
        {
            var station = await _db.Stations
                .AsNoTracking()
                .Include(s => s.Country)
                .Include(s => s.StateRegion)
                .Include(s => s.Locality)
                    .ThenInclude(l => l!.StateRegion)
                        .ThenInclude(r => r.Country)
                .FirstOrDefaultAsync(s => s.Id == stationId)
                ?? throw new KeyNotFoundException("Station not found");

            return ToDto(station);
        }

        private static StationDto ToDto(Station station) => new()
        {
            Id = station.Id,
            Code = station.Code,
            Name = station.Name,
            City = station.City,
            CountryId = station.CountryId ?? station.Locality?.StateRegion.CountryId,
            CountryCode = station.Country?.Code ?? station.Locality?.StateRegion.Country.Code ?? string.Empty,
            CountryName = station.Country?.Name ?? station.Locality?.StateRegion.Country.Name ?? string.Empty,
            StateRegionId = station.StateRegionId ?? station.Locality?.StateRegionId,
            StateRegionCode = station.StateRegion?.Code ?? station.Locality?.StateRegion.Code ?? string.Empty,
            StateRegionName = station.StateRegion?.Name ?? station.Locality?.StateRegion.Name ?? string.Empty,
            LocalityId = station.LocalityId,
            LocalityName = station.Locality?.Name ?? station.City,
            LocalityType = station.Locality?.Type ?? string.Empty
        };
    }
}
