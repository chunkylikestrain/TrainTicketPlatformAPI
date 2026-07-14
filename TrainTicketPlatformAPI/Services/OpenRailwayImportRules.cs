namespace TrainTicketPlatformAPI.Services
{
    public static class OpenRailwayImportRules
    {
        public static bool IsInterCityCarrier(string? carrierCode)
            => string.Equals(carrierCode?.Trim(), "IC", StringComparison.OrdinalIgnoreCase);
    }
}
