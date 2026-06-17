using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class Train
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        [Precision(18, 2)]
        public decimal Price { get; set; }
        public ICollection<Trip> Trips { get; set; }
            = new List<Trip>();
        public ICollection<Booking> Bookings { get; set; } 
            = new List<Booking>();
    }
}
