namespace TrainTicketPlatformAPI.Contracts.Loyalty
{
    public class LoyaltyTransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Points { get; set; }
        public decimal SourceAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDateUtc { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public int? BookingId { get; set; }
        public int? BookingOrderId { get; set; }
    }
}
