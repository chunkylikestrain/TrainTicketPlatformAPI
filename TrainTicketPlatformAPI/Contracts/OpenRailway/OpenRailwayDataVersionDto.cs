namespace TrainTicketPlatformAPI.Contracts.OpenRailway
{
    public class OpenRailwayDataVersionDto
    {
        public Guid? DataVersion { get; set; }
        public Guid? SchedulesVersion { get; set; }
        public Guid? OperationsVersion { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
