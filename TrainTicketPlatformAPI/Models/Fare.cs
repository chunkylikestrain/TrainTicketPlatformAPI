using Microsoft.EntityFrameworkCore;

namespace TrainTicketPlatformAPI.Models
{
    public class Fare
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public string ClassType { get; set; } = string.Empty;

        [Precision(18, 2)]
        public decimal Price { get; set; }

        public string Currency { get; set; } = "USD";

        public Trip Trip { get; set; } = null!;
    }
}
