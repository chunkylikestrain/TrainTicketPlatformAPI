namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayRouteDto
    {
        public int ScheduleId { get; set; }
        public int OrderId { get; set; }
        public int TrainOrderId { get; set; }
        public string? Name { get; set; }
        public string? CarrierCode { get; set; }
        public string? NationalNumber { get; set; }
        public string? InternationalArrivalNumber { get; set; }
        public string? InternationalDepartureNumber { get; set; }
        public string? CommercialCategorySymbol { get; set; }
        public List<DateOnly>? OperatingDates { get; set; }
        public List<OpenRailwayStationOnRouteDto>? Stations { get; set; }
    }
}
