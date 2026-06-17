using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class TrainRoute
    {
        public int Id { get; set; }
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }

        [Precision(10, 2)]
        public decimal DistanceKm { get; set; }

        public bool IsActive { get; set; } = true;

        public Station DepartureStation { get; set; } = null!;
        public Station ArrivalStation { get; set; } = null!;

        public ICollection<Trip> Trips { get; set; }
            = new List<Trip>();
    }
}
