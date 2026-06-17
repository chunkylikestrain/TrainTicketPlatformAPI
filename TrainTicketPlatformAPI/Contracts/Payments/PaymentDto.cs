namespace TrainTicketPlatformAPI.Contracts.Payments
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
