using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class TrainService : ITrainService
    {
        private readonly TrainTicketDbContext _db;
        public TrainService(TrainTicketDbContext db) => _db = db;

        public async Task<IEnumerable<Train>> SearchTrainsAsync(
            string departureStation,
            string arrivalStation,
            DateTime date)
        {
            return await _db.Trains
                .Where(t =>
                    t.DepartureStation.Equals(departureStation, StringComparison.OrdinalIgnoreCase) &&
                    t.ArrivalStation.Equals(arrivalStation, StringComparison.OrdinalIgnoreCase) &&
                    t.DepartureTime.Date == date.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Train>> GetAllTrainsAsync() =>
            await _db.Trains.ToListAsync();

        public async Task<Train> GetTrainByIdAsync(int trainId)
        {
            var train = await _db.Trains.FindAsync(trainId);
            if (train == null) throw new KeyNotFoundException("Train not found");
            return train;
        }

        public async Task<Train> CreateTrainAsync(Train train)
        {
            _db.Trains.Add(train);
            await _db.SaveChangesAsync();
            return train;
        }

        public async Task<Train> UpdateTrainAsync(Train train)
        {
            var existing = await _db.Trains.FindAsync(train.Id)
                          ?? throw new KeyNotFoundException("Train not found");

            existing.Name = train.Name;
            existing.DepartureStation = train.DepartureStation;
            existing.ArrivalStation = train.ArrivalStation;
            existing.DepartureTime = train.DepartureTime;
            existing.ArrivalTime = train.ArrivalTime;
            existing.Price = train.Price;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteTrainAsync(int trainId)
        {
            var train = await _db.Trains.FindAsync(trainId)
                        ?? throw new KeyNotFoundException("Train not found");

            // prevent deletion if seats or bookings exist
            bool hasSeats = await _db.Seats.AnyAsync(s => s.TrainId == trainId);
            bool hasBookings = await _db.Bookings.AnyAsync(b => b.TrainId == trainId);
            if (hasSeats || hasBookings)
                throw new InvalidOperationException(
                  "Cannot delete train with existing seats or bookings");

            _db.Trains.Remove(train);
            await _db.SaveChangesAsync();
        }
    }
}


