namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayRouteIdDto
    {
        public int ScheduleId { get; set; }
        public int OrderId { get; set; }
        public int TrainOrderId { get; set; }
        public string? Name { get; set; }
        public string? CarrierCode { get; set; }
    }
}
