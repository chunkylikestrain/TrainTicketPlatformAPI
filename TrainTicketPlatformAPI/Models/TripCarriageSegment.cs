namespace TrainTicketPlatformAPI.Models
{
    public class TripCarriageSegment
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int TrainCarriageId { get; set; }
        public int? FromRouteStopId { get; set; }
        public int? ToRouteStopId { get; set; }
        public string PortionCode { get; set; } = string.Empty;
        public string DestinationLabel { get; set; } = string.Empty;
        public bool IsBookable { get; set; } = true;
        public int DisplayOrder { get; set; }
        public string Notes { get; set; } = string.Empty;

        public Trip Trip { get; set; } = null!;
        public TrainCarriage TrainCarriage { get; set; } = null!;
        public TrainRouteStop? FromRouteStop { get; set; }
        public TrainRouteStop? ToRouteStop { get; set; }
    }
}
