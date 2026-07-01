namespace TrainTicketPlatformAPI.Services
{
    public class OpenRailwayOptions
    {
        public const string SectionName = "OpenRailway";

        public string BaseUrl { get; set; } = "https://pdp-api.plk-sa.pl";
        public string ApiKey { get; set; } = string.Empty;
        public string ExternalSource { get; set; } = "PLK";
    }
}
