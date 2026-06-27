namespace TrainTicketPlatformAPI.Models
{
    public class LoyaltyAccount
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RedeemablePoints { get; set; }
        public int PendingPoints { get; set; }
        public int ExpiringPoints { get; set; }
        public decimal RedeemableValuePln { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public User User { get; set; } = null!;
        public ICollection<LoyaltyTransaction> Transactions { get; set; } = new List<LoyaltyTransaction>();
    }
}
