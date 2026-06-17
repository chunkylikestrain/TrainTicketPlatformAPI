using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Payments
{
    public class ConfirmPaymentRequest
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string CardNumber { get; set; } = string.Empty;
    }
}
