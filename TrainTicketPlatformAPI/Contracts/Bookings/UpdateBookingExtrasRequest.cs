using System.ComponentModel.DataAnnotations;

namespace TrainTicketPlatformAPI.Contracts.Bookings
{
    public class UpdateBookingExtrasRequest
    {
        [Range(0, 1)]
        public int DogTicketCount { get; set; }

        [Range(0, 10)]
        public int LargeBaggageTicketCount { get; set; }
    }
}
