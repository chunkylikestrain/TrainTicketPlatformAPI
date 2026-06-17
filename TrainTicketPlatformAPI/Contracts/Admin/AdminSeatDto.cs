namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminSeatDto
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public string Coach { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string ClassType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
