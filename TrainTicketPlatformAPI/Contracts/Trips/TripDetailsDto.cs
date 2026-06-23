namespace TrainTicketPlatformAPI.Contracts.Trips
{
    public class TripDetailsDto
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
        public decimal DistanceKm { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public IEnumerable<FareDto> Fares { get; set; } = Enumerable.Empty<FareDto>();
    }
}
