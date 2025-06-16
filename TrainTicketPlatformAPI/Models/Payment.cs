using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        [Precision(18, 2)]
        public decimal Amount { get; set; }

        public Booking Booking { get; set; }
    }
}
