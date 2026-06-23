namespace TrainTicketPlatformAPI.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int TrainId { get; set; }
        public int? TripId { get; set; }
        public int SeatId { get; set; }
        public int? SegmentDepartureStationId { get; set; }
        public int? SegmentArrivalStationId { get; set; }
        public int? SegmentDepartureOrder { get; set; }
        public int? SegmentArrivalOrder { get; set; }
        public DateTime? SegmentDepartureTime { get; set; }
        public DateTime? SegmentArrivalTime { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string TicketNumber { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public string? PassengerName { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime TravelDate { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string BookingStatus { get; set; } = "PendingPayment";
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsCancelled { get; set; } = false;
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? ConfirmedAtUtc { get; set; }
        public DateTime? RefundedAtUtc { get; set; }
        public DateTime? TicketIssuedAtUtc { get; set; }
        public string TicketQrPayload { get; set; } = string.Empty;
        public string TicketEmailStatus { get; set; } = string.Empty;
        public DateTime? TicketEmailSentAtUtc { get; set; }
        public string TicketEmailRecipient { get; set; } = string.Empty;

        public User? User { get; set; }
        public Train Train { get; set; } = null!;
        public Trip? Trip { get; set; }
        public Seat Seat { get; set; } = null!;
        public Station? SegmentDepartureStation { get; set; }
        public Station? SegmentArrivalStation { get; set; }
        public ICollection<TicketEmailDelivery> TicketEmailDeliveries { get; set; } = new List<TicketEmailDelivery>();
    }
}
