namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class BookingOrderDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? ConfirmedAtUtc { get; set; }
        public decimal Amount { get; set; }
        public int TicketCount { get; set; }
        public bool HasTicketArtifacts { get; set; }
        public IEnumerable<BookingDto> Bookings { get; set; } = Enumerable.Empty<BookingDto>();
    }
}
