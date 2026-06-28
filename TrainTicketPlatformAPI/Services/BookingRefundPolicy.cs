using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public sealed class BookingRefundPolicyResult
    {
        public bool IsEligible { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public decimal RefundableAmount { get; init; }
        public decimal FeeAmount { get; init; }
        public DateTime? DeadlineUtc { get; init; }
    }

    public static class BookingRefundPolicy
    {
        private static readonly TimeSpan FullRefundWindow = TimeSpan.FromHours(24);
        private static readonly TimeSpan ReducedRefundWindow = TimeSpan.FromHours(2);
        private static readonly TimeSpan FinalRefundWindow = TimeSpan.FromMinutes(30);

        public static BookingRefundPolicyResult Evaluate(Booking booking, DateTime nowUtc)
        {
            var amount = Math.Max(booking.Amount, 0m);
            var departureTime = GetEffectiveDepartureTime(booking);
            var serviceCancelled = IsServiceCancelled(booking);

            if (booking.BookingStatus == "Refunded" || booking.PaymentStatus == "Refunded" || booking.RefundedAtUtc != null)
                return Ineligible("AlreadyRefunded", "This ticket has already been returned.", amount, departureTime);

            if (booking.IsCancelled || booking.BookingStatus == "Cancelled")
                return Ineligible("AlreadyCancelled", "This ticket has already been cancelled.", amount, departureTime);

            if (booking.BookingStatus != "Confirmed" || booking.PaymentStatus != "Successful")
                return Ineligible("NotPaidConfirmed", "Only paid confirmed tickets can be refunded.", amount, departureTime);

            if (serviceCancelled)
                return Eligible("ServiceCancelled", "The service is cancelled, so the ticket is eligible for a full refund.", amount, 0m, null);

            if (departureTime == null)
                return Ineligible("MissingDepartureTime", "Refund policy cannot be calculated because the departure time is missing.", amount, null);

            var timeToDeparture = departureTime.Value - nowUtc;
            if (timeToDeparture <= TimeSpan.Zero)
                return Ineligible("Departed", "This train has already departed, so self-service refund is closed.", amount, departureTime);

            if (timeToDeparture >= FullRefundWindow)
                return Eligible("FullRefund", "Refundable in full until 24 hours before departure.", amount, 0m, departureTime.Value - FullRefundWindow);

            if (timeToDeparture >= ReducedRefundWindow)
                return WithFee("TenPercentFee", "A 10% refund fee applies inside 24 hours before departure.", amount, 0.10m, departureTime.Value - ReducedRefundWindow);

            if (timeToDeparture >= FinalRefundWindow)
                return WithFee("FiftyPercentFee", "A 50% refund fee applies inside 2 hours before departure.", amount, 0.50m, departureTime.Value - FinalRefundWindow);

            return Ineligible("Closed", "Self-service refund closes 30 minutes before departure.", amount, departureTime);
        }

        private static BookingRefundPolicyResult Eligible(
            string code,
            string message,
            decimal amount,
            decimal fee,
            DateTime? deadlineUtc)
        {
            var roundedFee = Math.Round(fee, 2, MidpointRounding.AwayFromZero);
            return new BookingRefundPolicyResult
            {
                IsEligible = true,
                Code = code,
                Message = message,
                RefundableAmount = Math.Max(amount - roundedFee, 0m),
                FeeAmount = roundedFee,
                DeadlineUtc = deadlineUtc
            };
        }

        private static BookingRefundPolicyResult WithFee(
            string code,
            string message,
            decimal amount,
            decimal feePercent,
            DateTime? deadlineUtc)
            => Eligible(code, message, amount, amount * feePercent, deadlineUtc);

        private static BookingRefundPolicyResult Ineligible(
            string code,
            string message,
            decimal amount,
            DateTime? departureTime)
            => new()
            {
                IsEligible = false,
                Code = code,
                Message = message,
                RefundableAmount = 0m,
                FeeAmount = amount,
                DeadlineUtc = departureTime
            };

        private static DateTime? GetEffectiveDepartureTime(Booking booking)
            => booking.SegmentDepartureTime ?? booking.Trip?.DepartureTime ?? booking.Train?.DepartureTime ?? booking.TravelDate;

        private static bool IsServiceCancelled(Booking booking)
            => booking.Trip != null &&
               (string.Equals(booking.Trip.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                !string.IsNullOrWhiteSpace(booking.Trip.CancellationReason));
    }
}
