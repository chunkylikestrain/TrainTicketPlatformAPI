namespace TrainTicketPlatformAPI.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string NormalizedCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ExternalSource { get; set; } = string.Empty;
        public int? ExternalStationId { get; set; }
        public int? CountryId { get; set; }
        public int? StateRegionId { get; set; }
        public int? LocalityId { get; set; }

        public Country? Country { get; set; }
        public StateRegion? StateRegion { get; set; }
        public Locality? Locality { get; set; }

        public ICollection<TrainRoute> DepartureRoutes { get; set; }
            = new List<TrainRoute>();

        public ICollection<TrainRoute> ArrivalRoutes { get; set; }
            = new List<TrainRoute>();
    }
}
