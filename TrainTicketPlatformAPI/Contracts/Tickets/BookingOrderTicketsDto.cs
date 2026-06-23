namespace TrainTicketPlatformAPI.Contracts.Tickets
{
    public class BookingOrderTicketsDto
    {
        public int BookingOrderId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public int TicketCount { get; set; }
        public IEnumerable<TicketArtifactDto> Tickets { get; set; } = Enumerable.Empty<TicketArtifactDto>();
    }
}
