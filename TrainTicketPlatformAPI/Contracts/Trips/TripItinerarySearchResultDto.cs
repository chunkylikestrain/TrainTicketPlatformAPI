namespace TrainTicketPlatformAPI.Contracts.Trips
{
    public class TripItinerarySearchResultDto
    {
        public string ItineraryId { get; set; } = string.Empty;
        public int TransferCount { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public int TotalDurationMinutes { get; set; }
        public int TotalTransferMinutes { get; set; }
        public decimal? LowestFare { get; set; }
        public string Currency { get; set; } = string.Empty;
        public IEnumerable<TripItinerarySegmentDto> Segments { get; set; } =
            Enumerable.Empty<TripItinerarySegmentDto>();
    }
}
