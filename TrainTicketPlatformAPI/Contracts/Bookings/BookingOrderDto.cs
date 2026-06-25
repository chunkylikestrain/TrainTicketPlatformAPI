namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class BookingOrderDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public string? ItineraryId { get; set; }
        public bool IsItinerary { get; set; }
        public int SegmentCount { get; set; }
        public int? JourneyDepartureStationId { get; set; }
        public int? JourneyArrivalStationId { get; set; }
        public DateTime? JourneyDepartureTime { get; set; }
        public DateTime? JourneyArrivalTime { get; set; }
        public string? GuestEmail { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime? ConfirmedAtUtc { get; set; }
        public decimal Amount { get; set; }
        public int TicketCount { get; set; }
        public bool HasTicketArtifacts { get; set; }
        public IEnumerable<BookingOrderSegmentDto> Segments { get; set; } = Enumerable.Empty<BookingOrderSegmentDto>();
        public IEnumerable<BookingDto> Bookings { get; set; } = Enumerable.Empty<BookingDto>();
    }

    public class BookingOrderSegmentDto
    {
        public int SegmentIndex { get; set; }
        public int? TripId { get; set; }
        public int TrainId { get; set; }
        public string TrainName { get; set; } = string.Empty;
        public int? DepartureStationId { get; set; }
        public int? ArrivalStationId { get; set; }
        public string Route { get; set; } = string.Empty;
        public DateTime? DepartureTime { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public IEnumerable<BookingDto> Tickets { get; set; } = Enumerable.Empty<BookingDto>();
    }
}
