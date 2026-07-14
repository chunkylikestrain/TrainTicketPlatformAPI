namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class UpdateBookingRequest
    {
        public int? SeatId { get; set; }
        public int? TripId { get; set; }
        public DateTime? TravelDate { get; set; }
    }
}
