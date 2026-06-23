namespace TrainTicketPlatformAPI.Contracts.Tickets
{
    public class BookingOrderEmailDeliveryDto
    {
        public int BookingOrderId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public int RequestedCount { get; set; }
        public int SentCount { get; set; }
        public IEnumerable<TicketEmailDeliveryDto> Deliveries { get; set; } = Enumerable.Empty<TicketEmailDeliveryDto>();
    }
}
