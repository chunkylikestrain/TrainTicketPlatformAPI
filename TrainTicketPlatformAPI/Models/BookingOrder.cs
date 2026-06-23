namespace TrainTicketPlatformAPI.Models
{
    public class BookingOrder
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string BookingStatus { get; set; } = "PendingPayment";
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? ConfirmedAtUtc { get; set; }

        public User? User { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
