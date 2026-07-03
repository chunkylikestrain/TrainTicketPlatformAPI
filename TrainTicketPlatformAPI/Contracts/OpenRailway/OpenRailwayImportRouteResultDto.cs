namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportRouteResultDto
    {
        public string ExternalSource { get; set; } = "PLK";
        public int ScheduleId { get; set; }
        public int OrderId { get; set; }
        public DateOnly OperatingDate { get; set; }
        public int TrainId { get; set; }
        public int TrainRouteId { get; set; }
        public int TripId { get; set; }
        public bool TrainCreated { get; set; }
        public bool RouteCreated { get; set; }
        public bool RouteReused { get; set; }
        public bool TripCreated { get; set; }
        public bool DefaultConsistApplied { get; set; }
        public int StationsCreated { get; set; }
        public int CarriagesCreated { get; set; }
        public int SeatsCreated { get; set; }
        public int StopsWritten { get; set; }
        public string TrainCode { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string AdminDisplayName { get; set; } = string.Empty;
        public string RouteFingerprint { get; set; } = string.Empty;
    }
}
