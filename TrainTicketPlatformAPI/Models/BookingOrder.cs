namespace TrainTicketPlatformAPI.Models
{
    public class BookingOrder
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public string TripType { get; set; } = "OneWay";
        public string? ItineraryId { get; set; }
        public bool IsItinerary { get; set; }
        public int SegmentCount { get; set; } = 1;
        public int? JourneyDepartureStationId { get; set; }
        public int? JourneyArrivalStationId { get; set; }
        public DateTime? JourneyDepartureTime { get; set; }
        public DateTime? JourneyArrivalTime { get; set; }
        public string? GuestEmail { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
        public string BookingStatus { get; set; } = "PendingPayment";
        public string PaymentStatus { get; set; } = "Pending";
        public int LoyaltyPointsRedeemed { get; set; }
        public decimal LoyaltyDiscountAmount { get; set; }
        public DateTime? ConfirmedAtUtc { get; set; }

        public User? User { get; set; }
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
