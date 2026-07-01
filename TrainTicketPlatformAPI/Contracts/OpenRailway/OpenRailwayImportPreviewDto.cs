namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportPreviewDto
    {
        public string ExternalSource { get; set; } = "PLK";
        public int ScheduleId { get; set; }
        public int OrderId { get; set; }
        public int TrainOrderId { get; set; }
        public string TrainCode { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public string CarrierCode { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public IReadOnlyList<DateOnly> OperatingDates { get; set; } = [];
        public IReadOnlyList<OpenRailwayImportStopPreviewDto> Stops { get; set; } = [];
    }
}
