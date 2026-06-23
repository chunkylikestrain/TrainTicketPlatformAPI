namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class BookingDto
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
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public int DelayMinutes { get; set; }
        public string TripCancellationReason { get; set; } = string.Empty;
        public string OriginalPlatform { get; set; } = string.Empty;
        public string OriginalTrack { get; set; } = string.Empty;
        public string DisruptionMessage { get; set; } = string.Empty;
        public string DisruptionSeverity { get; set; } = string.Empty;
        public bool HasPlatformChange { get; set; }
        public bool HasDisruption { get; set; }
        public decimal Amount { get; set; }
        public DateTime? TicketIssuedAtUtc { get; set; }
        public bool HasTicketArtifact { get; set; }
        public string TicketEmailStatus { get; set; } = string.Empty;
        public DateTime? TicketEmailSentAtUtc { get; set; }
        public string TicketEmailRecipient { get; set; } = string.Empty;
    }
}
