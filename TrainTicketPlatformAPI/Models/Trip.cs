namespace TrainTicketPlatformAPI.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public int TrainRouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Status { get; set; } = "Scheduled";

        public Train Train { get; set; }
        public TrainRoute TrainRoute { get; set; }

        public ICollection<Fare> Fares { get; set; }
            = new List<Fare>();

        public ICollection<Booking> Bookings { get; set; }
            = new List<Booking>();
    }
}
