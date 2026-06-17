using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Payments
{
    public class ConfirmPaymentRequest
    {
        [Required]
        public string PaymentIntentId { get; set; } = string.Empty;

        [Required]
        public string PaymentMethodToken { get; set; } = string.Empty;
    }
}
