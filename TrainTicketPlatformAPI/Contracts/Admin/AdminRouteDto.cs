namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminRouteDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AdminDisplayName { get; set; } = string.Empty;
        public string RouteFingerprint { get; set; } = string.Empty;
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }
        public string DepartureStationName { get; set; } = string.Empty;
        public string ArrivalStationName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public string OperatingDays { get; set; } = "Daily";
        public string IntermediateStops { get; set; } = string.Empty;
        public List<int> IntermediateStopStationIds { get; set; } = [];
        public List<AdminRouteStopDto> Stops { get; set; } = [];
        public bool IsActive { get; set; }
    }

    public class AdminRouteStopDto
    {
        public int StationId { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public int StopOrder { get; set; }
        public int? ArrivalOffsetMinutes { get; set; }
        public int? DepartureOffsetMinutes { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string StopType { get; set; } = string.Empty;
    }
}
