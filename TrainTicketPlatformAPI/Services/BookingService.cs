using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;


namespace TrainTicketPlatformAPI.Services
{
    public class BookingService : IBookingService
    {
        private readonly TrainTicketDbContext _db;
        public BookingService(TrainTicketDbContext db)
            => _db = db;

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            // 1. Check seat availability
            var seat = await _db.Seats.FindAsync(booking.SeatId);
            if (seat == null || !seat.IsAvailable)
                throw new InvalidOperationException("Seat not available");

            // 2. Mark seat unavailable
            seat.IsAvailable = false;

            // 3. Add the booking record
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return booking;
        }

        public async Task CancelBookingAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId)
                       ?? throw new KeyNotFoundException("Booking not found");

            // 1. Enforce cancellation window (using UTC)
            var cutoff = booking.TravelDate.AddHours(-1);
            if (DateTime.UtcNow > cutoff)
                throw new InvalidOperationException(
                    "Cannot cancel booking within 1 hour of travel date");

            // 2. Soft‐delete the booking
            booking.IsCancelled = true;
            booking.CancellationDate = DateTime.UtcNow;

            // 3. Free the seat
            var seat = await _db.Seats.FindAsync(booking.SeatId);
            if (seat != null) seat.IsAvailable = true;

            // 4. Persist (no Remove!)
            await _db.SaveChangesAsync();
        }


        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        {
            var b = await _db.Bookings.FindAsync(bookingId);
            if (b == null) throw new KeyNotFoundException("Booking not found");
            return b;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
            => await _db.Bookings.ToListAsync();

        public async Task<Booking> UpdateBookingAsync(Booking booking)
        {
            var existing = await _db.Bookings.FindAsync(booking.Id)
                           ?? throw new KeyNotFoundException("Booking not found");

            // Seat change?
            if (existing.SeatId != booking.SeatId)
            {
                var newSeat = await _db.Seats.FindAsync(booking.SeatId);
                if (newSeat == null || !newSeat.IsAvailable)
                    throw new InvalidOperationException("New seat unavailable");

                // free old, reserve new
                var oldSeat = await _db.Seats.FindAsync(existing.SeatId);
                if (oldSeat != null) oldSeat.IsAvailable = true;
                newSeat.IsAvailable = false;

                // **Don’t forget to update the booking’s SeatId**
                existing.SeatId = booking.SeatId;
            }

            // Date change
            existing.TravelDate = booking.TravelDate;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId)
        {
            return await _db.Bookings
                            .Where(b => b.UserId == userId)
                            .ToListAsync();
        }
        public async Task<bool> CheckSeatAvailabilityAsync(int trainId, int seatId, DateTime travelDate)
        {
            // 1) Does the seat exist on that train?
            var seat = await _db.Seats
                                .FirstOrDefaultAsync(s => s.Id == seatId && s.TrainId == trainId);
            if (seat == null || !seat.IsAvailable)
                return false;

            // 2) Is it already booked on that date?
            var clash = await _db.Bookings
                                 .AnyAsync(b => b.SeatId == seatId
                                             && b.TravelDate.Date == travelDate.Date);
            return !clash;
        }
        public async Task<BookingReport> GenerateBookingReportAsync(DateTime from, DateTime to)
        {
            var totalBookings = await _db.Bookings
                .CountAsync(b => b.BookingDate >= from && b.BookingDate <= to);

            var totalRevenue = await _db.Payments
                .Where(p => p.PaymentDate >= from
                         && p.PaymentDate <= to
                         && p.Status == "Successful")
                .SumAsync(p => (decimal?)p.Amount) ?? 0m;

            var totalCancellations = await _db.Bookings
                .CountAsync(b =>
                    b.IsCancelled
                 && b.CancellationDate >= from
                 && b.CancellationDate <= to);

            return new BookingReport
            {
                From = from,
                To = to,
                TotalBookings = totalBookings,
                TotalRevenue = totalRevenue,
                TotalCancellations = totalCancellations
            };
        }


    }
}