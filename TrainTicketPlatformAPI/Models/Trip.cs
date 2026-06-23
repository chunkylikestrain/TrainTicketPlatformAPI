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

        public Train Train { get; set; } = null!;
        public TrainRoute TrainRoute { get; set; } = null!;

        public ICollection<Fare> Fares { get; set; }
            = new List<Fare>();

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}
