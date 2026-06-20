namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminRouteDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }
        public string DepartureStationName { get; set; } = string.Empty;
        public string ArrivalStationName { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public int EstimatedDurationMinutes { get; set; }
        public string OperatingDays { get; set; } = "Daily";
        public string IntermediateStops { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
