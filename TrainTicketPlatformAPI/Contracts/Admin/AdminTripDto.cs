namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminTripDto
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public string TrainCode { get; set; } = string.Empty;
        public int TrainRouteId { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
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
        public bool HasPlatformChange { get; set; }
        public bool HasDisruption { get; set; }
        public decimal Class1Price { get; set; }
        public decimal Class2Price { get; set; }
    }
}
