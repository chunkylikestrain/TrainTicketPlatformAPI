namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportDateResultDto
    {
        public string ExternalSource { get; set; } = "PLK";
        public DateOnly Date { get; set; }
        public bool DryRun { get; set; }
        public int RequestedCount { get; set; }
        public int SucceededCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<OpenRailwayImportDateItemResultDto> Items { get; set; } = [];
    }
}
