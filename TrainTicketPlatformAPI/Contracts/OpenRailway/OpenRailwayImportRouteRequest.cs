namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportRouteRequest
    {
        public DateOnly? OperatingDate { get; set; }
        public bool ConfirmApply { get; set; }
        public string ConfirmationText { get; set; } = string.Empty;
    }
}
