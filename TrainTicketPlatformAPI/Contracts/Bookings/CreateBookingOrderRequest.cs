using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class CreateBookingOrderRequest
    {
        [Required]
        public int TrainId { get; set; }

        public int? TripId { get; set; }

        public int? SegmentDepartureStationId { get; set; }

        public int? SegmentArrivalStationId { get; set; }

        [Required]
        public DateTime TravelDate { get; set; }

        [EmailAddress]
        public string? GuestEmail { get; set; }

        [StringLength(40)]
        public string? TripType { get; set; }

        [StringLength(120)]
        public string? ItineraryId { get; set; }

        public List<CreateBookingOrderJourneyRequest> Journeys { get; set; } = [];

        public List<CreateBookingOrderSegmentRequest> Segments { get; set; } = [];

        public List<CreateBookingOrderPassengerRequest> Passengers { get; set; } = [];
    }

    public class CreateBookingOrderJourneyRequest
    {
        [StringLength(40)]
        public string? Direction { get; set; }

        [MinLength(1)]
        public List<CreateBookingOrderSegmentRequest> Segments { get; set; } = [];
    }

    public class CreateBookingOrderSegmentRequest
    {
        public int SegmentIndex { get; set; }

        [Required]
        public int TrainId { get; set; }

        [Required]
        public int TripId { get; set; }

        public int? SegmentDepartureStationId { get; set; }

        public int? SegmentArrivalStationId { get; set; }

        public DateTime? TravelDate { get; set; }

        [MinLength(1)]
        public List<CreateBookingOrderPassengerRequest> Passengers { get; set; } = [];
    }

    public class CreateBookingOrderPassengerRequest
    {
        [Required]
        public int SeatId { get; set; }

        [StringLength(200)]
        public string? PassengerName { get; set; }

        [StringLength(40)]
        public string? PassengerType { get; set; }

        [StringLength(40)]
        public string? DiscountCode { get; set; }
    }
}
