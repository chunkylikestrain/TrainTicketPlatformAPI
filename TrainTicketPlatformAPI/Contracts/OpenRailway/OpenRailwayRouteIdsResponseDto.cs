namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayRouteIdsResponseDto
    {
        public DateTime GeneratedAt { get; set; }
        public DateOnly Date { get; set; }
        public int Count { get; set; }
        public List<OpenRailwayRouteIdDto>? Routes { get; set; }
    }
}
