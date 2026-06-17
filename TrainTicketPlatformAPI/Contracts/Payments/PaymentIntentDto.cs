namespace TrainTicketPlatformAPI.Contracts.Payments
{
    public class PaymentIntentDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ExpiresAtUtc { get; set; }
        public IEnumerable<string> TestPaymentMethodTokens { get; set; } = Enumerable.Empty<string>();
    }
}
