using TrainTicketPlatformAPI.Models;
using TrainTicketPlatformAPI.Services;

namespace TrainTicketPlatformAPI.Tests
{
    public class BookingRefundPolicyTests
    {
        [Test]
        public void Evaluate_ReturnsFullRefund_WhenDepartureIsMoreThanTwentyFourHoursAway()
        {
            var now = new DateTime(2026, 6, 28, 8, 0, 0, DateTimeKind.Utc);
            var booking = CreateConfirmedBooking(now.AddHours(30), 120m);

            var result = BookingRefundPolicy.Evaluate(booking, now);

            Assert.That(result.IsEligible, Is.True);
            Assert.That(result.Code, Is.EqualTo("FullRefund"));
            Assert.That(result.RefundableAmount, Is.EqualTo(120m));
            Assert.That(result.FeeAmount, Is.EqualTo(0m));
        }

        [Test]
        public void Evaluate_AppliesTenPercentFee_WhenDepartureIsInsideTwentyFourHours()
        {
            var now = new DateTime(2026, 6, 28, 8, 0, 0, DateTimeKind.Utc);
            var booking = CreateConfirmedBooking(now.AddHours(6), 100m);

            var result = BookingRefundPolicy.Evaluate(booking, now);

            Assert.That(result.IsEligible, Is.True);
            Assert.That(result.Code, Is.EqualTo("TenPercentFee"));
            Assert.That(result.RefundableAmount, Is.EqualTo(90m));
            Assert.That(result.FeeAmount, Is.EqualTo(10m));
        }

        [Test]
        public void Evaluate_AppliesFiftyPercentFee_WhenDepartureIsInsideTwoHours()
        {
            var now = new DateTime(2026, 6, 28, 8, 0, 0, DateTimeKind.Utc);
            var booking = CreateConfirmedBooking(now.AddMinutes(90), 100m);

            var result = BookingRefundPolicy.Evaluate(booking, now);

            Assert.That(result.IsEligible, Is.True);
            Assert.That(result.Code, Is.EqualTo("FiftyPercentFee"));
            Assert.That(result.RefundableAmount, Is.EqualTo(50m));
            Assert.That(result.FeeAmount, Is.EqualTo(50m));
        }

        [Test]
        public void Evaluate_ClosesSelfServiceRefund_WhenDepartureIsInsideThirtyMinutes()
        {
            var now = new DateTime(2026, 6, 28, 8, 0, 0, DateTimeKind.Utc);
            var booking = CreateConfirmedBooking(now.AddMinutes(20), 100m);

            var result = BookingRefundPolicy.Evaluate(booking, now);

            Assert.That(result.IsEligible, Is.False);
            Assert.That(result.Code, Is.EqualTo("Closed"));
            Assert.That(result.RefundableAmount, Is.EqualTo(0m));
        }

        [Test]
        public void Evaluate_ReturnsFullRefund_WhenServiceIsCancelled()
        {
            var now = new DateTime(2026, 6, 28, 8, 0, 0, DateTimeKind.Utc);
            var booking = CreateConfirmedBooking(now.AddMinutes(10), 100m);
            booking.Trip!.Status = "Cancelled";
            booking.Trip.CancellationReason = "Operational disruption";

            var result = BookingRefundPolicy.Evaluate(booking, now);

            Assert.That(result.IsEligible, Is.True);
            Assert.That(result.Code, Is.EqualTo("ServiceCancelled"));
            Assert.That(result.RefundableAmount, Is.EqualTo(100m));
            Assert.That(result.FeeAmount, Is.EqualTo(0m));
        }

        private static Booking CreateConfirmedBooking(DateTime departure, decimal amount)
            => new()
            {
                BookingStatus = "Confirmed",
                PaymentStatus = "Successful",
                Amount = amount,
                TravelDate = departure.Date,
                SegmentDepartureTime = departure,
                Trip = new Trip
                {
                    DepartureTime = departure,
                    ArrivalTime = departure.AddHours(3),
                    Status = "Scheduled"
                }
            };
    }
}
