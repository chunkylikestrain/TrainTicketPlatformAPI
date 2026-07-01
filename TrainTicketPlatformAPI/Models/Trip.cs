namespace TrainTicketPlatformAPI.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public int TrainRouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public int DelayMinutes { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public string OriginalPlatform { get; set; } = string.Empty;
        public string OriginalTrack { get; set; } = string.Empty;
        public string DisruptionMessage { get; set; } = string.Empty;
        public string DisruptionSeverity { get; set; } = string.Empty;
        public string ExternalSource { get; set; } = string.Empty;
        public int? ExternalScheduleId { get; set; }
        public int? ExternalOrderId { get; set; }
        public int? ExternalTrainOrderId { get; set; }
        public DateOnly? ExternalOperatingDate { get; set; }
        public DateTime? ExternalImportedAtUtc { get; set; }
        public string ExternalRawVersion { get; set; } = string.Empty;

        public Train Train { get; set; } = null!;
        public TrainRoute TrainRoute { get; set; } = null!;

        public ICollection<Fare> Fares { get; set; }
            = new List<Fare>();

        public ICollection<TripServiceIdentity> ServiceIdentities { get; set; }
            = new List<TripServiceIdentity>();

        public ICollection<TripCarriageSegment> CarriageSegments { get; set; }
            = new List<TripCarriageSegment>();

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}
