namespace TrainTicketPlatformAPI.Models
{
    public class TicketEmailDelivery
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string Status { get; set; } = "Sent";
        public DateTime RequestedAtUtc { get; set; }
        public DateTime? SentAtUtc { get; set; }
        public string ProviderMessageId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public Booking Booking { get; set; } = null!;
    }
}
