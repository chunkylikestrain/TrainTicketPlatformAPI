namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportStopPreviewDto
    {
        public int ExternalStationId { get; set; }
        public int OrderNumber { get; set; }
        public string? Arrival { get; set; }
        public string? Departure { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public int? StopTypeId { get; set; }
        public string StopTypeName { get; set; } = string.Empty;
    }
}
