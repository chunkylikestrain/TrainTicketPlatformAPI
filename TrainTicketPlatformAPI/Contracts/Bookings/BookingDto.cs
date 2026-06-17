namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TrainId { get; set; }
        public int? TripId { get; set; }
        public int SeatId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public DateTime? CancellationDate { get; set; }
    }
}
