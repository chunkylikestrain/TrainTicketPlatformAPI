using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Payments
{
    public class CreatePaymentIntentRequest
    {
        public int? BookingId { get; set; }

        public int? BookingOrderId { get; set; }

        public int RedeemLoyaltyPoints { get; set; }
    }
}
