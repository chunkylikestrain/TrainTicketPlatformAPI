namespace TrainTicketPlatformAPI.Contracts.Trips
{
    public class TripSearchResultDto
    {
        public int TripId { get; set; }
        public int TrainId { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public int DepartureStationId { get; set; }
        public string DepartureStationCode { get; set; } = string.Empty;
        public string DepartureStationName { get; set; } = string.Empty;
        public int ArrivalStationId { get; set; }
        public string ArrivalStationCode { get; set; } = string.Empty;
        public string ArrivalStationName { get; set; } = string.Empty;
        public int DepartureStopOrder { get; set; }
        public int ArrivalStopOrder { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int DelayMinutes { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
        public string OriginalPlatform { get; set; } = string.Empty;
        public string OriginalTrack { get; set; } = string.Empty;
        public string DisruptionMessage { get; set; } = string.Empty;
        public string DisruptionSeverity { get; set; } = string.Empty;
        public bool HasPlatformChange { get; set; }
        public bool HasDisruption { get; set; }
        public decimal? LowestFare { get; set; }
        public string Currency { get; set; } = string.Empty;
        public IEnumerable<TripCallingPatternStopDto> CallingPattern { get; set; } =
            Enumerable.Empty<TripCallingPatternStopDto>();
    }
}
