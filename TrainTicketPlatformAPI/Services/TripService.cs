using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Trips;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class TripService : ITripService
    {
        private readonly TrainTicketDbContext _db;

        public TripService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<TripSearchResultDto>> SearchTripsAsync(
            string from,
            string to,
            DateTime date)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                throw new InvalidOperationException("Both departure and arrival stations are required");

            var departure = from.Trim().ToLower();
            var arrival = to.Trim().ToLower();
            var travelDate = date.Date;

            var trips = await _db.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.DepartureStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.ArrivalStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.Fares)
                .Where(t =>
                    t.TrainRoute.IsActive &&
                    t.DepartureTime.Date == travelDate &&
                    (t.TrainRoute.DepartureStation.Code.ToLower() == departure ||
                     t.TrainRoute.DepartureStation.Name.ToLower() == departure ||
                     t.TrainRoute.DepartureStation.City.ToLower() == departure ||
                     (t.TrainRoute.DepartureStation.Locality != null &&
                      t.TrainRoute.DepartureStation.Locality.Name.ToLower() == departure)) &&
                    (t.TrainRoute.ArrivalStation.Code.ToLower() == arrival ||
                     t.TrainRoute.ArrivalStation.Name.ToLower() == arrival ||
                     t.TrainRoute.ArrivalStation.City.ToLower() == arrival ||
                     (t.TrainRoute.ArrivalStation.Locality != null &&
                      t.TrainRoute.ArrivalStation.Locality.Name.ToLower() == arrival)))
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();

            return trips.Select(ToSearchResult);
        }

        public async Task<TripDetailsDto> GetTripByIdAsync(int tripId)
        {
            var trip = await _db.Trips
                .AsNoTracking()
                .Include(t => t.Train)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.DepartureStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.TrainRoute)
                    .ThenInclude(r => r.ArrivalStation)
                        .ThenInclude(s => s.Locality)
                .Include(t => t.Fares)
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException("Trip not found");

            return ToDetails(trip);
        }

        public async Task<IEnumerable<TripSeatAvailabilityDto>> GetSeatAvailabilityAsync(int tripId)
        {
            var trip = await _db.Trips
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tripId)
                ?? throw new KeyNotFoundException("Trip not found");

            var now = DateTime.UtcNow;
            var bookedSeatIds = await _db.Bookings
                .AsNoTracking()
                .Where(b =>
                    b.TripId == tripId &&
                    b.TravelDate.Date == trip.DepartureTime.Date &&
                    !b.IsCancelled &&
                    (b.BookingStatus == "Confirmed" ||
                     (b.BookingStatus == "PendingPayment" &&
                      (!b.ExpiresAtUtc.HasValue ||
                       b.ExpiresAtUtc.Value > now))))
                .Select(b => b.SeatId)
                .ToListAsync();

            return await _db.Seats
                .AsNoTracking()
                .Where(s => s.TrainId == trip.TrainId)
                .OrderBy(s => s.Coach)
                .ThenBy(s => s.Number)
                .Select(s => new TripSeatAvailabilityDto
                {
                    SeatId = s.Id,
                    Coach = s.Coach,
                    Number = s.Number,
                    ClassType = s.ClassType,
                    IsAvailable = s.IsAvailable && !bookedSeatIds.Contains(s.Id)
                })
                .ToListAsync();
        }

        private static TripSearchResultDto ToSearchResult(Trip trip)
        {
            var lowestFare = trip.Fares.OrderBy(f => f.Price).FirstOrDefault();

            return new TripSearchResultDto
            {
                TripId = trip.Id,
                TrainId = trip.TrainId,
                TrainName = trip.Train.Name,
                DepartureStationCode = trip.TrainRoute.DepartureStation.Code,
                DepartureStationName = trip.TrainRoute.DepartureStation.Name,
                ArrivalStationCode = trip.TrainRoute.ArrivalStation.Code,
                ArrivalStationName = trip.TrainRoute.ArrivalStation.Name,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                Status = trip.Status,
                LowestFare = lowestFare?.Price,
                Currency = lowestFare?.Currency ?? string.Empty
            };
        }

        private static TripDetailsDto ToDetails(Trip trip)
        {
            return new TripDetailsDto
            {
                TripId = trip.Id,
                TrainId = trip.TrainId,
                TrainName = trip.Train.Name,
                DepartureStationCode = trip.TrainRoute.DepartureStation.Code,
                DepartureStationName = trip.TrainRoute.DepartureStation.Name,
                ArrivalStationCode = trip.TrainRoute.ArrivalStation.Code,
                ArrivalStationName = trip.TrainRoute.ArrivalStation.Name,
                DistanceKm = trip.TrainRoute.DistanceKm,
                DepartureTime = trip.DepartureTime,
                ArrivalTime = trip.ArrivalTime,
                Status = trip.Status,
                Fares = trip.Fares
                    .OrderBy(f => f.Price)
                    .Select(f => new FareDto
                    {
                        ClassType = f.ClassType,
                        Price = f.Price,
                        Currency = f.Currency
                    })
                    .ToList()
            };
        }
    }
}
