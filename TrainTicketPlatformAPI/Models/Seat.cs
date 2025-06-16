namespace TrainTicketPlatformAPI.Models
{
    public class Seat
    {
        public int Id { get; set; }
        public int TrainId { get; set; }
        public string Coach { get; set; }
        public string Number { get; set; }
        public string ClassType { get; set; }
        public bool IsAvailable { get; set; }

        public Train Train { get; set; }
        public ICollection<Booking> Bookings { get; set; } 
            = new List<Booking>();
    }
}
