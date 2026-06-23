using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class BookingService : IBookingService
    {
        private static readonly TimeSpan BookingHoldDuration = TimeSpan.FromMinutes(15);
        private readonly TrainTicketDbContext _db;
        private readonly IBookingHoldExpiryService? _holdExpiryService;

        public BookingService(TrainTicketDbContext db, IBookingHoldExpiryService? holdExpiryService = null)
        {
            _db = db;
            _holdExpiryService = holdExpiryService;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            await using var transaction = await BeginTransactionIfRelationalAsync();
            await ExpireStalePendingBookingsAsync();

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

            if (booking.UserId.HasValue)
            {
                var userExists = await _db.Users.AnyAsync(u => u.Id == booking.UserId.Value);
                if (!userExists)
                    throw new KeyNotFoundException("User not found");
            }

            booking.GuestEmail = NormalizeOptionalEmail(booking.GuestEmail);
            booking.PassengerName = NormalizeOptionalText(booking.PassengerName);
            booking.BookingDate = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(booking.BookingReference))
                booking.BookingReference = GenerateBookingReference();

            if (string.IsNullOrWhiteSpace(booking.BookingStatus))
                booking.BookingStatus = "PendingPayment";

            if (booking.BookingStatus == "PendingPayment")
                booking.ExpiresAtUtc = DateTime.UtcNow.Add(BookingHoldDuration);

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

        public async Task<Booking> UpdateGuestBookingDataAsync(
            int bookingId,
            string guestEmail,
            string passengerName,
            bool acceptedTerms)
        {
            if (!acceptedTerms)
                throw new InvalidOperationException("Terms must be accepted before continuing");

            var booking = await _db.Bookings.FindAsync(bookingId)
                          ?? throw new KeyNotFoundException("Booking not found");

            if (ExpireIfPendingHoldElapsed(booking))
            {
                await _db.SaveChangesAsync();
                throw new InvalidOperationException("Booking hold has expired");
            }

            if (booking.IsCancelled || booking.BookingStatus is "Cancelled" or "Expired" or "Refunded")
                throw new InvalidOperationException("Cannot update data for this booking");

            var normalizedEmail = NormalizeRequiredEmail(guestEmail);
            var normalizedPassengerName = NormalizeRequiredText(passengerName, "Passenger name is required");

            booking.GuestEmail = normalizedEmail;
            booking.PassengerName = normalizedPassengerName;

            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task CancelBookingAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId)
                       ?? throw new KeyNotFoundException("Booking not found");

            if (ExpireIfPendingHoldElapsed(booking))
            {
                await _db.SaveChangesAsync();
                throw new InvalidOperationException("Booking hold has expired");
            }

            var cutoff = booking.TravelDate.AddHours(-1);
            if (DateTime.UtcNow > cutoff)
                throw new InvalidOperationException(
                    "Cannot cancel booking within 1 hour of travel date");

            booking.IsCancelled = true;
            booking.CancellationDate = DateTime.UtcNow;
            booking.BookingStatus = "Cancelled";
            await _db.SaveChangesAsync();
        }

        public async Task<Booking> AdminCancelAndRefundAsync(int bookingId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new InvalidOperationException("Cancellation reason is required");

            var booking = await _db.Bookings.FindAsync(bookingId)
                          ?? throw new KeyNotFoundException("Booking not found");

            if (booking.BookingStatus == "Refunded")
                return booking;

            if (booking.BookingStatus != "Confirmed" || booking.PaymentStatus != "Successful")
                throw new InvalidOperationException("Only paid confirmed bookings can be refunded by an admin");

            booking.BookingStatus = "Refunded";
            booking.PaymentStatus = "Refunded";
            booking.IsCancelled = true;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = reason.Trim();
            booking.RefundedAtUtc = DateTime.UtcNow;

            var successfulPayments = await _db.Payments
                .Where(p => p.BookingId == bookingId && p.Status == "Successful")
                .ToListAsync();

            foreach (var payment in successfulPayments)
                payment.Status = "Refunded";

            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _db.Bookings.FindAsync(bookingId);
            if (booking == null)
                throw new KeyNotFoundException("Booking not found");

            return booking;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
            => await _db.Bookings
                .Include(b => b.Train)
                .Include(b => b.Seat)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.TrainRoute)
                        .ThenInclude(r => r.DepartureStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.TrainRoute)
                        .ThenInclude(r => r.ArrivalStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Fares)
                .ToListAsync();

        public async Task<Booking> UpdateBookingAsync(Booking booking)
        {
            var existing = await _db.Bookings.FindAsync(booking.Id)
                           ?? throw new KeyNotFoundException("Booking not found");

            if (ExpireIfPendingHoldElapsed(existing))
            {
                await _db.SaveChangesAsync();
                throw new InvalidOperationException("Booking hold has expired");
            }

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

            if (ExpireIfPendingHoldElapsed(booking))
            {
                await _db.SaveChangesAsync();
                throw new InvalidOperationException("Booking hold has expired");
            }

            if (booking.IsCancelled)
                throw new InvalidOperationException("Cannot confirm a cancelled booking");

            var hasSuccessfulPayment = await _db.Payments
                .AnyAsync(p => p.BookingId == bookingId && p.Status == "Successful");

            if (!hasSuccessfulPayment)
                throw new InvalidOperationException("Booking does not have a successful payment");

            booking.PaymentStatus = "Successful";
            booking.BookingStatus = "Confirmed";
            booking.ConfirmedAtUtc ??= DateTime.UtcNow;
            EnsureTicketMetadata(booking);

            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId)
        {
            return await _db.Bookings
                .Include(b => b.Train)
                .Include(b => b.Seat)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.TrainRoute)
                        .ThenInclude(r => r.DepartureStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.TrainRoute)
                        .ThenInclude(r => r.ArrivalStation)
                .Include(b => b.Trip)
                    .ThenInclude(t => t!.Fares)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetGuestTicketsByEmailAsync(string guestEmail)
        {
            var normalizedEmail = NormalizeRequiredEmail(guestEmail);

            return await _db.Bookings
                .Include(b => b.Seat)
                .Include(b => b.Train)
                .Include(b => b.Trip)
                .Where(b => b.GuestEmail != null &&
                            b.GuestEmail.ToLower() == normalizedEmail.ToLower() &&
                            b.BookingStatus != "Expired")
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking> RefundTicketAsync(string ticketNumber, string guestEmail)
        {
            if (string.IsNullOrWhiteSpace(ticketNumber))
                throw new InvalidOperationException("Ticket number is required");

            var normalizedEmail = NormalizeRequiredEmail(guestEmail);
            var normalizedTicketNumber = ticketNumber.Trim();

            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.TicketNumber == normalizedTicketNumber)
                ?? throw new KeyNotFoundException("Ticket not found");

            if (!string.Equals(booking.GuestEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Ticket does not belong to this guest email");

            if (booking.BookingStatus != "Confirmed" || booking.PaymentStatus != "Successful")
                throw new InvalidOperationException("Only paid confirmed tickets can be refunded");

            var cutoff = booking.TravelDate.AddHours(-1);
            if (DateTime.UtcNow > cutoff)
                throw new InvalidOperationException("Cannot refund ticket within 1 hour of travel date");

            booking.BookingStatus = "Refunded";
            booking.PaymentStatus = "Refunded";
            booking.IsCancelled = true;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = "Passenger requested refund";
            booking.RefundedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking> RefundUserBookingAsync(int bookingId, int userId, string? reason)
        {
            var booking = await _db.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId)
                ?? throw new KeyNotFoundException("Ticket not found");

            if (booking.BookingStatus != "Confirmed" || booking.PaymentStatus != "Successful")
                throw new InvalidOperationException("Only paid confirmed tickets can be returned");

            var cutoff = booking.TravelDate.AddHours(-1);
            if (DateTime.UtcNow > cutoff)
                throw new InvalidOperationException("Cannot return ticket within 1 hour of travel date");

            booking.BookingStatus = "Refunded";
            booking.PaymentStatus = "Refunded";
            booking.IsCancelled = true;
            booking.CancellationDate = DateTime.UtcNow;
            booking.CancellationReason = string.IsNullOrWhiteSpace(reason)
                ? "Passenger requested return"
                : reason.Trim();
            booking.RefundedAtUtc = DateTime.UtcNow;

            var successfulPayments = await _db.Payments
                .Where(p => p.BookingId == bookingId && p.Status == "Successful")
                .ToListAsync();

            foreach (var payment in successfulPayments)
                payment.Status = "Refunded";

            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> CheckSeatAvailabilityAsync(int trainId, int seatId, DateTime travelDate)
        {
            await ExpireStalePendingBookingsAsync();
            var now = DateTime.UtcNow;

            var seat = await _db.Seats
                .FirstOrDefaultAsync(s => s.Id == seatId && s.TrainId == trainId);

            if (seat == null || !seat.IsAvailable)
                return false;

            var clash = await _db.Bookings
                .AnyAsync(b =>
                    b.SeatId == seatId &&
                    b.TrainId == trainId &&
                    b.TravelDate.Date == travelDate.Date &&
                    !b.IsCancelled &&
                    (b.BookingStatus == "Confirmed" ||
                     (b.BookingStatus == "PendingPayment" &&
                      (!b.ExpiresAtUtc.HasValue ||
                       b.ExpiresAtUtc.Value > now))));

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
            var now = DateTime.UtcNow;
            var query = _db.Bookings.Where(b =>
                b.SeatId == seatId &&
                b.TrainId == trainId &&
                !b.IsCancelled &&
                (b.BookingStatus == "Confirmed" ||
                 (b.BookingStatus == "PendingPayment" &&
                  (!b.ExpiresAtUtc.HasValue ||
                   b.ExpiresAtUtc.Value > now))) &&
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

        private static string GenerateBookingReference()
            => $"BKG-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..28];

        private static string GenerateTicketNumber()
            => $"WH{DateTime.UtcNow:yyMMdd}{Random.Shared.Next(1000, 9999)}";

        private static void EnsureTicketMetadata(Booking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.TicketNumber))
                booking.TicketNumber = GenerateTicketNumber();

            booking.TicketIssuedAtUtc ??= DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(booking.TicketQrPayload))
            {
                booking.TicketQrPayload = string.Join("|",
                    "railway-ticket-v1",
                    $"ticket={booking.TicketNumber}",
                    $"booking={booking.BookingReference}",
                    $"trip={booking.TripId?.ToString() ?? "legacy"}",
                    $"seat={booking.SeatId}",
                    $"date={booking.TravelDate:yyyy-MM-dd}",
                    $"issued={booking.TicketIssuedAtUtc:O}");
            }
        }

        private static string? NormalizeOptionalEmail(string? email)
        {
            return string.IsNullOrWhiteSpace(email)
                ? null
                : email.Trim().ToLowerInvariant();
        }

        private static string? NormalizeOptionalText(string? text)
        {
            return string.IsNullOrWhiteSpace(text)
                ? null
                : text.Trim();
        }

        private static string NormalizeRequiredEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new InvalidOperationException("Guest email is required");

            return email.Trim().ToLowerInvariant();
        }

        private static string NormalizeRequiredText(string text, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException(errorMessage);

            return text.Trim();
        }

        private async Task ExpireStalePendingBookingsAsync()
        {
            if (_holdExpiryService != null)
            {
                await _holdExpiryService.ExpireStaleHoldsAsync();
                return;
            }

            var now = DateTime.UtcNow;
            var staleBookings = await _db.Bookings
                .Where(b =>
                    !b.IsCancelled &&
                    b.BookingStatus == "PendingPayment" &&
                    b.ExpiresAtUtc.HasValue &&
                    b.ExpiresAtUtc.Value <= now)
                .ToListAsync();

            if (staleBookings.Count == 0)
                return;

            foreach (var booking in staleBookings)
                booking.BookingStatus = "Expired";

            await _db.SaveChangesAsync();
        }

        private static bool ExpireIfPendingHoldElapsed(Booking booking)
        {
            if (booking.IsCancelled ||
                booking.BookingStatus != "PendingPayment" ||
                !booking.ExpiresAtUtc.HasValue ||
                booking.ExpiresAtUtc.Value > DateTime.UtcNow)
            {
                return false;
            }

            booking.BookingStatus = "Expired";
            return true;
        }
    }
}
