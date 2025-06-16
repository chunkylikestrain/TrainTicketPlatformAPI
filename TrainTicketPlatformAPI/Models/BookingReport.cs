namespace TrainTicketPlatformAPI.Models
{
    public class BookingReport
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCancellations { get; set; }
    }
}
