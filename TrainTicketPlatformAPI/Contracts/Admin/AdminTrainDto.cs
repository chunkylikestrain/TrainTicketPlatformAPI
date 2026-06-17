namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminTrainDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
    }
}
