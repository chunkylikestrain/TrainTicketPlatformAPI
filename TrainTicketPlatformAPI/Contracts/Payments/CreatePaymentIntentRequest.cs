using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Payments
{
    public class CreatePaymentIntentRequest
    {
        [Required]
        public int BookingId { get; set; }
    }
}
