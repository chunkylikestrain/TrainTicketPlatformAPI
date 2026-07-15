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

            var stationList = await stations
                .OrderBy(s => s.Locality != null ? s.Locality.Name : s.City)
                .ThenBy(s => s.Name)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var normalizedQuery = TripSegmentResolver.NormalizeSearchText(query);
                stationList = stationList
                    .Where(s => StationContainsNormalizedQuery(s, normalizedQuery))
                    .ToList();
            }

            return stationList
                .GroupBy(GetStationIdentityKey)
                .Select(group => group
                    .OrderByDescending(GetStationDisplayScore)
                    .ThenBy(s => s.Name)
                    .ThenBy(s => s.Id)
                    .First())
                .Select(s => ToDto(s))
                .ToList();
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

        private static bool StationContainsNormalizedQuery(Station station, string normalizedQuery)
        {
            var candidates = new[]
            {
                station.Code,
                station.Name,
                station.City,
                station.Locality?.Name ?? string.Empty,
                station.StateRegion?.Name ?? string.Empty,
                station.Country?.Name ?? string.Empty
            };

            return candidates.Any(candidate =>
                TripSegmentResolver.NormalizeSearchText(candidate).Contains(normalizedQuery));
        }

        private static string GetStationIdentityKey(Station station)
        {
            var normalizedName = TripSegmentResolver.NormalizeSearchText(station.Name);
            var locality = TripSegmentResolver.NormalizeSearchText(station.Locality?.Name ?? station.City);
            var normalizedCode = TripSegmentResolver.NormalizeSearchText(station.Code);
            if (!string.IsNullOrWhiteSpace(normalizedName) &&
                !string.Equals(normalizedName, normalizedCode, StringComparison.Ordinal))
                return $"name:{normalizedName}:{locality}";

            if (!string.IsNullOrWhiteSpace(normalizedCode))
                return $"code:{normalizedCode}";

            if (!string.IsNullOrWhiteSpace(station.ExternalSource) && station.ExternalStationId.HasValue)
                return $"external:{station.ExternalSource}:{station.ExternalStationId.Value}";

            return $"id:{station.Id}";
        }

        private static int GetStationDisplayScore(Station station)
        {
            var score = 0;
            if (!string.IsNullOrWhiteSpace(station.Name))
                score += 20;
            if (!string.Equals(station.Name, station.Code, StringComparison.OrdinalIgnoreCase))
                score += 10;
            if (station.Name.Any(c => c > 127))
                score += 5;
            if (!string.IsNullOrWhiteSpace(station.Locality?.Name ?? station.City))
                score += 3;
            if (!string.IsNullOrWhiteSpace(station.Code))
                score += 2;
            if (station.ExternalStationId.HasValue)
                score += 1;

            return score;
        }
    }
}
