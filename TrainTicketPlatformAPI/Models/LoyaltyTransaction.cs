namespace TrainTicketPlatformAPI.Models
{
    public class LoyaltyTransaction
    {
        public int Id { get; set; }
        public int LoyaltyAccountId { get; set; }
        public int? BookingId { get; set; }
        public int? BookingOrderId { get; set; }
        public string Type { get; set; } = "TicketPurchase";
        public string Status { get; set; } = "Available";
        public int Points { get; set; }
        public decimal SourceAmount { get; set; }
        public string Currency { get; set; } = "PLN";
        public DateTime TransactionDateUtc { get; set; }
        public DateTime ValidFromUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public LoyaltyAccount LoyaltyAccount { get; set; } = null!;
        public Booking? Booking { get; set; }
        public BookingOrder? BookingOrder { get; set; }
    }
}
