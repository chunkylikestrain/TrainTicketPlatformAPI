using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
            await using var transaction = await BeginTransactionIfRelationalAsync();

            var seat = await _db.Seats.FindAsync(booking.SeatId);
            if (seat == null || !seat.IsAvailable)
                throw new InvalidOperationException("Seat not available");

            var train = await _db.Trains.FindAsync(booking.TrainId);
            if (train == null)
                throw new KeyNotFoundException("Train not found");

            if (seat.TrainId != booking.TrainId)
                throw new InvalidOperationException("Seat does not belong to the selected train");

            if (booking.TripId.HasValue)
            {
                var trip = await _db.Trips.FindAsync(booking.TripId.Value)
                           ?? throw new KeyNotFoundException("Trip not found");

                if (trip.TrainId != booking.TrainId)
                    throw new InvalidOperationException("Trip does not belong to the selected train");

                booking.TravelDate = trip.DepartureTime.Date;
            }
            else if (booking.TravelDate == default)
            {
                throw new InvalidOperationException("Travel date is required");
            }

            if (await HasActiveSeatBookingAsync(
                    booking.TrainId,
                    booking.TripId,
                    booking.SeatId,
                    booking.TravelDate,
                    ignoredBookingId: null))
            {
                throw new InvalidOperationException("Seat already booked for this travel date");
            }

            booking.BookingDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(booking.PaymentStatus))
                booking.PaymentStatus = "Pending";

            _db.Bookings.Add(booking);

            try
            {
                await _db.SaveChangesAsync();
                if (transaction != null)
                    await transaction.CommitAsync();
            }
            catch (DbUpdateException ex) when (IsDuplicateBookingException(ex))
            {
                throw new InvalidOperationException("Seat already booked for this trip or travel date", ex);
            }

            return booking;
        }

        public async Task CancelBookingAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId)
                       ?? throw new KeyNotFoundException("Booking not found");

            var cutoff = booking.TravelDate.AddHours(-1);
            if (DateTime.UtcNow > cutoff)
                throw new InvalidOperationException(
                    "Cannot cancel booking within 1 hour of travel date");

            booking.IsCancelled = true;
            booking.CancellationDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            return booking;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
            => await _db.Bookings.ToListAsync();

        public async Task<Booking> UpdateBookingAsync(Booking booking)
        {
            var existing = await _db.Bookings.FindAsync(booking.Id)
                           ?? throw new KeyNotFoundException("Booking not found");

            if (existing.IsCancelled)
                throw new InvalidOperationException("Cannot update a cancelled booking");

            var requestedSeatId = booking.SeatId == 0 ? existing.SeatId : booking.SeatId;
            var requestedTravelDate = booking.TravelDate == default
                ? existing.TravelDate
                : booking.TravelDate;
            var requestedTripId = booking.TripId ?? existing.TripId;

            if (existing.SeatId != requestedSeatId)
            {
                var newSeat = await _db.Seats.FindAsync(requestedSeatId);
                if (newSeat == null || !newSeat.IsAvailable)
                    throw new InvalidOperationException("New seat unavailable");

                if (newSeat.TrainId != existing.TrainId)
                    throw new InvalidOperationException("New seat does not belong to the booked train");
            }

            if (requestedTripId.HasValue)
            {
                var trip = await _db.Trips.FindAsync(requestedTripId.Value)
                           ?? throw new KeyNotFoundException("Trip not found");

                if (trip.TrainId != existing.TrainId)
                    throw new InvalidOperationException("Trip does not belong to the booked train");

                requestedTravelDate = trip.DepartureTime.Date;
            }

            if (await HasActiveSeatBookingAsync(
                    existing.TrainId,
                    requestedTripId,
                    requestedSeatId,
                    requestedTravelDate,
                    existing.Id))
            {
                throw new InvalidOperationException("Seat already booked for this travel date");
            }

            existing.SeatId = requestedSeatId;
            existing.TripId = requestedTripId;
            existing.TravelDate = requestedTravelDate;

            await _db.SaveChangesAsync();
            return existing;
        }

        public async Task<Booking> ConfirmBookingAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId)
                          ?? throw new KeyNotFoundException("Booking not found");

            if (booking.IsCancelled)
                throw new InvalidOperationException("Cannot confirm a cancelled booking");

            var hasSuccessfulPayment = await _db.Payments
                .AnyAsync(p => p.BookingId == bookingId && p.Status == "Successful");

            if (!hasSuccessfulPayment)
                throw new InvalidOperationException("Booking does not have a successful payment");

            booking.PaymentStatus = "Successful";
            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId)
        {
            return await _db.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> CheckSeatAvailabilityAsync(int trainId, int seatId, DateTime travelDate)
        {
            var seat = await _db.Seats
                .FirstOrDefaultAsync(s => s.Id == seatId && s.TrainId == trainId);

            if (seat == null || !seat.IsAvailable)
                return false;

            var clash = await _db.Bookings
                .AnyAsync(b =>
                    b.SeatId == seatId &&
                    b.TrainId == trainId &&
                    b.TravelDate.Date == travelDate.Date &&
                    !b.IsCancelled);

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

        private Task<bool> HasActiveSeatBookingAsync(
            int trainId,
            int? tripId,
            int seatId,
            DateTime travelDate,
            int? ignoredBookingId)
        {
            var query = _db.Bookings.Where(b =>
                b.SeatId == seatId &&
                b.TrainId == trainId &&
                !b.IsCancelled &&
                (!ignoredBookingId.HasValue || b.Id != ignoredBookingId.Value));

            query = tripId.HasValue
                ? query.Where(b => b.TripId == tripId.Value)
                : query.Where(b => b.TripId == null && b.TravelDate.Date == travelDate.Date);

            return query.AnyAsync();
        }

        private async Task<IDbContextTransaction?> BeginTransactionIfRelationalAsync()
        {
            return _db.Database.IsRelational()
                ? await _db.Database.BeginTransactionAsync()
                : null;
        }

        private static bool IsDuplicateBookingException(DbUpdateException ex)
        {
            return ex.InnerException?.Message.Contains("IX_Bookings", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
