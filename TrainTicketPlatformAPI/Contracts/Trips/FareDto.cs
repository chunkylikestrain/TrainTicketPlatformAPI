namespace TrainTicketPlatformAPI.Contracts.Trips
{
    public class FareDto
    {
        public string ClassType { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
