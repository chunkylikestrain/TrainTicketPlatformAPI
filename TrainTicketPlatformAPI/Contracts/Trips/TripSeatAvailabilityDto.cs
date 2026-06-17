namespace TrainTicketPlatformAPI.Contracts.Trips
{
    public class TripSeatAvailabilityDto
    {
        public int SeatId { get; set; }
        public string Coach { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string ClassType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}
