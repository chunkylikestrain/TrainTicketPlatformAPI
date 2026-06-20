namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminTripDto
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public string TrainCode { get; set; } = string.Empty;
        public int TrainRouteId { get; set; }
        public string RouteCode { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Track { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public decimal Class1Price { get; set; }
        public decimal Class2Price { get; set; }
    }
}
