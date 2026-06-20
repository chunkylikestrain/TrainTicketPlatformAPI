using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminCancelBookingRequest
    {
        [Required]
        [StringLength(300)]
        public string Reason { get; set; } = string.Empty;
    }
}
