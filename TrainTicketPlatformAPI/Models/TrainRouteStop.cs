namespace TrainTicketPlatformAPI.Models
{
    public class TrainRouteStop
    {
        public int Id { get; set; }
        public int TrainRouteId { get; set; }
        public int StationId { get; set; }
        public int StopOrder { get; set; }

        public TrainRoute TrainRoute { get; set; } = null!;
        public Station Station { get; set; } = null!;
    }
}
