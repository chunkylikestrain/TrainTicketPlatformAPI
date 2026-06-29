using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class TrainRoute
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int DepartureStationId { get; set; }
        public int ArrivalStationId { get; set; }

        [Precision(10, 2)]
        public decimal DistanceKm { get; set; }

        public int EstimatedDurationMinutes { get; set; }
        public string OperatingDays { get; set; } = "Daily";
        public string IntermediateStops { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string ExternalSource { get; set; } = string.Empty;
        public int? ExternalScheduleId { get; set; }
        public int? ExternalOrderId { get; set; }
        public int? ExternalTrainOrderId { get; set; }
        public DateOnly? ExternalOperatingDate { get; set; }

        public Station DepartureStation { get; set; } = null!;
        public Station ArrivalStation { get; set; } = null!;

        public ICollection<TrainRouteStop> RouteStops { get; set; }
            = new List<TrainRouteStop>();

        public ICollection<Trip> Trips { get; set; }
            = new List<Trip>();
    }
}
