namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportDateItemResultDto
    {
        public int ScheduleId { get; set; }
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Error { get; set; }
        public OpenRailwayImportPreviewDto? Preview { get; set; }
        public OpenRailwayImportRouteResultDto? Import { get; set; }
    }
}
