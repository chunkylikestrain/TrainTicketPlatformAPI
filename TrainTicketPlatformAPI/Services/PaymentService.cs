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
                BookingIds = new[] { booking.Id },
                Amount = BookingPricingCalculator.GetPayableAmount(booking, fare),
                Currency = fare.Currency,
                Status = booking.PaymentStatus,
                ExpiresAtUtc = booking.ExpiresAtUtc,
                TestPaymentMethodTokens = new[] { SuccessToken, FailToken }
            };
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentForOrderAsync(int bookingOrderId)
        {
            var order = await LoadOrderForPaymentAsync(bookingOrderId)
                ?? throw new KeyNotFoundException("Booking order not found");

            EnsureOrderCanBePaid(order);

            var totals = await GetOrderFareTotalAsync(order);

            return new PaymentIntentDto
            {
                PaymentIntentId = BuildOrderPaymentIntentId(order.Id),
                BookingOrderId = order.Id,
                BookingIds = order.Bookings.Select(b => b.Id).Order().ToList(),
                Amount = totals.Amount,
                Currency = totals.Currency,
                Status = order.PaymentStatus,
                ExpiresAtUtc = order.ExpiresAtUtc,
                TestPaymentMethodTokens = new[] { SuccessToken, FailToken }
            };
        }

        public async Task<Payment> ConfirmPaymentAsync(string paymentIntentId, string paymentMethodToken)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new InvalidOperationException("Payment intent is required");

            if (string.IsNullOrWhiteSpace(paymentMethodToken))
                throw new InvalidOperationException("Payment method token is required");

            paymentIntentId = paymentIntentId.Trim();
            if (paymentIntentId.StartsWith("pi_order_", StringComparison.OrdinalIgnoreCase))
                return await ConfirmOrderPaymentAsync(paymentIntentId, paymentMethodToken);

            var bookingId = ParseBookingIdFromIntent(paymentIntentId);
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
                Amount = BookingPricingCalculator.GetPayableAmount(booking, fare),
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
            var p = await _db.Payments
                .Include(payment => payment.BookingOrder)
                    .ThenInclude(order => order!.Bookings)
                .FirstOrDefaultAsync(payment => payment.Id == paymentId);
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

        private static string BuildOrderPaymentIntentId(int bookingOrderId)
            => $"pi_order_{bookingOrderId}";

        private static int ParseBookingIdFromIntent(string paymentIntentId)
        {
            if (!paymentIntentId.StartsWith("pi_", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(paymentIntentId[3..], out var bookingId))
            {
                throw new InvalidOperationException("Payment intent is invalid");
            }

            return bookingId;
        }

        private static int ParseOrderIdFromIntent(string paymentIntentId)
        {
            if (!paymentIntentId.StartsWith("pi_order_", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(paymentIntentId["pi_order_".Length..], out var bookingOrderId))
            {
                throw new InvalidOperationException("Payment intent is invalid");
            }

            return bookingOrderId;
        }

        private async Task<Payment> ConfirmOrderPaymentAsync(string paymentIntentId, string paymentMethodToken)
        {
            var orderId = ParseOrderIdFromIntent(paymentIntentId);
            var order = await LoadOrderForPaymentAsync(orderId)
                ?? throw new KeyNotFoundException("Booking order not found");

            EnsureOrderCanBePaid(order);

            if (await _db.Payments.AnyAsync(p =>
                    p.BookingOrderId == order.Id &&
                    p.PaymentIntentId == paymentIntentId &&
                    p.Status == "Successful"))
            {
                throw new InvalidOperationException("Payment intent has already been confirmed");
            }

            var totals = await GetOrderFareTotalAsync(order);
            var status = paymentMethodToken.Trim() switch
            {
                SuccessToken => "Successful",
                FailToken => "Failed",
                _ => throw new InvalidOperationException("Payment method token is invalid")
            };

            var payment = new Payment
            {
                BookingOrderId = order.Id,
                PaymentIntentId = paymentIntentId,
                PaymentMethodToken = paymentMethodToken.Trim(),
                PaymentDate = DateTime.UtcNow,
                Amount = totals.Amount,
                Status = status
            };

            _db.Payments.Add(payment);
            order.PaymentStatus = status;

            foreach (var booking in order.Bookings)
            {
                booking.PaymentStatus = status;
                if (status == "Successful")
                {
                    booking.BookingStatus = "Confirmed";
                    booking.ConfirmedAtUtc ??= DateTime.UtcNow;
                    EnsureTicketMetadata(booking);
                }
            }

            if (status == "Successful")
            {
                order.BookingStatus = "Confirmed";
                order.ConfirmedAtUtc ??= DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            return payment;
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

        private static void EnsureOrderCanBePaid(BookingOrder order)
        {
            if (order.BookingStatus == "Confirmed")
                throw new InvalidOperationException("Booking order is already confirmed");

            if (order.BookingStatus == "Expired" ||
                (order.BookingStatus == "PendingPayment" &&
                 order.ExpiresAtUtc.HasValue &&
                 order.ExpiresAtUtc.Value <= DateTime.UtcNow))
            {
                order.BookingStatus = "Expired";
                foreach (var booking in order.Bookings.Where(b => b.BookingStatus == "PendingPayment"))
                    booking.BookingStatus = "Expired";

                throw new InvalidOperationException("Booking hold has expired");
            }

            if (!order.UserId.HasValue && string.IsNullOrWhiteSpace(order.GuestEmail))
                throw new InvalidOperationException("Guest email is required before payment");

            if (order.Bookings.Count == 0)
                throw new InvalidOperationException("Booking order has no tickets");

            if (order.Bookings.Any(b =>
                    b.IsCancelled ||
                    b.BookingStatus != "PendingPayment" ||
                    b.PaymentStatus != "Pending" ||
                    string.IsNullOrWhiteSpace(b.PassengerName)))
            {
                throw new InvalidOperationException("All passenger tickets must be pending and contain passenger names before payment");
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
                    $"segment={booking.SegmentDepartureOrder?.ToString() ?? "origin"}-{booking.SegmentArrivalOrder?.ToString() ?? "destination"}",
                    $"date={(booking.SegmentDepartureTime ?? booking.TravelDate):yyyy-MM-dd}",
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

        private async Task<(decimal Amount, string Currency)> GetOrderFareTotalAsync(BookingOrder order)
        {
            decimal amount = 0;
            var currency = string.Empty;

            foreach (var booking in order.Bookings)
            {
                var fare = await GetFareForBookingAsync(booking);
                amount += BookingPricingCalculator.GetPayableAmount(booking, fare);
                currency = string.IsNullOrWhiteSpace(currency)
                    ? !string.IsNullOrWhiteSpace(booking.Currency) ? booking.Currency : fare.Currency
                    : currency;
            }

            return (amount, currency);
        }

        private async Task<BookingOrder?> LoadOrderForPaymentAsync(int bookingOrderId)
        {
            return await _db.BookingOrders
                .Include(o => o.Bookings)
                    .ThenInclude(b => b.Seat)
                .FirstOrDefaultAsync(o => o.Id == bookingOrderId);
        }
    }
}
