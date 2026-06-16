namespace TrainTicketPlatformAPI.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        public ICollection<TrainRoute> DepartureRoutes { get; set; }
            = new List<TrainRoute>();

        public ICollection<TrainRoute> ArrivalRoutes { get; set; }
            = new List<TrainRoute>();
    }
}
