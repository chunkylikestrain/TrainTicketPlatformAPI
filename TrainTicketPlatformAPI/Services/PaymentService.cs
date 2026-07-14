using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.Loyalty;
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
        private readonly ILoyaltyService? _loyaltyService;

        public PaymentService(TrainTicketDbContext db, ILoyaltyService? loyaltyService = null)
        {
            _db = db;
            _loyaltyService = loyaltyService;
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int bookingId, int redeemLoyaltyPoints = 0)
        {
            var booking = await _db.Bookings
                .Include(b => b.Seat)
                .FirstOrDefaultAsync(b => b.Id == bookingId)
                ?? throw new KeyNotFoundException("Booking not found");

            EnsureBookingCanBePaid(booking);

            var fare = await GetFareForBookingAsync(booking);
            var originalAmount = GetPayableAmountBeforeLoyalty(booking, fare);
            var redemption = await CalculateRedemptionAsync(booking.UserId, originalAmount, redeemLoyaltyPoints);
            booking.LoyaltyPointsRedeemed = redemption.Points;
            booking.LoyaltyDiscountAmount = redemption.Amount;
            await _db.SaveChangesAsync();

            return new PaymentIntentDto
            {
                PaymentIntentId = BuildPaymentIntentId(booking.Id),
                BookingId = booking.Id,
                BookingIds = new[] { booking.Id },
                OriginalAmount = originalAmount,
                Amount = originalAmount - redemption.Amount,
                LoyaltyPointsRedeemed = redemption.Points,
                LoyaltyDiscountAmount = redemption.Amount,
                Currency = fare.Currency,
                Status = booking.PaymentStatus,
                ExpiresAtUtc = booking.ExpiresAtUtc
            };
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentForOrderAsync(int bookingOrderId, int redeemLoyaltyPoints = 0)
        {
            var order = await LoadOrderForPaymentAsync(bookingOrderId)
                ?? throw new KeyNotFoundException("Booking order not found");

            EnsureOrderCanBePaid(order);

            var totals = await GetOrderFareTotalAsync(order);
            var redemption = await CalculateRedemptionAsync(order.UserId, totals.Amount, redeemLoyaltyPoints);
            order.LoyaltyPointsRedeemed = redemption.Points;
            order.LoyaltyDiscountAmount = redemption.Amount;
            await ApplyOrderRedemptionToBookingsAsync(order, redemption);
            await _db.SaveChangesAsync();

            return new PaymentIntentDto
            {
                PaymentIntentId = BuildOrderPaymentIntentId(order.Id),
                BookingOrderId = order.Id,
                BookingIds = order.Bookings.Select(b => b.Id).Order().ToList(),
                OriginalAmount = totals.Amount,
                Amount = totals.Amount - redemption.Amount,
                LoyaltyPointsRedeemed = redemption.Points,
                LoyaltyDiscountAmount = redemption.Amount,
                Currency = totals.Currency,
                Status = order.PaymentStatus,
                ExpiresAtUtc = order.ExpiresAtUtc
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
            var payableAmount = GetPayableAmountBeforeLoyalty(booking, fare);
            var redemption = await ValidateStoredRedemptionAsync(booking.UserId, payableAmount, booking.LoyaltyPointsRedeemed, booking.LoyaltyDiscountAmount);
            var finalAmount = payableAmount - redemption.Amount;
            var paymentDate = DateTime.UtcNow;
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
                PaymentMethodToken = RedactPaymentMethodToken(paymentMethodToken),
                PaymentDate = paymentDate,
                Amount = finalAmount,
                LoyaltyPointsRedeemed = redemption.Points,
                LoyaltyDiscountAmount = redemption.Amount,
                Status = status
            };

            _db.Payments.Add(payment);
            booking.PaymentStatus = status;
            if (status == "Successful")
            {
                var hadStoredAmount = booking.Amount > 0m;
                booking.Amount = finalAmount;
                booking.Currency = hadStoredAmount && !string.IsNullOrWhiteSpace(booking.Currency)
                    ? booking.Currency
                    : fare.Currency;
                booking.LoyaltyPointsRedeemed = redemption.Points;
                booking.LoyaltyDiscountAmount = redemption.Amount;
                booking.BookingStatus = "Confirmed";
                booking.ConfirmedAtUtc ??= paymentDate;
                EnsureTicketMetadata(booking);
                if (_loyaltyService != null)
                {
                    await _loyaltyService.RedeemForBookingPaymentAsync(booking, redemption.Points, redemption.Amount, paymentDate);
                    await _loyaltyService.AwardTicketPurchaseAsync(booking, paymentDate);
                }
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

        private static string RedactPaymentMethodToken(string paymentMethodToken)
            => paymentMethodToken.Trim().Equals(SuccessToken, StringComparison.Ordinal)
                ? "test_success_redacted"
                : "test_failure_redacted";

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
            var redemption = await ValidateStoredRedemptionAsync(order.UserId, totals.Amount, order.LoyaltyPointsRedeemed, order.LoyaltyDiscountAmount);
            var finalAmount = totals.Amount - redemption.Amount;
            await ApplyOrderRedemptionToBookingsAsync(order, redemption);
            var paymentDate = DateTime.UtcNow;
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
                PaymentMethodToken = RedactPaymentMethodToken(paymentMethodToken),
                PaymentDate = paymentDate,
                Amount = finalAmount,
                LoyaltyPointsRedeemed = redemption.Points,
                LoyaltyDiscountAmount = redemption.Amount,
                Status = status
            };

            _db.Payments.Add(payment);
            order.PaymentStatus = status;

            if (status == "Successful" && _loyaltyService != null)
                await _loyaltyService.RedeemForOrderPaymentAsync(order, redemption.Points, redemption.Amount, paymentDate);

            foreach (var booking in order.Bookings)
            {
                booking.PaymentStatus = status;
                if (status == "Successful")
                {
                    await EnsureStoredAmountAsync(booking, forceRecalculate: booking.LoyaltyDiscountAmount > 0m);
                    booking.BookingStatus = "Confirmed";
                    booking.ConfirmedAtUtc ??= paymentDate;
                    EnsureTicketMetadata(booking);
                    if (_loyaltyService != null)
                        await _loyaltyService.AwardTicketPurchaseAsync(booking, paymentDate);
                }
            }

            if (status == "Successful")
            {
                order.LoyaltyPointsRedeemed = redemption.Points;
                order.LoyaltyDiscountAmount = redemption.Amount;
                order.BookingStatus = "Confirmed";
                order.ConfirmedAtUtc ??= paymentDate;
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
                amount += GetPayableAmountBeforeLoyalty(booking, fare);
                currency = string.IsNullOrWhiteSpace(currency)
                    ? !string.IsNullOrWhiteSpace(booking.Currency) ? booking.Currency : fare.Currency
                    : currency;
            }

            return (amount, currency);
        }

        private async Task EnsureStoredAmountAsync(Booking booking, bool forceRecalculate = false)
        {
            if (!forceRecalculate && booking.Amount > 0m && !string.IsNullOrWhiteSpace(booking.Currency))
                return;

            var fare = await GetFareForBookingAsync(booking);
            var hadStoredAmount = booking.Amount > 0m;
            var originalAmount = GetPayableAmountBeforeLoyalty(booking, fare);
            booking.Amount = Math.Max(0m, originalAmount - booking.LoyaltyDiscountAmount);
            booking.Currency = hadStoredAmount && !string.IsNullOrWhiteSpace(booking.Currency)
                ? booking.Currency
                : fare.Currency;
        }

        private async Task<LoyaltyRedemptionQuote> CalculateRedemptionAsync(
            int? userId,
            decimal payableAmount,
            int requestedPoints)
        {
            if (_loyaltyService == null)
            {
                if (requestedPoints > 0)
                    throw new InvalidOperationException("Loyalty redemption is unavailable");

                return new LoyaltyRedemptionQuote();
            }

            return await _loyaltyService.CalculateRedemptionAsync(userId, payableAmount, requestedPoints);
        }

        private async Task<LoyaltyRedemptionQuote> ValidateStoredRedemptionAsync(
            int? userId,
            decimal payableAmount,
            int storedPoints,
            decimal storedAmount)
        {
            var quote = await CalculateRedemptionAsync(userId, payableAmount, storedPoints);
            if (quote.Points != storedPoints || quote.Amount != storedAmount)
                throw new InvalidOperationException("Loyalty redemption is no longer available. Create a new payment intent.");

            return quote;
        }

        private static decimal GetPayableAmountBeforeLoyalty(Booking booking, Fare fare)
        {
            var amount = BookingPricingCalculator.GetPayableAmount(booking, fare);
            return booking.Amount > 0m && booking.LoyaltyDiscountAmount > 0m
                ? amount + booking.LoyaltyDiscountAmount
                : amount;
        }

        private async Task ApplyOrderRedemptionToBookingsAsync(BookingOrder order, LoyaltyRedemptionQuote redemption)
        {
            var bookings = order.Bookings
                .OrderBy(booking => booking.Id)
                .ToList();

            if (bookings.Count == 0)
                return;

            var originalAmounts = new Dictionary<int, decimal>();
            foreach (var booking in bookings)
            {
                var fare = await GetFareForBookingAsync(booking);
                originalAmounts[booking.Id] = GetPayableAmountBeforeLoyalty(booking, fare);
            }

            var total = originalAmounts.Values.Sum();
            var remainingAmount = redemption.Amount;
            var remainingPoints = redemption.Points;

            for (var index = 0; index < bookings.Count; index++)
            {
                var booking = bookings[index];
                var originalAmount = originalAmounts[booking.Id];
                var isLast = index == bookings.Count - 1;
                var discountAmount = isLast || total <= 0m
                    ? remainingAmount
                    : Math.Round(redemption.Amount * (originalAmount / total), 2, MidpointRounding.AwayFromZero);
                var points = isLast || total <= 0m
                    ? remainingPoints
                    : (int)Math.Round(redemption.Points * (originalAmount / total), MidpointRounding.AwayFromZero);

                discountAmount = Math.Min(discountAmount, originalAmount);
                points = Math.Min(points, remainingPoints);

                booking.LoyaltyDiscountAmount = discountAmount;
                booking.LoyaltyPointsRedeemed = points;
                booking.Amount = Math.Max(0m, originalAmount - discountAmount);

                remainingAmount = Math.Max(0m, remainingAmount - discountAmount);
                remainingPoints = Math.Max(0, remainingPoints - points);
            }
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
