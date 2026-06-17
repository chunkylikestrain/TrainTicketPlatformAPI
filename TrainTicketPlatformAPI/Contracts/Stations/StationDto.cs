namespace TrainTicketPlatformAPI.Contracts.Stations
{
    public class StationDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int? CountryId { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public int? StateRegionId { get; set; }
        public string StateRegionCode { get; set; } = string.Empty;
        public string StateRegionName { get; set; } = string.Empty;
        public int? LocalityId { get; set; }
        public string LocalityName { get; set; } = string.Empty;
        public string LocalityType { get; set; } = string.Empty;
    }
}
