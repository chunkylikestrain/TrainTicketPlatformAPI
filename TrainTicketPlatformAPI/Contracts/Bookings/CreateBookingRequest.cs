using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class CreateBookingRequest
    {
        [Required]
        public int TrainId { get; set; }

        public int? TripId { get; set; }

        [Required]
        public int SeatId { get; set; }

        public int? SegmentDepartureStationId { get; set; }

        public int? SegmentArrivalStationId { get; set; }

        [Required]
        public DateTime TravelDate { get; set; }

        [EmailAddress]
        public string? GuestEmail { get; set; }

        [StringLength(200)]
        public string? PassengerName { get; set; }

        [StringLength(40)]
        public string? PassengerType { get; set; }

        [StringLength(40)]
        public string? DiscountCode { get; set; }

        [Range(0, 1)]
        public int DogTicketCount { get; set; }

        [Range(0, 10)]
        public int LargeBaggageTicketCount { get; set; }
    }
}
