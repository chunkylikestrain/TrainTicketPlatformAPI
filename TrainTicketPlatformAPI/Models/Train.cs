using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class Train
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "InterCity";
        public int CarriageCount { get; set; } = 1;
        public int SeatsPerCarriage { get; set; } = 40;
        public string Status { get; set; } = "Active";
        public string DepartureStation { get; set; } = string.Empty;
        public string ArrivalStation { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public ICollection<Trip> Trips { get; set; }
            = new List<Trip>();
        public ICollection<Booking> Bookings { get; set; } 
            = new List<Booking>();
    }
}
