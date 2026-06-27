namespace TrainTicketPlatformAPI.Contracts.Loyalty
{
    public class LoyaltyAccountDto
    {
        public int UserId { get; set; }
        public int RedeemablePoints { get; set; }
        public int PendingPoints { get; set; }
        public int ExpiringPoints { get; set; }
        public decimal RedeemableValuePln { get; set; }
        public decimal EarnRatePointsPerPln { get; set; }
        public int RedeemRatePointsPerPln { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
