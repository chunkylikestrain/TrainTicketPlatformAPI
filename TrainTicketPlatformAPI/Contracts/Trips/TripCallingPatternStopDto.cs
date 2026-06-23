namespace TrainTicketPlatformAPI.Contracts.Trips
{
    public class TripCallingPatternStopDto
    {
        public int StationId { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }
        public int? ArrivalOffsetMinutes { get; set; }
        public int? DepartureOffsetMinutes { get; set; }
        public int DwellMinutes { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string StopType { get; set; } = string.Empty;
    }
}
