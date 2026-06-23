namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int TrainId { get; set; }
        public int? TripId { get; set; }
        public int SeatId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public string? PassengerName { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? ConfirmedAtUtc { get; set; }
        public DateTime? RefundedAtUtc { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string SeatLabel { get; set; } = string.Empty;
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public decimal Amount { get; set; }
        public DateTime? TicketIssuedAtUtc { get; set; }
        public bool HasTicketArtifact { get; set; }
        public string TicketEmailStatus { get; set; } = string.Empty;
        public DateTime? TicketEmailSentAtUtc { get; set; }
        public string TicketEmailRecipient { get; set; } = string.Empty;
    }
}
