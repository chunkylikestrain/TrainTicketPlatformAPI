namespace TrainTicketPlatformAPI.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TrainId { get; set; }
        public int SeatId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime TravelDate { get; set; }
        public string PaymentStatus { get; set; }
        public bool IsCancelled { get; set; } = false;
        public DateTime? CancellationDate { get; set; }

        public User User { get; set; }
        public Train Train { get; set; }
        public Seat Seat { get; set; }
    }
}
