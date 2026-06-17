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

        [Required]
        public DateTime TravelDate { get; set; }
    }
}
