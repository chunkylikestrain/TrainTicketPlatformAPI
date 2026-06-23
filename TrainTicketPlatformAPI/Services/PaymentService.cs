using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Payments;
using TrainTicketPlatformAPI.Data;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public class PaymentService : IPaymentService
    {
        public const string SuccessToken = "tok_success";
        public const string FailToken = "tok_fail";

        private readonly TrainTicketDbContext _db;

        public PaymentService(TrainTicketDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int bookingId)
        {
            var booking = await _db.Bookings
                .Include(b => b.Seat)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new KeyNotFoundException("Booking not found");

            EnsureBookingCanBePaid(booking);

            var fare = await GetFareForBookingAsync(booking);

            return new PaymentIntentDto
            {
                PaymentIntentId = BuildPaymentIntentId(booking.Id),
                BookingId = booking.Id,
                Amount = fare.Price,
                Currency = fare.Currency,
                Status = booking.PaymentStatus,
                ExpiresAtUtc = booking.ExpiresAtUtc,
                TestPaymentMethodTokens = new[] { SuccessToken, FailToken }
            };
        }

        public async Task<Payment> ConfirmPaymentAsync(string paymentIntentId, string paymentMethodToken)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new InvalidOperationException("Payment intent is required");

            if (string.IsNullOrWhiteSpace(paymentMethodToken))
                throw new InvalidOperationException("Payment method token is required");

            var bookingId = ParseBookingIdFromIntent(paymentIntentId.Trim());
            var booking = await _db.Bookings
                .Include(b => b.Seat)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new KeyNotFoundException("Booking not found");

            EnsureBookingCanBePaid(booking);

            if (await _db.Payments.AnyAsync(p =>
                    p.BookingId == booking.Id &&
                    p.PaymentIntentId == paymentIntentId &&
                    p.Status == "Successful"))
            {
                throw new InvalidOperationException("Payment intent has already been confirmed");
            }

            var fare = await GetFareForBookingAsync(booking);
            var status = paymentMethodToken.Trim() switch
            {
                SuccessToken => "Successful",
                FailToken => "Failed",
                _ => throw new InvalidOperationException("Payment method token is invalid")
            };

            var payment = new Payment
            {
                BookingId = booking.Id,
                PaymentIntentId = paymentIntentId,
                PaymentMethodToken = paymentMethodToken.Trim(),
                PaymentDate = DateTime.UtcNow,
                Amount = fare.Price,
                Status = status
            };

            _db.Payments.Add(payment);
            booking.PaymentStatus = status;
            if (status == "Successful")
            {
                booking.BookingStatus = "Confirmed";
                booking.ConfirmedAtUtc ??= DateTime.UtcNow;
                EnsureTicketMetadata(booking);
            }

            await _db.SaveChangesAsync();
            return payment;
        }

        public Task<Payment> ProcessPaymentAsync(int bookingId, decimal amount, string paymentMethodToken)
        {
            return ConfirmPaymentAsync(BuildPaymentIntentId(bookingId), paymentMethodToken);
        }

        public async Task<Payment> GetPaymentByIdAsync(int paymentId)
        {
            var p = await _db.Payments.FindAsync(paymentId);
            if (p == null)
                throw new KeyNotFoundException("Payment not found");

            return p;
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByBookingAsync(int bookingId)
        {
            return await _db.Payments
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _db.Payments.ToListAsync();
        }

        private static string BuildPaymentIntentId(int bookingId)
            => $"pi_{bookingId}";

        private static int ParseBookingIdFromIntent(string paymentIntentId)
        {
            if (!paymentIntentId.StartsWith("pi_", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(paymentIntentId[3..], out var bookingId))
            {
                throw new InvalidOperationException("Payment intent is invalid");
            }

            return bookingId;
        }

        private static void EnsureBookingCanBePaid(Booking booking)
        {
            if (booking.IsCancelled)
                throw new InvalidOperationException("Cannot process payment for a cancelled booking");

            if (booking.BookingStatus == "Confirmed")
                throw new InvalidOperationException("Booking is already confirmed");

            if (booking.BookingStatus == "Expired" ||
                (booking.BookingStatus == "PendingPayment" &&
                 booking.ExpiresAtUtc.HasValue &&
                 booking.ExpiresAtUtc.Value <= DateTime.UtcNow))
            {
                booking.BookingStatus = "Expired";
                throw new InvalidOperationException("Booking hold has expired");
            }

            if (!booking.UserId.HasValue &&
                (string.IsNullOrWhiteSpace(booking.GuestEmail) ||
                 string.IsNullOrWhiteSpace(booking.PassengerName)))
            {
                throw new InvalidOperationException("Guest email and passenger name are required before payment");
            }
        }

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

        private async Task<Fare> GetFareForBookingAsync(Booking booking)
        {
            if (!booking.TripId.HasValue)
                throw new InvalidOperationException("Booking must be linked to a trip before payment");

            var classType = booking.Seat?.ClassType;
            var fare = await _db.Fares
                .Where(f => f.TripId == booking.TripId.Value)
                .OrderByDescending(f => classType != null && f.ClassType == classType)
                .ThenBy(f => f.Price)
                .FirstOrDefaultAsync();

            return fare ?? throw new InvalidOperationException("No fare is configured for this booking");
        }
    }
}
