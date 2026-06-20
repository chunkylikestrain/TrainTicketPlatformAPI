namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminTrainDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "InterCity";
        public int CarriageCount { get; set; }
        public int SeatsPerCarriage { get; set; }
        public string Status { get; set; } = "Active";
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
