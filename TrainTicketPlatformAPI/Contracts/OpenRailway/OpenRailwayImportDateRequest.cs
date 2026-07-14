namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportDateRequest
    {
        public int Limit { get; set; } = 25;
        public bool DryRun { get; set; }
        public bool ConfirmApply { get; set; }
        public string ConfirmationText { get; set; } = string.Empty;
        public List<OpenRailwayImportRouteKeyDto> Routes { get; set; } = [];
    }
}
