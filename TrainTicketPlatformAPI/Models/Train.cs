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
        public string ExternalSource { get; set; } = string.Empty;
        public string ExternalCarrierCode { get; set; } = string.Empty;
        public string ExternalCommercialCategorySymbol { get; set; } = string.Empty;
        public string ExternalNationalNumber { get; set; } = string.Empty;
        public string ExternalInternationalArrivalNumber { get; set; } = string.Empty;
        public string ExternalInternationalDepartureNumber { get; set; } = string.Empty;
        public ICollection<Trip> Trips { get; set; }
            = new List<Trip>();
        public ICollection<TrainCarriage> Carriages { get; set; }
            = new List<TrainCarriage>();
        public ICollection<Booking> Bookings { get; set; } 
            = new List<Booking>();
    }
}
