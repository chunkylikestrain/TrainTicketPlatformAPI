namespace TrainTicketPlatformAPI.Contracts.Payments
{
    public class PaymentIntentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public int? BookingOrderId { get; set; }
        public IEnumerable<int> BookingIds { get; set; } = Enumerable.Empty<int>();
        public decimal OriginalAmount { get; set; }
        public decimal Amount { get; set; }
        public int LoyaltyPointsRedeemed { get; set; }
        public decimal LoyaltyDiscountAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ExpiresAtUtc { get; set; }
    }
}
