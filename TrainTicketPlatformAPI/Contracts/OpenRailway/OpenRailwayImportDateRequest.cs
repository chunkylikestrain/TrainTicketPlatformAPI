namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayImportDateRequest
    {
        public int Limit { get; set; } = 25;
        public bool DryRun { get; set; }
        public List<OpenRailwayImportRouteKeyDto> Routes { get; set; } = [];
    }
}
