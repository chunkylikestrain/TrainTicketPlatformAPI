namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayStationOnRouteDto
    {
        public int StationId { get; set; }
        public int OrderNumber { get; set; }
        public string? ArrivalCommercialCategory { get; set; }
        public string? ArrivalTrainNumber { get; set; }
        public string? ArrivalPlatform { get; set; }
        public string? ArrivalTrack { get; set; }
        public int? ArrivalDay { get; set; }
        public TimeSpan? ArrivalTime { get; set; }
        public string? DepartureCommercialCategory { get; set; }
        public string? DepartureTrainNumber { get; set; }
        public string? DeparturePlatform { get; set; }
        public string? DepartureTrack { get; set; }
        public int? DepartureDay { get; set; }
        public TimeSpan? DepartureTime { get; set; }
        public int? StopTypeId { get; set; }
        public string? StopTypeName { get; set; }
    }
}
