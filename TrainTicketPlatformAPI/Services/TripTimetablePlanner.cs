using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    internal static class TripTimetablePlanner
    {
        private static readonly string[] MajorStationTerms =
        [
            "warszawa",
            "warsaw",
            "krakow",
            "kraków",
            "gdansk",
            "gdańsk",
            "gdynia",
            "wroclaw",
            "wrocław",
            "poznan",
            "poznań",
            "katowice",
            "lodz",
            "łódź",
            "szczecin",
            "rzeszow",
            "rzeszów",
            "lublin",
            "bialystok",
            "białystok",
            "bydgoszcz",
            "torun",
            "toruń",
            "gliwice",
            "opole",
            "olsztyn",
            "kielce"
        ];

        private static readonly string[] MediumStationTerms =
        [
            "glowny",
            "główny",
            "central",
            "centrala",
            "miasto",
            "zachodni",
            "wschodni",
            "airport",
            "lotnisko"
        ];

        public static IReadOnlyList<PlannedTripStop> Build(Trip trip)
        {
            var routeStops = TripSegmentResolver.BuildOrderedRouteStations(trip.TrainRoute);
            var stopCount = routeStops.Count;

            return routeStops
                .Select(stop => BuildStop(trip, stop, stopCount))
                .ToList();
        }

        public static IEnumerable<TripCallingPatternStopDto> ToDto(
            IEnumerable<PlannedTripStop> stops,
            int? fromOrder = null,
            int? toOrder = null)
        {
            return stops
                .Where(stop =>
                    (!fromOrder.HasValue || stop.StopOrder >= fromOrder.Value) &&
                    (!toOrder.HasValue || stop.StopOrder <= toOrder.Value))
                .Select(stop => new TripCallingPatternStopDto
                {
                    StationId = stop.StationId,
                    StationCode = stop.StationCode,
                    StationName = stop.StationName,
                    StopOrder = stop.StopOrder,
                    ArrivalTime = stop.ArrivalTime,
                    DepartureTime = stop.DepartureTime,
                    ArrivalOffsetMinutes = stop.ArrivalOffsetMinutes,
                    DepartureOffsetMinutes = stop.DepartureOffsetMinutes,
                    DwellMinutes = stop.DwellMinutes,
                    Platform = stop.Platform,
                    Track = stop.Track,
                    StopType = stop.StopType
                });
        }

        private static PlannedTripStop BuildStop(
            Trip trip,
            TripSegmentResolver.RouteSearchStop stop,
            int stopCount)
        {
            var isOrigin = stop.Order == 0;
            var isDestination = stop.Order == stopCount - 1;
            var stopType = GetStopType(stop.Station, isOrigin || isDestination, stop.RouteStop?.StopType);
            var dwellMinutes = isOrigin || isDestination ? 0 : GetDwellMinutes(stopType);
            var baseOffset = GetEvenOffset(trip, stop.Order, stopCount);

            int? arrivalOffset = isOrigin
                ? null
                : stop.RouteStop?.ArrivalOffsetMinutes ?? Math.Max(0, baseOffset - (isDestination ? 0 : dwellMinutes / 2));
            int? departureOffset = isDestination
                ? null
                : stop.RouteStop?.DepartureOffsetMinutes ?? Math.Min(
                    GetTotalMinutes(trip),
                    isOrigin ? 0 : arrivalOffset.GetValueOrDefault(baseOffset) + dwellMinutes);

            if (!isOrigin && !isDestination && arrivalOffset.HasValue && departureOffset.HasValue &&
                departureOffset.Value <= arrivalOffset.Value)
            {
                departureOffset = arrivalOffset.Value + dwellMinutes;
            }

            return new PlannedTripStop(
                stop.StationId,
                stop.Station.Code,
                stop.Station.Name,
                stop.Order,
                arrivalOffset.HasValue ? trip.DepartureTime.AddMinutes(arrivalOffset.Value) : null,
                departureOffset.HasValue ? trip.DepartureTime.AddMinutes(departureOffset.Value) : null,
                arrivalOffset,
                departureOffset,
                dwellMinutes,
                string.IsNullOrWhiteSpace(stop.RouteStop?.Platform) ? GeneratePlatform(stop) : stop.RouteStop.Platform,
                string.IsNullOrWhiteSpace(stop.RouteStop?.Track) ? GenerateTrack(stop) : stop.RouteStop.Track,
                stopType);
        }

        private static int GetEvenOffset(Trip trip, int stopOrder, int stopCount)
        {
            if (stopOrder <= 0)
                return 0;

            if (stopOrder >= stopCount - 1)
                return GetTotalMinutes(trip);

            var totalStopsBetweenTermini = Math.Max(1, stopCount - 1);
            return (int)Math.Round(GetTotalMinutes(trip) * stopOrder / (double)totalStopsBetweenTermini);
        }

        private static int GetTotalMinutes(Trip trip)
            => Math.Max(0, (int)Math.Round((trip.ArrivalTime - trip.DepartureTime).TotalMinutes));

        public static int GetDwellMinutes(string stopType)
            => stopType switch
            {
                "Major" => 8,
                "Medium" => 5,
                _ => 2
            };

        public static string GetStopType(Station station, bool isTerminus, string? configuredStopType = null)
        {
            if (!string.IsNullOrWhiteSpace(configuredStopType))
                return configuredStopType.Trim();

            if (isTerminus)
                return "Terminus";

            var text = TripSegmentResolver.NormalizeSearchText(
                $"{station.Code} {station.Name} {station.City} {station.Locality?.Name}");

            if (MajorStationTerms.Any(term => text.Contains(TripSegmentResolver.NormalizeSearchText(term))))
                return "Major";

            if (MediumStationTerms.Any(term => text.Contains(TripSegmentResolver.NormalizeSearchText(term))))
                return "Medium";

            return "Local";
        }

        private static string GeneratePlatform(TripSegmentResolver.RouteSearchStop stop)
            => ((stop.StationId + stop.Order) % 4 + 1).ToString();

        private static string GenerateTrack(TripSegmentResolver.RouteSearchStop stop)
            => ((stop.StationId + stop.Order + 1) % 6 + 1).ToString();
    }

    internal sealed record PlannedTripStop(
        int StationId,
        string StationCode,
        string StationName,
        int StopOrder,
        DateTime? ArrivalTime,
        DateTime? DepartureTime,
        int? ArrivalOffsetMinutes,
        int? DepartureOffsetMinutes,
        int DwellMinutes,
        string Platform,
        string Track,
        string StopType);
}
