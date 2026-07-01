namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayStationsResponseDto
    {
        public DateTime GeneratedAt { get; set; }
        public List<OpenRailwayStationDto>? Stations { get; set; }
        public int TotalCount { get; set; }
        public int ReturnedCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
