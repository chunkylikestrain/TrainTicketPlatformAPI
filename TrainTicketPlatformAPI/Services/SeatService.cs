using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class SeatService : ISeatService
    {
        private readonly TrainTicketDbContext _db;
        public SeatService(TrainTicketDbContext db) => _db = db;

        public async Task<IEnumerable<Seat>> GetAllSeatsAsync()
            => await _db.Seats.ToListAsync();

        public async Task<Seat> GetSeatByIdAsync(int seatId)
        {
            var seat = await _db.Seats.FindAsync(seatId);
            if (seat == null) throw new KeyNotFoundException("Seat not found");
            return seat;
        }

        public async Task<IEnumerable<Seat>> GetSeatsByTrainAsync(int trainId)
        {
            // Return all seats for the given train
            return await _db.Seats
                            .Where(s => s.TrainId == trainId)
                            .ToListAsync();
        }

        public async Task<Seat> CreateSeatAsync(Seat seat)
        {
            // Validate Train exists
            var train = await _db.Trains.FindAsync(seat.TrainId);
            if (train == null) throw new KeyNotFoundException("Train not found");

            _db.Seats.Add(seat);
            await _db.SaveChangesAsync();
            return seat;
        }

        public async Task<Seat> UpdateSeatAsync(Seat seat)
        {
            var existing = await _db.Seats.FindAsync(seat.Id)
                         ?? throw new KeyNotFoundException("Seat not found");

            // Update mutable fields
            existing.Coach = seat.Coach;
            existing.Number = seat.Number;
            existing.ClassType = seat.ClassType;
            existing.IsAvailable = seat.IsAvailable;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteSeatAsync(int seatId)
        {
            var seat = await _db.Seats.FindAsync(seatId);
            if (seat == null) throw new KeyNotFoundException("Seat not found");

            // Prevent deleting seats that are booked
            bool hasBooking = await _db.Bookings.AnyAsync(b => b.SeatId == seatId);
            if (hasBooking)
                throw new InvalidOperationException("Cannot delete a seat with existing bookings");

            _db.Seats.Remove(seat);
            await _db.SaveChangesAsync();
        }
    }
}
