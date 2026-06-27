using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int? BookingId { get; set; }
        public int? BookingOrderId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string PaymentMethodToken { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        [Precision(18, 2)]
        public decimal Amount { get; set; }
        public int LoyaltyPointsRedeemed { get; set; }
        [Precision(18, 2)]
        public decimal LoyaltyDiscountAmount { get; set; }

        public Booking? Booking { get; set; }
        public BookingOrder? BookingOrder { get; set; }
    }
}
