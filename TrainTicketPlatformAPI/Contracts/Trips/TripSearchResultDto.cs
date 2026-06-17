namespace TrainTicketPlatformAPI.Contracts.Trips
{
    public class TripSearchResultDto
    {
        public int TripId { get; set; }
        public int TrainId { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public string DepartureStationCode { get; set; } = string.Empty;
        public string DepartureStationName { get; set; } = string.Empty;
        public string ArrivalStationCode { get; set; } = string.Empty;
        public string ArrivalStationName { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? LowestFare { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
