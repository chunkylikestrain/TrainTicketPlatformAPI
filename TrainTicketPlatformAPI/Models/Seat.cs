namespace TrainTicketPlatformAPI.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public string Coach { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string ClassType { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }

        public Train Train { get; set; } = null!;
        public ICollection<Booking> Bookings { get; set; } 
            = new List<Booking>();
    }
}
