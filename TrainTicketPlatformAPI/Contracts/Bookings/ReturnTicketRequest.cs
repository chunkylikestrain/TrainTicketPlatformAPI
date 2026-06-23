using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class ReturnTicketRequest
    {
        [StringLength(300)]
        public string? Reason { get; set; }
    }
}
