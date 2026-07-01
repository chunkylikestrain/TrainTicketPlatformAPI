namespace TrainTicketPlatformAPI.Models
{
    public class TripServiceIdentity
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int DisplayOrder { get; set; }
        public string CarrierCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string ServiceCategory { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string DisplayNumber { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public int? FromRouteStopId { get; set; }
        public int? ToRouteStopId { get; set; }
        public bool IsPrimary { get; set; }
        public string ExternalSource { get; set; } = string.Empty;

        public Trip Trip { get; set; } = null!;
        public TrainRouteStop? FromRouteStop { get; set; }
        public TrainRouteStop? ToRouteStop { get; set; }
    }
}
