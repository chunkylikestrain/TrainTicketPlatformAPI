namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TrainId { get; set; }
        public int? TripId { get; set; }
        public int SeatId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime TravelDate { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsCancelled { get; set; }
        public DateTime? CancellationDate { get; set; }
    }
}
