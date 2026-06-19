using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class UpdateGuestBookingDataRequest
    {
        [Required]
        [EmailAddress]
        public string GuestEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string PassengerName { get; set; } = string.Empty;

        public bool AcceptedTerms { get; set; }

        public bool AcceptedMarketing { get; set; }
    }
}
