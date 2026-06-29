using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    public static class BookingPricingCalculator
    {
        public const decimal DogTicketPrice = 15m;
        public const decimal LargeBaggageTicketPrice = 5m;

        private static readonly IReadOnlyDictionary<string, DiscountDefinition> Discounts =
            new Dictionary<string, DiscountDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                ["normal"] = new("normal", "Normal Ticket", 0m, "adult", true),
                ["student51"] = new("student51", "Student 51%", 51m, "adult", true),
                ["student37"] = new("student37", "Student 37%", 37m, "adult", false),
                ["child37"] = new("child37", "Child 37%", 37m, "child", false),
                ["senior30"] = new("senior30", "Senior 30%", 30m, "adult", true),
                ["senior37"] = new("senior37", "Senior statutory 37%", 37m, "adult", false),
                ["bigFamily30"] = new("bigFamily30", "Big Family 30%", 30m, "all", true),
                ["family30"] = new("family30", "Family Ticket 30%", 30m, "all", true),
                ["largeFamily50"] = new("largeFamily50", "Large Family 50%", 50m, "all", true)
            };

        public static TicketPrice Calculate(Booking booking, Fare fare)
        {
            var passengerType = NormalizePassengerType(booking.PassengerType);
            var discountCode = NormalizeDiscountCode(booking.DiscountCode, passengerType);
            var discount = Discounts[discountCode];

            if (discount.AppliesTo != "all" &&
                !string.Equals(discount.AppliesTo, passengerType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"{discount.Name} is not valid for {passengerType} passengers");
            }

            if (!discount.AllClasses &&
                !string.Equals(fare.ClassType, "Class 2", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(fare.ClassType, "Economy", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"{discount.Name} can only be used in second class");
            }

            var ticketAmount = decimal.Round(fare.Price * (100m - discount.Percent) / 100m, 2, MidpointRounding.AwayFromZero);
            var extraChargeAmount = GetExtraChargeAmount(booking);
            var amount = ticketAmount + extraChargeAmount;
            return new TicketPrice(
                passengerType,
                discount.Code,
                discount.Name,
                discount.Percent,
                fare.Price,
                extraChargeAmount,
                amount,
                fare.Currency);
        }

        public static decimal GetPayableAmount(Booking booking, Fare fare)
        {
            if (booking.Amount > 0m)
                return booking.Amount;

            return Calculate(booking, fare).Amount;
        }

        public static decimal GetExtraChargeAmount(Booking booking)
            => (NormalizeDogTicketCount(booking.DogTicketCount) * DogTicketPrice) +
               (NormalizeLargeBaggageTicketCount(booking.LargeBaggageTicketCount) * LargeBaggageTicketPrice);

        public static int NormalizeDogTicketCount(int count)
            => Math.Clamp(count, 0, 1);

        public static int NormalizeLargeBaggageTicketCount(int count)
            => Math.Clamp(count, 0, 10);

        public static string NormalizePassengerType(string? passengerType)
        {
            if (string.Equals(passengerType, "child", StringComparison.OrdinalIgnoreCase))
                return "Child";

            return "Adult";
        }

        private static string NormalizeDiscountCode(string? discountCode, string passengerType)
        {
            if (!string.IsNullOrWhiteSpace(discountCode) &&
                Discounts.ContainsKey(discountCode.Trim()))
            {
                return discountCode.Trim();
            }

            return string.Equals(passengerType, "Child", StringComparison.OrdinalIgnoreCase)
                ? "child37"
                : "normal";
        }

        private sealed record DiscountDefinition(
            string Code,
            string Name,
            decimal Percent,
            string AppliesTo,
            bool AllClasses);

        public sealed record TicketPrice(
            string PassengerType,
            string DiscountCode,
            string DiscountName,
            decimal DiscountPercent,
            decimal BaseAmount,
            decimal ExtraChargeAmount,
            decimal Amount,
            string Currency);
    }
}
