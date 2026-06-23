using System.Globalization;
using System.Text;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    internal static class TripSegmentResolver
    {
        public static TripSegmentInfo Resolve(
            Trip trip,
            int? departureStationId,
            int? arrivalStationId)
        {
            var routeStations = BuildOrderedRouteStations(trip.TrainRoute);
            if (routeStations.Count < 2)
                throw new InvalidOperationException("Trip route must have departure and arrival stations");

            var departureStop = departureStationId.HasValue
                ? routeStations.FirstOrDefault(stop => stop.StationId == departureStationId.Value)
                : routeStations.First();
            var arrivalStop = arrivalStationId.HasValue
                ? routeStations.FirstOrDefault(stop => stop.StationId == arrivalStationId.Value)
                : routeStations.Last();

            if (departureStop == null || arrivalStop == null)
                throw new InvalidOperationException("Selected segment stations are not on this trip route");

            if (departureStop.Order >= arrivalStop.Order)
                throw new InvalidOperationException("Selected segment arrival must be after departure");

            var plannedStops = TripTimetablePlanner.Build(trip);
            var plannedDeparture = plannedStops.Single(stop => stop.StopOrder == departureStop.Order);
            var plannedArrival = plannedStops.Single(stop => stop.StopOrder == arrivalStop.Order);

            return new TripSegmentInfo(
                departureStop.StationId,
                arrivalStop.StationId,
                departureStop.Order,
                arrivalStop.Order,
                plannedDeparture.DepartureTime ?? plannedDeparture.ArrivalTime ?? trip.DepartureTime,
                plannedArrival.ArrivalTime ?? plannedArrival.DepartureTime ?? trip.ArrivalTime,
                departureStop.Station.Name,
                arrivalStop.Station.Name);
        }

        public static List<RouteSearchStop> BuildOrderedRouteStations(TrainRoute route)
        {
            var stops = new List<RouteSearchStop>
            {
                new(0, route.DepartureStationId, route.DepartureStation)
            };

            stops.AddRange(route.RouteStops
                .OrderBy(stop => stop.StopOrder)
                .Select((stop, index) => new RouteSearchStop(index + 1, stop.StationId, stop.Station, stop)));

            stops.Add(new RouteSearchStop(stops.Count, route.ArrivalStationId, route.ArrivalStation));
            return stops;
        }

        public static DateTime EstimateStopTime(Trip trip, int stopOrder, int stopCount)
        {
            if (stopOrder <= 0)
                return trip.DepartureTime;

            if (stopOrder >= stopCount - 1)
                return trip.ArrivalTime;

            var totalStopsBetweenTermini = Math.Max(1, stopCount - 1);
            var tripMinutes = (trip.ArrivalTime - trip.DepartureTime).TotalMinutes;
            var minutesFromOrigin = tripMinutes * stopOrder / totalStopsBetweenTermini;
            return trip.DepartureTime.AddMinutes(minutesFromOrigin);
        }

        public static bool StationMatches(Station station, string query)
        {
            return NormalizeSearchText(station.Code) == query ||
                NormalizeSearchText(station.Name) == query ||
                NormalizeSearchText(station.City) == query ||
                (station.Locality != null &&
                 NormalizeSearchText(station.Locality.Name) == query);
        }

        public static string NormalizeSearchText(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(normalized.Length);

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                    continue;

                builder.Append(character switch
                {
                    '\u0141' or '\u0142' => 'l',
                    _ => char.ToLowerInvariant(character)
                });
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        public sealed record RouteSearchStop(
            int Order,
            int StationId,
            Station Station,
            TrainRouteStop? RouteStop = null);
    }

    internal sealed record TripSegmentInfo(
        int DepartureStationId,
        int ArrivalStationId,
        int DepartureOrder,
        int ArrivalOrder,
        DateTime DepartureTime,
        DateTime ArrivalTime,
        string DepartureStationName,
        string ArrivalStationName);
}
